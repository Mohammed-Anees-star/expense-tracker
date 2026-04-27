using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Mappings;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Application.Validators;
using FluentValidation;

namespace ExpenseTracker.API.Extensions;

/// <summary>
/// Extension methods for registering Application-layer services.
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers AutoMapper, FluentValidation validators, and application services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ExpenseMappingProfile).Assembly);

        services.AddScoped<IValidator<CreateExpenseRequest>, CreateExpenseRequestValidator>();
        services.AddScoped<IValidator<UpdateExpenseRequest>, UpdateExpenseRequestValidator>();

        services.AddScoped<IExpenseService, ExpenseService>();

        return services;
    }
}
