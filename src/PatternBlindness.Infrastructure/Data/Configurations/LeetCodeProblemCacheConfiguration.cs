using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class LeetCodeProblemCacheConfiguration : IEntityTypeConfiguration<LeetCodeProblemCache>
{
  public void Configure(EntityTypeBuilder<LeetCodeProblemCache> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.LeetCodeId)
        .HasMaxLength(50)
        .IsRequired();

    builder.Property(x => x.FrontendId)
        .HasMaxLength(20)
        .IsRequired();

    builder.Property(x => x.Title)
        .HasMaxLength(500)
        .IsRequired();

    builder.Property(x => x.TitleSlug)
        .HasMaxLength(500)
        .IsRequired();

    builder.Property(x => x.Difficulty)
        .HasMaxLength(20)
        .IsRequired();

    builder.Property(x => x.Content)
        .IsRequired();

    builder.Property(x => x.Tags)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.Examples)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.Hints)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.AcceptanceRate);

    builder.Property(x => x.CachedAt)
        .IsRequired();

    builder.Property(x => x.LastRefreshedAt)
        .IsRequired();

    // Indexes
    builder.HasIndex(x => x.TitleSlug)
        .IsUnique();

    builder.HasIndex(x => x.LeetCodeId)
        .IsUnique();

    builder.HasIndex(x => x.FrontendId);

    // Relationships
    builder.HasOne(x => x.Analysis)
        .WithOne(x => x.Problem)
        .HasForeignKey<ProblemAnalysis>(x => x.LeetCodeProblemCacheId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(x => x.Attempts)
        .WithOne(x => x.LeetCodeProblem)
        .HasForeignKey(x => x.LeetCodeProblemCacheId)
        .OnDelete(DeleteBehavior.Restrict);
  }
}
