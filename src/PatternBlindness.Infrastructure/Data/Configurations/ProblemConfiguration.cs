using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
  public void Configure(EntityTypeBuilder<Problem> builder)
  {
    builder.ToTable("Problems");

    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id).ValueGeneratedNever();

    builder.Property(p => p.Title)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(p => p.Description)
        .IsRequired()
        .HasMaxLength(10000);

    builder.Property(p => p.Difficulty)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(20);

    builder.Property(p => p.Signals)
        .HasColumnType("jsonb");

    builder.Property(p => p.Constraints)
        .HasColumnType("jsonb");

    builder.Property(p => p.Examples)
        .HasColumnType("jsonb");

    builder.Property(p => p.KeyInvariant)
        .HasMaxLength(1000);

    builder.Property(p => p.SolutionExplanation)
        .HasMaxLength(5000);

    builder.Property(p => p.IsActive)
        .IsRequired()
        .HasDefaultValue(true);

    builder.Property(p => p.CreatedAt)
        .IsRequired();

    // Relationship with correct pattern
    builder.HasOne(p => p.CorrectPattern)
        .WithMany(pattern => pattern.Problems)
        .HasForeignKey(p => p.CorrectPatternId)
        .OnDelete(DeleteBehavior.Restrict);

    // Owned collection of wrong approaches
    builder.HasMany(p => p.WrongApproaches)
        .WithOne(w => w.Problem)
        .HasForeignKey(w => w.ProblemId)
        .OnDelete(DeleteBehavior.Cascade);

    // Indexes
    builder.HasIndex(p => p.Difficulty);
    builder.HasIndex(p => p.IsActive);
    builder.HasIndex(p => p.CorrectPatternId);
    builder.HasIndex(p => p.Title);
  }
}
