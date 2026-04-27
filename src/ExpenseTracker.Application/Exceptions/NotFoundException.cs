namespace ExpenseTracker.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found in the data store.
/// Maps to HTTP 404 Not Found via the global exception handler.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initialises a new <see cref="NotFoundException"/> with a descriptive message.
    /// </summary>
    /// <param name="entityName">The name of the entity type that was not found.</param>
    /// <param name="id">The identifier that was searched for.</param>
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    /// <summary>Gets the entity type name.</summary>
    public string EntityName { get; }

    /// <summary>Gets the identifier that was searched for.</summary>
    public object Id { get; }
}
