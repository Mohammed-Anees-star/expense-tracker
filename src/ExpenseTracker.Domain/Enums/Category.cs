namespace ExpenseTracker.Domain.Enums;

/// <summary>
/// Represents the category of an expense.
/// </summary>
public enum Category
{
    /// <summary>Food and dining expenses.</summary>
    Food = 1,

    /// <summary>Travel and transportation expenses.</summary>
    Travel = 2,

    /// <summary>Office and work-related expenses.</summary>
    Office = 3,

    /// <summary>Entertainment and leisure expenses.</summary>
    Entertainment = 4,

    /// <summary>Health and medical expenses.</summary>
    Health = 5,

    /// <summary>Miscellaneous expenses that don't fit other categories.</summary>
    Other = 6
}
