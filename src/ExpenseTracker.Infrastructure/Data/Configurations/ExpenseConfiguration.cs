using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

/// <summary>
/// Fluent API configuration for the <see cref="Expense"/> entity.
/// </summary>
internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // GUID assigned by the application

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()   // stores as "Food", "Travel", etc.
            .HasMaxLength(50);

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        // Indexes to speed up the common query patterns
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Expenses_UserId");

        builder.HasIndex(e => new { e.UserId, e.Category })
            .HasDatabaseName("IX_Expenses_UserId_Category");

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("IX_Expenses_Date");
    }
}
