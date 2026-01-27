using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("Attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.Confidence)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.IsPatternCorrect)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.ChosenPatternName)
            .HasMaxLength(100);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Relationship with legacy problem (nullable now)
        builder.HasOne(a => a.Problem)
            .WithMany(p => p.Attempts)
            .HasForeignKey(a => a.ProblemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relationship with LeetCode problem cache
        builder.HasOne(a => a.LeetCodeProblem)
            .WithMany(p => p.Attempts)
            .HasForeignKey(a => a.LeetCodeProblemCacheId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relationship with chosen pattern
        builder.HasOne(a => a.ChosenPattern)
            .WithMany()
            .HasForeignKey(a => a.ChosenPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-one with ColdStartSubmission
        builder.HasOne(a => a.ColdStartSubmission)
            .WithOne(c => c.Attempt)
            .HasForeignKey<ColdStartSubmission>(c => c.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one with Reflection
        builder.HasOne(a => a.Reflection)
            .WithOne(r => r.Attempt)
            .HasForeignKey<Reflection>(r => r.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        builder.Ignore(a => a.DomainEvents);

        // Indexes for common queries
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ProblemId);
        builder.HasIndex(a => a.LeetCodeProblemCacheId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.StartedAt);
        builder.HasIndex(a => new { a.UserId, a.IsPatternCorrect });
        builder.HasIndex(a => new { a.UserId, a.Confidence });
    }
}
