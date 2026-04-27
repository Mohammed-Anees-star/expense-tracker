namespace ExpenseTracker.Application.Exceptions;

/// <summary>
/// Thrown when one or more FluentValidation rules fail.
/// Maps to HTTP 422 Unprocessable Entity via the global exception handler.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initialises a new <see cref="ValidationException"/> with the collected errors.
    /// </summary>
    /// <param name="errors">Dictionary of field name → error messages.</param>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>Gets the validation errors keyed by property name.</summary>
    public IDictionary<string, string[]> Errors { get; }
}
