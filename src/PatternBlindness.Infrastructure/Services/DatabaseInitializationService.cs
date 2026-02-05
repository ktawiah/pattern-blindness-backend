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
                    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                    if (!canConnect)
                    {
                        _logger.LogError("❌ Cannot connect to database!");
                        return;
                    }
                    _logger.LogInformation("✓ Database connection successful.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to connect to database");
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
                // TEMPORARILY DISABLED: Seeding was causing database query timeouts
                // The seed data is too large (2979+ patterns with mega JSON fields)
                // TODO: Implement batch seeding, admin endpoint, or incremental seeding
                _logger.LogWarning("⚠ Database seeding is disabled - application will run without seed data");
                _logger.LogInformation("To enable seeding, create an admin endpoint to manually trigger DatabaseSeeder.SeedAsync(dbContext)");
                /*
                _logger.LogInformation("Starting database seeding at {Time}", DateTime.UtcNow);
                try
                {
                    var seedStopwatch = Stopwatch.StartNew();
                    
                    // Use a longer timeout for seeding (5 minutes)
                    using (var seedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        seedCancellation.CancelAfter(TimeSpan.FromMinutes(5));
                        await DatabaseSeeder.SeedAsync(dbContext);
                    }
                    
                    seedStopwatch.Stop();
                    _logger.LogInformation("✓ Database seeding completed in {ElapsedMs}ms at {Time}", seedStopwatch.ElapsedMilliseconds, DateTime.UtcNow);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⚠ Database seeding timed out after 5 minutes - continuing without full seed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during database seeding - continuing with partial data");
                    // Don't rethrow for seeding - allow app to continue
                }
                */
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
}
