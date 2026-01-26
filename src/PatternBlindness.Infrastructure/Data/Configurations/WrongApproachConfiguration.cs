using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class WrongApproachConfiguration : IEntityTypeConfiguration<WrongApproach>
{
  public void Configure(EntityTypeBuilder<WrongApproach> builder)
  {
    builder.ToTable("WrongApproaches");

    builder.HasKey(w => w.Id);
    builder.Property(w => w.Id).ValueGeneratedNever();

    builder.Property(w => w.Explanation)
        .IsRequired()
        .HasMaxLength(2000);

    builder.Property(w => w.FrequencyPercent)
        .IsRequired();

    builder.Property(w => w.CreatedAt)
        .IsRequired();

    // Relationship with wrong pattern
    builder.HasOne(w => w.WrongPattern)
        .WithMany()
        .HasForeignKey(w => w.WrongPatternId)
        .OnDelete(DeleteBehavior.Restrict);

    // Indexes
    builder.HasIndex(w => w.ProblemId);
    builder.HasIndex(w => w.WrongPatternId);
    builder.HasIndex(w => w.FrequencyPercent);
  }
}
