using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs;

/// <summary>
/// Request payload for updating an existing expense.
/// All fields are required; partial updates are not supported via this DTO.
/// </summary>
/// <param name="Title">Updated title (required, max 200 chars).</param>
/// <param name="Amount">Updated amount; must be greater than zero.</param>
/// <param name="Category">Updated category.</param>
/// <param name="Date">Updated expense date.</param>
/// <param name="Notes">Updated notes (optional, max 1000 chars).</param>
public record UpdateExpenseRequest(
    string Title,
    decimal Amount,
    Category Category,
    DateTime Date,
    string? Notes);
