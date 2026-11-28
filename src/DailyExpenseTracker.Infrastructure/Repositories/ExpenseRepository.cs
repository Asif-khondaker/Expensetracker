using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Application.Expenses.DTOs;
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

    public async Task DeleteAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expense);

        _dbContext.Expenses.Remove(expense);
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

    public async Task<ExpensePageDto> GetPageAsync(
        DateTime? fromDate, DateTime? toDate, int? categoryId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Expenses.AsNoTracking().AsQueryable();
        if (fromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= toDate.Value);
        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        var total = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var safePage = totalPages == 0 ? 1 : Math.Clamp(pageNumber, 1, totalPages);
        var items = await query
            .OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id)
            .Skip((safePage - 1) * pageSize).Take(pageSize)
            .Select(e => new ExpenseListItemDto
            {
                Id = e.Id, ExpenseDate = e.ExpenseDate, Amount = e.Amount,
                CategoryId = e.CategoryId, CategoryName = e.Category!.Name,
                PaymentMethod = e.PaymentMethod, Description = e.Description
            }).ToListAsync(cancellationToken);
        return new ExpensePageDto { Items = items, TotalCount = total, PageNumber = safePage };
    }

    public async Task<decimal> GetTotalAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default) =>
        (await _dbContext.Expenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken)) ?? 0m;

    public async Task<IReadOnlyList<CategoryExpenseTotalDto>> GetCategoryTotalsAsync(
        DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default) =>
        await _dbContext.Expenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
            .GroupBy(e => new { e.CategoryId, e.Category!.Name })
            .Select(g => new CategoryExpenseTotalDto { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Name, Total = g.Sum(e => e.Amount) })
            .OrderByDescending(x => x.Total).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ExpenseListItemDto>> GetRecentAsync(
        int count, CancellationToken cancellationToken = default) =>
        await _dbContext.Expenses.AsNoTracking()
            .OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id).Take(count)
            .Select(e => new ExpenseListItemDto
            {
                Id = e.Id, ExpenseDate = e.ExpenseDate, Amount = e.Amount,
                CategoryId = e.CategoryId, CategoryName = e.Category!.Name,
                PaymentMethod = e.PaymentMethod, Description = e.Description
            }).ToListAsync(cancellationToken);
}
