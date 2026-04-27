using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Interfaces;

/// <summary>
/// Provides data-access operations for <see cref="Expense"/> entities.
/// </summary>
public interface IExpenseRepository
{
    /// <summary>
    /// Retrieves all expenses for the specified user, optionally filtered by category.
    /// </summary>
    /// <param name="userId">The owner's user identifier.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of matching expenses.</returns>
    Task<IReadOnlyList<Expense>> GetAllAsync(
        string userId,
        Category? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single expense by its unique identifier.
    /// </summary>
    /// <param name="id">The expense identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The expense, or <c>null</c> if not found.</returns>
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new expense to the data store.
    /// </summary>
    /// <param name="expense">The expense to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing expense in the data store.
    /// </summary>
    /// <param name="expense">The expense with updated values.</param>
    void Update(Expense expense);

    /// <summary>
    /// Removes an expense from the data store.
    /// </summary>
    /// <param name="expense">The expense to delete.</param>
    void Delete(Expense expense);

    /// <summary>
    /// Returns the sum of expenses grouped by category for a specific user.
    /// </summary>
    /// <param name="userId">The owner's user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Dictionary mapping each category to its total amount.</returns>
    Task<Dictionary<Category, decimal>> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
