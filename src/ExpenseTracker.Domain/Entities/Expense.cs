using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

/// <summary>
/// Represents a single expense record in the system.
/// </summary>
public class Expense
{
    /// <summary>Unique identifier for the expense.</summary>
    public Guid Id { get; set; }

    /// <summary>Short descriptive title for the expense.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Monetary amount of the expense (in the user's base currency).</summary>
    public decimal Amount { get; set; }

    /// <summary>Category that classifies the type of expense.</summary>
    public Category Category { get; set; }

    /// <summary>The date on which the expense was incurred.</summary>
    public DateTime Date { get; set; }

    /// <summary>Identifier of the user who owns this expense.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Optional free-text notes providing additional context.</summary>
    public string? Notes { get; set; }

    /// <summary>UTC timestamp when the record was first created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the record was last modified.</summary>
    public DateTime UpdatedAt { get; set; }
}
