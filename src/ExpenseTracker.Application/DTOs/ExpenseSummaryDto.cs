using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs;

/// <summary>
/// Summarises spending totals per category and the overall grand total.
/// </summary>
/// <param name="ByCategory">Total amount spent in each category.</param>
/// <param name="GrandTotal">Sum of all expense amounts.</param>
public record ExpenseSummaryDto(
    IReadOnlyDictionary<string, decimal> ByCategory,
    decimal GrandTotal);
