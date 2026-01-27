using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class ReflectionConfiguration : IEntityTypeConfiguration<Reflection>
{
  public void Configure(EntityTypeBuilder<Reflection> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.AttemptId)
        .IsRequired();

    builder.Property(x => x.UserColdStartSummary)
        .HasMaxLength(2000);

    builder.Property(x => x.WasPatternCorrect)
        .IsRequired();

    builder.Property(x => x.Feedback)
        .IsRequired();

    builder.Property(x => x.CorrectIdentifications)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.MissedSignals)
        .HasColumnType("jsonb")
        .IsRequired();

    builder.Property(x => x.NextTimeAdvice)
        .HasMaxLength(2000);

    builder.Property(x => x.PatternTips)
        .HasMaxLength(2000);

    builder.Property(x => x.ConfidenceCalibration)
        .HasMaxLength(1000);

    builder.Property(x => x.ModelUsed)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.GeneratedAt)
        .IsRequired();

    builder.Property(x => x.RawLlmResponse);

    // Relationship with Attempt
    builder.HasOne(x => x.Attempt)
        .WithOne(x => x.Reflection)
        .HasForeignKey<Reflection>(x => x.AttemptId)
        .OnDelete(DeleteBehavior.Cascade);

    // Index
    builder.HasIndex(x => x.AttemptId)
        .IsUnique();
  }
}
