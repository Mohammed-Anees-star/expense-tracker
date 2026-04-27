using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Provides endpoints for creating, reading, updating, and deleting expenses,
/// plus a spending-summary endpoint.
/// </summary>
[ApiController]
[Route("expenses")]
[Produces("application/json")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;
    private readonly ILogger<ExpensesController> _logger;

    /// <summary>Initialises the controller with required dependencies.</summary>
    public ExpensesController(IExpenseService service, ILogger<ExpensesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // =========================================================================
    // GET /expenses
    // =========================================================================

    /// <summary>
    /// Returns all expenses for a user, optionally filtered by category.
    /// </summary>
    /// <param name="userId">Required. The user whose expenses to retrieve.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <returns>A list of expense objects.</returns>
    /// <response code="200">Returns the list of expenses (may be empty).</response>
    /// <response code="400">userId query parameter is missing.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string userId,
        [FromQuery] Category? category = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = "userId query parameter is required."
            });

        var expenses = await _service.GetAllAsync(userId, category, cancellationToken);
        return Ok(expenses);
    }

    // =========================================================================
    // GET /expenses/summary
    // =========================================================================

    /// <summary>
    /// Returns spending totals grouped by category plus a grand total for a user.
    /// </summary>
    /// <param name="userId">Required. The user whose summary to compute.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <returns>Category totals and a grand total.</returns>
    /// <response code="200">Returns the summary.</response>
    /// <response code="400">userId query parameter is missing.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ExpenseSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = "userId query parameter is required."
            });

        var summary = await _service.GetSummaryAsync(userId, cancellationToken);
        return Ok(summary);
    }

    // =========================================================================
    // GET /expenses/{id}
    // =========================================================================

    /// <summary>
    /// Returns a single expense by its unique identifier.
    /// </summary>
    /// <param name="id">The GUID identifier of the expense.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <returns>The expense object.</returns>
    /// <response code="200">Returns the expense.</response>
    /// <response code="404">No expense found with the given id.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var expense = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(expense);
    }

    // =========================================================================
    // POST /expenses
    // =========================================================================

    /// <summary>
    /// Creates a new expense.
    /// </summary>
    /// <param name="request">The expense creation payload.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <returns>The newly created expense, with its assigned id.</returns>
    /// <response code="201">Expense created successfully.</response>
    /// <response code="422">One or more validation errors occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // =========================================================================
    // PUT /expenses/{id}
    // =========================================================================

    /// <summary>
    /// Replaces an existing expense with new values.
    /// </summary>
    /// <param name="id">The GUID identifier of the expense to update.</param>
    /// <param name="request">The updated expense payload.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <returns>The updated expense.</returns>
    /// <response code="200">Expense updated successfully.</response>
    /// <response code="404">No expense found with the given id.</response>
    /// <response code="422">One or more validation errors occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    // =========================================================================
    // DELETE /expenses/{id}
    // =========================================================================

    /// <summary>
    /// Permanently deletes an expense.
    /// </summary>
    /// <param name="id">The GUID identifier of the expense to delete.</param>
    /// <param name="cancellationToken">Injected by the framework.</param>
    /// <response code="204">Expense deleted successfully.</response>
    /// <response code="404">No expense found with the given id.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
