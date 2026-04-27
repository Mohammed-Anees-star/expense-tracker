using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IExpenseRepository"/>.
/// </summary>
internal sealed class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initialises the repository with the shared <see cref="AppDbContext"/>.
    /// </summary>
    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Expense>> GetAllAsync(
        string userId,
        Category? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId);

        if (category.HasValue)
            query = query.Where(e => e.Category == category.Value);

        return await query
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Expense expense, CancellationToken cancellationToken = default)
        => await _context.Expenses.AddAsync(expense, cancellationToken);

    /// <inheritdoc />
    public void Update(Expense expense)
        => _context.Expenses.Update(expense);

    /// <inheritdoc />
    public void Delete(Expense expense)
        => _context.Expenses.Remove(expense);

    /// <inheritdoc />
    public async Task<Dictionary<Category, decimal>> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(
                x => x.Category,
                x => x.Total,
                cancellationToken);
}
