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
        try
        {
            _logger.LogInformation("Starting database initialization service at {Time}", DateTime.UtcNow);

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
                        _logger.LogError("Cannot connect to database!");
                        return;
                    }
                    _logger.LogInformation("Database connection successful.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to database");
                    return;
                }

                // Run migrations
                _logger.LogInformation("Starting database migrations...");
                try
                {
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Database migrations completed successfully at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during database migrations");
                    throw;
                }

                // Seed database
                _logger.LogInformation("Starting database seeding at {Time}", DateTime.UtcNow);
                try
                {
                    await DatabaseSeeder.SeedAsync(dbContext);
                    _logger.LogInformation("Database seeding completed successfully at {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during database seeding");
                    // Don't rethrow for seeding - allow app to continue
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization. The application will continue running, but data may not be properly initialized.");
            // Don't throw - we want the app to stay running even if migration fails
            // This allows debugging via logs and health endpoints
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
