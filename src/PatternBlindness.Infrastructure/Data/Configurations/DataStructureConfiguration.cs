using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Data.Configurations;

public class DataStructureConfiguration : IEntityTypeConfiguration<DataStructure>
{
  public void Configure(EntityTypeBuilder<DataStructure> builder)
  {
    builder.ToTable("DataStructures");

    builder.HasKey(ds => ds.Id);
    builder.Property(ds => ds.Id).ValueGeneratedNever();

    builder.Property(ds => ds.Name)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(ds => ds.Description)
        .HasMaxLength(2000);

    builder.Property(ds => ds.Category)
        .IsRequired()
        .HasConversion<string>()
        .HasMaxLength(50);

    builder.Property(ds => ds.WhatItIs)
        .HasColumnType("text");

    builder.Property(ds => ds.Operations)
        .HasColumnType("jsonb");

    builder.Property(ds => ds.WhenToUse)
        .HasColumnType("text");

    builder.Property(ds => ds.Tradeoffs)
        .HasColumnType("text");

    builder.Property(ds => ds.CommonUseCases)
        .HasColumnType("jsonb");

    builder.Property(ds => ds.Implementation)
        .HasColumnType("text");

    builder.Property(ds => ds.Resources)
        .HasColumnType("jsonb");

    builder.Property(ds => ds.RelatedStructureIds)
        .HasColumnType("jsonb");

    builder.Property(ds => ds.CreatedAt)
        .IsRequired();

    // Indexes
    builder.HasIndex(ds => ds.Category);
    builder.HasIndex(ds => ds.Name).IsUnique();
  }
}
