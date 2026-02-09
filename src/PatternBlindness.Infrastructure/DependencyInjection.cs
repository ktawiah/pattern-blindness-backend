using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Interfaces;
using PatternBlindness.Infrastructure.Data;
using PatternBlindness.Infrastructure.Repositories;
using PatternBlindness.Infrastructure.Services;

namespace PatternBlindness.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services.
/// </summary>
public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
  {
    // Register DbContext
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
          npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
          npgsqlOptions.EnableRetryOnFailure(3);
        })
        // Suppress pending model changes warning since we may apply DB changes directly
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

    // Register repositories
    services.AddScoped<IPatternRepository, PatternRepository>();
    services.AddScoped<IProblemRepository, ProblemRepository>();
    services.AddScoped<IAttemptRepository, AttemptRepository>();
    services.AddScoped<ILeetCodeProblemCacheRepository, LeetCodeProblemCacheRepository>();
    services.AddScoped<IAnalysisRepository, AnalysisRepository>();
    services.AddScoped<IUserProfileRepository, UserProfileRepository>();

    // Register HTTP client and LeetCode service
    services.AddHttpClient<ILeetCodeService, LeetCodeService>(client =>
    {
      client.Timeout = TimeSpan.FromSeconds(30);
    });

    // Register LLM service
    services.AddScoped<ILlmService, OpenAiLlmService>();

    // Register pattern tracking service
    services.AddScoped<IPatternTrackingService, PatternTrackingService>();

    // Register database initialization hosted service
    // This handles migrations and seeding after app startup
    services.AddHostedService<DatabaseInitializationService>();

    return services;
  }
}
