using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;

namespace ExpenseTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// Wraps <see cref="AppDbContext"/> and exposes typed repositories.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IExpenseRepository? _expenses;

    /// <summary>
    /// Initialises a new unit of work around the given <see cref="AppDbContext"/>.
    /// </summary>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public IExpenseRepository Expenses
        => _expenses ??= new ExpenseRepository(_context);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose()
        => _context.Dispose();
}
