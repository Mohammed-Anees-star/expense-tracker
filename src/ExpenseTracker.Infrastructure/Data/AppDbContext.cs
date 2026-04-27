using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Expense Tracker application.
/// Applies all entity configurations from the same assembly.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Initialises a new instance with the supplied options.
    /// </summary>
    /// <param name="options">EF Core context options (connection string, provider, etc.).</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Gets or sets the <see cref="Expense"/> table.</summary>
    public DbSet<Expense> Expenses => Set<Expense>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
