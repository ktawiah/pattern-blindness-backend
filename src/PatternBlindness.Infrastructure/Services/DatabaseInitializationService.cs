using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Services;

/// <summary>
/// Background service that handles database migrations and seeding after app startup.
/// This prevents startup delays and allows the app to be healthy even if seeding fails.
/// </summary>
public class DatabaseInitializationService : IHostedService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<DatabaseInitializationService> _logger;

  public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var stopwatch = Stopwatch.StartNew();
    try
    {
      _logger.LogInformation("=== Starting database initialization service at {Time} ===", DateTime.UtcNow);

      using (var scope = _serviceProvider.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Test database connection first
        _logger.LogInformation("Testing database connection...");
        try
        {
          // Log connection string info (without credentials)
          var connString = dbContext.Database.GetConnectionString() ?? "NO CONNECTION STRING FOUND";
          var sanitized = SanitizeConnectionString(connString);
          _logger.LogInformation("Using connection string: {ConnectionString}", sanitized);

          var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
          if (!canConnect)
          {
            _logger.LogError(" Cannot connect to database! Connection string: {ConnectionString}", sanitized);
            _logger.LogError("Possible causes:");
            _logger.LogError("1. Database host is unreachable");
            _logger.LogError("2. Wrong database credentials");
            _logger.LogError("3. Database doesn't exist or credentials don't have access");
            _logger.LogError("4. Network/firewall blocking connection");
            return;
          }
          _logger.LogInformation("✓ Database connection successful.");
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to connect to database. Connection error: {ErrorMessage}", ex.Message);
          return;
        }

        // Run migrations
        _logger.LogInformation("Starting database migrations...");
        try
        {
          var migrationStopwatch = Stopwatch.StartNew();
          var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
          var migrationCount = pendingMigrations.Count();

          if (migrationCount > 0)
          {
            _logger.LogInformation("Found {MigrationCount} pending migrations", migrationCount);
            await dbContext.Database.MigrateAsync(cancellationToken);
          }

          migrationStopwatch.Stop();
          _logger.LogInformation("✓ Database migrations completed in {ElapsedMs}ms", migrationStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error during database migrations");
          throw;
        }

        // Ensure missing tables and columns exist using raw SQL (safety net for incomplete migrations)
        _logger.LogInformation("Ensuring all required tables and columns exist...");
        try
        {
          var connection = dbContext.Database.GetDbConnection();
          await connection.OpenAsync(cancellationToken);

          using (var command = connection.CreateCommand())
          {
            // Use SQL to create missing tables completely if they don't exist
            var sqlScript = @"
DO $$
BEGIN
  -- Create Patterns table if it doesn't exist (required for FK integrity)
  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Patterns') THEN
    CREATE TABLE ""Patterns"" (
      ""Id"" uuid NOT NULL PRIMARY KEY,
      ""Name"" character varying(100) NOT NULL,
      ""Description"" character varying(2000) NOT NULL DEFAULT '',
      ""Category"" character varying(50) NOT NULL,
      ""WhatItIs"" text NOT NULL DEFAULT '',
      ""WhenToUse"" text NOT NULL DEFAULT '',
      ""WhyItWorks"" text NOT NULL DEFAULT '',
      ""CommonUseCases"" jsonb NOT NULL DEFAULT '[]',
      ""TimeComplexity"" character varying(100) NOT NULL DEFAULT '',
      ""SpaceComplexity"" character varying(100) NOT NULL DEFAULT '',
      ""PseudoCode"" text NOT NULL DEFAULT '',
      ""TriggerSignals"" jsonb NOT NULL DEFAULT '[]',
      ""CommonMistakes"" jsonb NOT NULL DEFAULT '[]',
      ""Resources"" jsonb NOT NULL DEFAULT '[]',
      ""RelatedPatternIds"" jsonb NOT NULL DEFAULT '[]',
      ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
      ""UpdatedAt"" timestamp with time zone
    );
    CREATE UNIQUE INDEX ""IX_Patterns_Name"" ON ""Patterns"" (""Name"");
    CREATE INDEX ""IX_Patterns_Category"" ON ""Patterns"" (""Category"");
  END IF;

  -- Create DataStructures table if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'DataStructures') THEN
    CREATE TABLE ""DataStructures"" (
      ""Id"" uuid NOT NULL PRIMARY KEY,
      ""Name"" character varying(100) NOT NULL,
      ""Description"" character varying(2000) NOT NULL DEFAULT '',
      ""Category"" character varying(50) NOT NULL,
      ""WhatItIs"" text NOT NULL DEFAULT '',
      ""Operations"" jsonb NOT NULL DEFAULT '[]',
      ""WhenToUse"" text NOT NULL DEFAULT '',
      ""Tradeoffs"" text NOT NULL DEFAULT '',
      ""CommonUseCases"" jsonb NOT NULL DEFAULT '[]',
      ""Implementation"" text NOT NULL DEFAULT '',
      ""Resources"" jsonb NOT NULL DEFAULT '[]',
      ""RelatedStructureIds"" jsonb NOT NULL DEFAULT '[]',
      ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
      ""UpdatedAt"" timestamp with time zone
    );
    CREATE UNIQUE INDEX ""IX_DataStructures_Name"" ON ""DataStructures"" (""Name"");
    CREATE INDEX ""IX_DataStructures_Category"" ON ""DataStructures"" (""Category"");
  END IF;

  -- Restore FK constraints to Patterns if they were dropped
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Problems_Patterns_CorrectPatternId') THEN
    ALTER TABLE ""Problems"" ADD CONSTRAINT ""FK_Problems_Patterns_CorrectPatternId""
      FOREIGN KEY (""CorrectPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_WrongApproaches_Patterns_WrongPatternId') THEN
    ALTER TABLE ""WrongApproaches"" ADD CONSTRAINT ""FK_WrongApproaches_Patterns_WrongPatternId""
      FOREIGN KEY (""WrongPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_ColdStartSubmissions_Patterns_ChosenPatternId') THEN
    ALTER TABLE ""ColdStartSubmissions"" ADD CONSTRAINT ""FK_ColdStartSubmissions_Patterns_ChosenPatternId""
      FOREIGN KEY (""ChosenPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_ColdStartSubmissions_Patterns_SecondaryPatternId') THEN
    ALTER TABLE ""ColdStartSubmissions"" ADD CONSTRAINT ""FK_ColdStartSubmissions_Patterns_SecondaryPatternId""
      FOREIGN KEY (""SecondaryPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_ColdStartSubmissions_Patterns_RejectedPatternId') THEN
    ALTER TABLE ""ColdStartSubmissions"" ADD CONSTRAINT ""FK_ColdStartSubmissions_Patterns_RejectedPatternId""
      FOREIGN KEY (""RejectedPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Attempts_Patterns_ChosenPatternId') THEN
    ALTER TABLE ""Attempts"" ADD CONSTRAINT ""FK_Attempts_Patterns_ChosenPatternId""
      FOREIGN KEY (""ChosenPatternId"") REFERENCES ""Patterns""(""Id"") ON DELETE RESTRICT;
  END IF;

  -- Create LeetCodeProblemCache table if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'LeetCodeProblemCache') THEN
    CREATE TABLE ""LeetCodeProblemCache"" (
      ""Id"" uuid NOT NULL PRIMARY KEY,
      ""LeetCodeId"" character varying(50) NOT NULL,
      ""FrontendId"" character varying(20) NOT NULL,
      ""Title"" character varying(500) NOT NULL,
      ""TitleSlug"" character varying(500) NOT NULL,
      ""Difficulty"" character varying(20) NOT NULL,
      ""Content"" text NOT NULL,
      ""Tags"" jsonb DEFAULT '[]',
      ""Examples"" jsonb DEFAULT '[]',
      ""Hints"" jsonb DEFAULT '[]',
      ""AcceptanceRate"" double precision,
      ""CachedAt"" timestamp with time zone NOT NULL,
      ""LastRefreshedAt"" timestamp with time zone NOT NULL,
      ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
      ""UpdatedAt"" timestamp with time zone
    );
  END IF;

  -- Create ProblemAnalyses table if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ProblemAnalyses') THEN
    CREATE TABLE ""ProblemAnalyses"" (
      ""Id"" uuid NOT NULL PRIMARY KEY,
      ""LeetCodeProblemCacheId"" uuid,
      ""PrimaryPatterns"" jsonb DEFAULT '[]',
      ""SecondaryPatterns"" jsonb DEFAULT '[]',
      ""KeySignals"" jsonb DEFAULT '[]',
      ""CommonMistakes"" jsonb DEFAULT '[]',
      ""TimeComplexity"" character varying(100),
      ""SpaceComplexity"" character varying(100),
      ""KeyInsight"" character varying(2000),
      ""ApproachExplanation"" text,
      ""SimilarProblems"" jsonb DEFAULT '[]',
      ""ModelUsed"" character varying(100),
      ""AnalyzedAt"" timestamp with time zone,
      ""RawLlmResponse"" text,
      ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
      ""UpdatedAt"" timestamp with time zone
    );
  END IF;

  -- Create Reflections table if it doesn't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Reflections') THEN
    CREATE TABLE ""Reflections"" (
      ""Id"" uuid NOT NULL PRIMARY KEY,
      ""AttemptId"" uuid,
      ""UserColdStartSummary"" character varying(2000),
      ""WasPatternCorrect"" boolean DEFAULT false,
      ""Feedback"" text,
      ""CorrectIdentifications"" jsonb DEFAULT '[]',
      ""MissedSignals"" jsonb DEFAULT '[]',
      ""NextTimeAdvice"" character varying(2000),
      ""PatternTips"" character varying(2000),
      ""ConfidenceCalibration"" character varying(1000),
      ""ModelUsed"" character varying(100),
      ""GeneratedAt"" timestamp with time zone,
      ""RawLlmResponse"" text,
      ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
      ""UpdatedAt"" timestamp with time zone
    );
  END IF;

  -- Add missing columns to Attempts if they don't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Attempts' AND column_name = 'LeetCodeProblemCacheId') THEN
    ALTER TABLE ""Attempts"" ADD COLUMN ""LeetCodeProblemCacheId"" uuid;
  END IF;
  
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Attempts' AND column_name = 'ChosenPatternName') THEN
    ALTER TABLE ""Attempts"" ADD COLUMN ""ChosenPatternName"" character varying(100);
  END IF;
  
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Attempts' AND column_name = 'ChosenPatternId') THEN
    ALTER TABLE ""Attempts"" ADD COLUMN ""ChosenPatternId"" uuid;
  END IF;
  
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Attempts' AND column_name = 'ProblemId') THEN
    ALTER TABLE ""Attempts"" ADD COLUMN ""ProblemId"" uuid;
  END IF;

  -- Ensure ProblemId is nullable (FK is optional in EF config)
  ALTER TABLE ""Attempts"" ALTER COLUMN ""ProblemId"" DROP NOT NULL;

  -- Add missing columns to ColdStartSubmissions if they don't exist
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ColdStartSubmissions' AND column_name = 'IdentifiedSignals') THEN
    ALTER TABLE ""ColdStartSubmissions"" ADD COLUMN ""IdentifiedSignals"" jsonb DEFAULT '[]';
  END IF;

  -- Add missing columns to LeetCodeProblemCache if table exists
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'LeetCodeProblemCache' AND column_name = 'CreatedAt') THEN
    ALTER TABLE ""LeetCodeProblemCache"" ADD COLUMN ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'LeetCodeProblemCache' AND column_name = 'UpdatedAt') THEN
    ALTER TABLE ""LeetCodeProblemCache"" ADD COLUMN ""UpdatedAt"" timestamp with time zone;
  END IF;

  -- Add missing columns to ProblemAnalyses if table exists
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ProblemAnalyses' AND column_name = 'CreatedAt') THEN
    ALTER TABLE ""ProblemAnalyses"" ADD COLUMN ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ProblemAnalyses' AND column_name = 'UpdatedAt') THEN
    ALTER TABLE ""ProblemAnalyses"" ADD COLUMN ""UpdatedAt"" timestamp with time zone;
  END IF;

  -- Add missing columns to Reflections if table exists
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Reflections' AND column_name = 'CreatedAt') THEN
    ALTER TABLE ""Reflections"" ADD COLUMN ""CreatedAt"" timestamp with time zone DEFAULT CURRENT_TIMESTAMP;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Reflections' AND column_name = 'UpdatedAt') THEN
    ALTER TABLE ""Reflections"" ADD COLUMN ""UpdatedAt"" timestamp with time zone;
  END IF;
END $$;
";

            command.CommandText = sqlScript;
            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("✓ Database schema safety check completed successfully");
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "⚠ Error during schema safety check - this may cause API errors");
        }

        // Seed database (with timeout protection)
        _logger.LogInformation("Starting database seeding at {Time}", DateTime.UtcNow);
        try
        {
          var seedStopwatch = Stopwatch.StartNew();


          using (var seedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
          {
            seedCancellation.CancelAfter(TimeSpan.FromMinutes(10));
            await DatabaseSeeder.SeedAsync(dbContext);
          }

          seedStopwatch.Stop();
          _logger.LogInformation("✓ Database seeding completed in {ElapsedMs}ms at {Time}", seedStopwatch.ElapsedMilliseconds, DateTime.UtcNow);
        }
        catch (OperationCanceledException)
        {
          _logger.LogWarning("⚠ Database seeding timed out after 10 minutes - continuing without full seed");
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error during database seeding - continuing with partial data");
          // Don't rethrow for seeding - allow app to continue
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Critical error during database initialization. The application will continue running.");
      // Don't throw - we want the app to stay running even if migrations fail
    }
    finally
    {
      stopwatch.Stop();
      _logger.LogInformation("=== Database initialization completed in {ElapsedMs}ms ===", stopwatch.ElapsedMilliseconds);
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  /// <summary>
  /// Sanitize connection string for logging by removing credentials
  /// </summary>
  private static string SanitizeConnectionString(string connectionString)
  {
    if (string.IsNullOrEmpty(connectionString))
      return "EMPTY";

    // Remove password/credentials from log output for security
    var sanitized = System.Text.RegularExpressions.Regex.Replace(
        connectionString,
        @"Password=([^;]*)",
        "Password=***REDACTED***",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return sanitized;
  }
}
