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
            _logger.LogInformation("Starting database initialization service...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Run migrations
                _logger.LogInformation("Running database migrations...");
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations completed successfully.");

                // Seed database
                _logger.LogInformation("Starting database seeding...");
                await DatabaseSeeder.SeedAsync(dbContext);
                _logger.LogInformation("Database seeding completed successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization. The application will continue running, but data may not be properly initialized.");
            // Don't throw - we want the app to stay running even if seeding fails
            // This allows debugging via logs and health endpoints
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
