using AutoMapper;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

/// <summary>
/// Implements expense business-logic use-cases.
/// Orchestrates the repository, AutoMapper, and FluentValidation.
/// </summary>
public sealed class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateExpenseRequest> _createValidator;
    private readonly IValidator<UpdateExpenseRequest> _updateValidator;
    private readonly ILogger<ExpenseService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ExpenseService"/>.
    /// </summary>
    public ExpenseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateExpenseRequest> createValidator,
        IValidator<UpdateExpenseRequest> updateValidator,
        ILogger<ExpenseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync(
        string userId,
        Category? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching expenses for user {UserId}, category filter: {Category}",
            userId, category?.ToString() ?? "None");

        var expenses = await _unitOfWork.Expenses.GetAllAsync(userId, category, cancellationToken);
        return _mapper.Map<IReadOnlyList<ExpenseDto>>(expenses);
    }

    /// <inheritdoc />
    public async Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching expense {ExpenseId}", id);

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), id);

        return _mapper.Map<ExpenseDto>(expense);
    }

    /// <inheritdoc />
    public async Task<ExpenseDto> CreateAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating expense for user {UserId}", request.UserId);

        await ValidateAsync(_createValidator, request, cancellationToken);

        var expense = _mapper.Map<Expense>(request);

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created expense {ExpenseId}", expense.Id);
        return _mapper.Map<ExpenseDto>(expense);
    }

    /// <inheritdoc />
    public async Task<ExpenseDto> UpdateAsync(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating expense {ExpenseId}", id);

        await ValidateAsync(_updateValidator, request, cancellationToken);

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), id);

        _mapper.Map(request, expense);
        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated expense {ExpenseId}", id);
        return _mapper.Map<ExpenseDto>(expense);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting expense {ExpenseId}", id);

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), id);

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted expense {ExpenseId}", id);
    }

    /// <inheritdoc />
    public async Task<ExpenseSummaryDto> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating expense summary for user {UserId}", userId);

        var summaryByCategory = await _unitOfWork.Expenses.GetSummaryAsync(userId, cancellationToken);

        var byCategory = summaryByCategory.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);

        var grandTotal = byCategory.Values.Sum();

        return new ExpenseSummaryDto(byCategory, grandTotal);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Exceptions.ValidationException(errors);
        }
    }
}
