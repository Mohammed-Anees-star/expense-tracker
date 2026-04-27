using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs;

/// <summary>
/// Read model returned to API consumers for a single expense.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Title">Short descriptive title.</param>
/// <param name="Amount">Monetary amount.</param>
/// <param name="Category">Expense category.</param>
/// <param name="CategoryName">Human-readable category label.</param>
/// <param name="Date">Date the expense was incurred.</param>
/// <param name="UserId">Owner's user identifier.</param>
/// <param name="Notes">Optional additional notes.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="UpdatedAt">UTC last-modified timestamp.</param>
public record ExpenseDto(
    Guid Id,
    string Title,
    decimal Amount,
    Category Category,
    string CategoryName,
    DateTime Date,
    string UserId,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
