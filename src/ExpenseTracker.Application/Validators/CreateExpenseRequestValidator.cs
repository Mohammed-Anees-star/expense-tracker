using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Enums;
using FluentValidation;

namespace ExpenseTracker.Application.Validators;

/// <summary>
/// Validates a <see cref="CreateExpenseRequest"/> before it reaches the service layer.
/// </summary>
public sealed class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    /// <summary>Initialises all validation rules.</summary>
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount must not exceed 1,000,000.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid Category value.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MaximumLength(128).WithMessage("UserId must not exceed 128 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1,000 characters.")
            .When(x => x.Notes is not null);
    }
}
