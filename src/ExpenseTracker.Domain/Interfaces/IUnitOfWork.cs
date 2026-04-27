namespace ExpenseTracker.Domain.Interfaces;

/// <summary>
/// Abstracts the database transaction boundary, ensuring all repository changes
/// are committed atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Gets the expense repository.</summary>
    IExpenseRepository Expenses { get; }

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
