using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class DataStructureConfiguration : IEntityTypeConfiguration<DataStructure>
{
  public void Configure(EntityTypeBuilder<DataStructure> builder)
  {
    builder.ToTable("DataStructures");

    builder.HasKey(d => d.Id);
    builder.Property(d => d.Id).ValueGeneratedNever();

    builder.Property(d => d.Name)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(d => d.Description)
        .IsRequired()
        .HasMaxLength(2000);

    builder.Property(d => d.Category)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(50);

    builder.Property(d => d.WhatItIs)
        .HasColumnType("text");

    builder.Property(d => d.Operations)
        .HasColumnType("jsonb");

    builder.Property(d => d.WhenToUse)
        .HasColumnType("text");

    builder.Property(d => d.Tradeoffs)
        .HasColumnType("text");

    builder.Property(d => d.CommonUseCases)
        .HasColumnType("jsonb");

    builder.Property(d => d.Implementation)
        .HasColumnType("text");

    builder.Property(d => d.Resources)
        .HasColumnType("jsonb");

    builder.Property(d => d.RelatedStructureIds)
        .HasColumnType("jsonb");

    builder.Property(d => d.CreatedAt)
        .IsRequired();

    // Indexes
    builder.HasIndex(d => d.Category);
    builder.HasIndex(d => d.Name).IsUnique();
  }
}
