using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class ProblemAnalysisConfiguration : IEntityTypeConfiguration<ProblemAnalysis>
{
  public void Configure(EntityTypeBuilder<ProblemAnalysis> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.LeetCodeProblemCacheId)
        .IsRequired();

    builder.Property(x => x.PrimaryPatterns)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.SecondaryPatterns)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.KeySignals)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.CommonMistakes)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.TimeComplexity)
        .HasMaxLength(100);

    builder.Property(x => x.SpaceComplexity)
        .HasMaxLength(100);

    builder.Property(x => x.KeyInsight)
        .HasMaxLength(2000);

    builder.Property(x => x.ApproachExplanation);

    builder.Property(x => x.SimilarProblems)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.ModelUsed)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.AnalyzedAt)
        .IsRequired();

    builder.Property(x => x.RawLlmResponse);

    // Index for finding analysis by problem
    builder.HasIndex(x => x.LeetCodeProblemCacheId)
        .IsUnique();
  }
}
