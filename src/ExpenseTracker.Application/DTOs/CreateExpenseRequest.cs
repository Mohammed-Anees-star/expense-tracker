using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs;

/// <summary>
/// Request payload for creating a new expense.
/// </summary>
/// <param name="Title">Short descriptive title (required, max 200 chars).</param>
/// <param name="Amount">Monetary amount; must be greater than zero.</param>
/// <param name="Category">The expense category.</param>
/// <param name="Date">Date the expense was incurred.</param>
/// <param name="UserId">Identifier of the owning user.</param>
/// <param name="Notes">Optional free-text notes (max 1000 chars).</param>
public record CreateExpenseRequest(
    string Title,
    decimal Amount,
    Category Category,
    DateTime Date,
    string UserId,
    string? Notes);
