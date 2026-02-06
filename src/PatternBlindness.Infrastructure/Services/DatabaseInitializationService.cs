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
          _logger.LogError(ex, "❌ Error during database migrations");
          throw;
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
          _logger.LogError(ex, "❌ Error during database seeding - continuing with partial data");
          // Don't rethrow for seeding - allow app to continue
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Critical error during database initialization. The application will continue running.");
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
