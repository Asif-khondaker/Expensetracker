using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Domain.Entities;
using DailyExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpenseTracker.Infrastructure.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext _dbContext;

    public ExpenseRepository(ExpenseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);

        await _dbContext.Expenses.AddAsync(expense, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return expense.Id;
    }

    public async Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Expense?> GetTrackingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
            .ToListAsync(cancellationToken);
    }
}
