using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(up => up.Id);
        builder.Property(up => up.Id).ValueGeneratedNever();

        builder.Property(up => up.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(up => up.DsaProblemsCompleted)
            .IsRequired();

        builder.Property(up => up.IsQualified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(up => up.CurrentPhase)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(up => up.CompletedAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(up => up.WasGrandfathered)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(up => up.InterviewReadinessOptIn)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(up => up.CreatedAt)
            .IsRequired();

        // Unique constraint on UserId
        builder.HasIndex(up => up.UserId)
            .IsUnique();

        // Index for qualification queries
        builder.HasIndex(up => up.IsQualified);

        // Ignore domain events (not persisted)
        builder.Ignore(up => up.DomainEvents);
    }
}
