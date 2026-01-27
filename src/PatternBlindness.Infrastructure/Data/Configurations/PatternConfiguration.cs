using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class PatternConfiguration : IEntityTypeConfiguration<Pattern>
{
    public void Configure(EntityTypeBuilder<Pattern> builder)
    {
        builder.ToTable("Patterns");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.WhatItIs)
            .HasColumnType("text");

        builder.Property(p => p.WhenToUse)
            .HasColumnType("text");

        builder.Property(p => p.WhyItWorks)
            .HasColumnType("text");

        builder.Property(p => p.CommonUseCases)
            .HasColumnType("jsonb");

        builder.Property(p => p.TimeComplexity)
            .HasMaxLength(100);

        builder.Property(p => p.SpaceComplexity)
            .HasMaxLength(100);

        builder.Property(p => p.PseudoCode)
            .HasColumnType("text");

        builder.Property(p => p.TriggerSignals)
            .HasColumnType("jsonb");

        builder.Property(p => p.CommonMistakes)
            .HasColumnType("jsonb");

        builder.Property(p => p.Resources)
            .HasColumnType("jsonb");

        builder.Property(p => p.RelatedPatternIds)
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Index for category searches
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.Name).IsUnique();
    }
}
