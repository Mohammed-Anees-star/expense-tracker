using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces;

/// <summary>
/// Application-level service for managing expense use-cases.
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// Returns all expenses for a user, optionally filtered by category.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync(
        string userId,
        Category? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single expense by id.
    /// </summary>
    /// <param name="id">The expense identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the expense does not exist.</exception>
    Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new expense.
    /// </summary>
    /// <param name="request">The creation request payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created expense DTO.</returns>
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    /// <param name="id">The id of the expense to update.</param>
    /// <param name="request">The update request payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated expense DTO.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the expense does not exist.</exception>
    Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing expense.
    /// </summary>
    /// <param name="id">The id of the expense to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the expense does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns spending totals grouped by category plus a grand total.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<ExpenseSummaryDto> GetSummaryAsync(string userId, CancellationToken cancellationToken = default);
}
