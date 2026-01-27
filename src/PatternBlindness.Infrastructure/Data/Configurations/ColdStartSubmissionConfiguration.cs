using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class ColdStartSubmissionConfiguration : IEntityTypeConfiguration<ColdStartSubmission>
{
    public void Configure(EntityTypeBuilder<ColdStartSubmission> builder)
    {
        builder.ToTable("ColdStartSubmissions");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.SubmittedAt)
            .IsRequired();

        builder.Property(c => c.ThinkingDurationSeconds)
            .IsRequired();

        builder.Property(c => c.IdentifiedSignals)
            .HasColumnType("text");

        builder.Property(c => c.RejectionReason)
            .HasMaxLength(1000);

        // Multiple hypothesis mode fields
        builder.Property(c => c.PrimaryVsSecondaryReason)
            .HasMaxLength(1000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Relationship with chosen pattern (primary)
        builder.HasOne(c => c.ChosenPattern)
            .WithMany()
            .HasForeignKey(c => c.ChosenPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with secondary pattern (optional, for multiple hypothesis mode)
        builder.HasOne(c => c.SecondaryPattern)
            .WithMany()
            .HasForeignKey(c => c.SecondaryPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with rejected pattern (optional)
        builder.HasOne(c => c.RejectedPattern)
            .WithMany()
            .HasForeignKey(c => c.RejectedPatternId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events (not persisted)
        builder.Ignore(c => c.DomainEvents);

        // Indexes
        builder.HasIndex(c => c.AttemptId).IsUnique();
        builder.HasIndex(c => c.ChosenPatternId);
        builder.HasIndex(c => c.SecondaryPatternId);
    }
}
