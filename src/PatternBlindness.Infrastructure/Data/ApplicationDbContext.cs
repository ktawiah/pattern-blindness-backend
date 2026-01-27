using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data;

/// <summary>
/// Application database context with ASP.NET Core Identity integration.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<Pattern> Patterns => Set<Pattern>();
  public DbSet<Problem> Problems => Set<Problem>();
  public DbSet<WrongApproach> WrongApproaches => Set<WrongApproach>();
  public DbSet<Attempt> Attempts => Set<Attempt>();
  public DbSet<ColdStartSubmission> ColdStartSubmissions => Set<ColdStartSubmission>();
  public DbSet<LeetCodeProblemCache> LeetCodeProblemCache => Set<LeetCodeProblemCache>();
  public DbSet<ProblemAnalysis> ProblemAnalyses => Set<ProblemAnalysis>();
  public DbSet<Reflection> Reflections => Set<Reflection>();
  public DbSet<DataStructure> DataStructures => Set<DataStructure>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply all entity configurations from this assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    // Ignore the DomainEvents navigation property on all entities
    // Domain events are not persisted to the database
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      var clrType = entityType.ClrType;
      if (clrType.IsAssignableTo(typeof(Domain.Common.Entity)))
      {
        modelBuilder.Entity(clrType).Ignore("DomainEvents");
      }
    }

    // Configure Identity tables with custom schema/names if needed
    modelBuilder.Entity<ApplicationUser>(entity =>
    {
      entity.ToTable("Users");
    });

    modelBuilder.Entity<IdentityRole>(entity =>
    {
      entity.ToTable("Roles");
    });

    modelBuilder.Entity<IdentityUserRole<string>>(entity =>
    {
      entity.ToTable("UserRoles");
    });

    modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
    {
      entity.ToTable("UserClaims");
    });

    modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
    {
      entity.ToTable("UserLogins");
    });

    modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
    {
      entity.ToTable("RoleClaims");
    });

    modelBuilder.Entity<IdentityUserToken<string>>(entity =>
    {
      entity.ToTable("UserTokens");
    });
  }

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    UpdateTimestamps();
    return base.SaveChangesAsync(cancellationToken);
  }

  private void UpdateTimestamps()
  {
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified);

    foreach (var entry in entries)
    {
      if (entry.Entity is Domain.Common.Entity entity)
      {
        // Use reflection to set UpdatedAt since it has a private setter
        var property = entry.Property("UpdatedAt");
        property.CurrentValue = DateTime.UtcNow;
      }
    }
  }
}
