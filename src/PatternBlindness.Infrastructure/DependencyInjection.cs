using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Infrastructure.Data;
using PatternBlindness.Infrastructure.Repositories;

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
        }));

    // Register repositories
    services.AddScoped<IPatternRepository, PatternRepository>();
    services.AddScoped<IProblemRepository, ProblemRepository>();
    services.AddScoped<IAttemptRepository, AttemptRepository>();

    return services;
  }
}
