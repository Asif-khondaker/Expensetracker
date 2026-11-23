using DailyExpenseTracker.Application.Categories.DTOs;
using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpenseTracker.Infrastructure.Repositories;

public sealed class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly ExpenseDbContext _dbContext;

    public ExpenseCategoryRepository(ExpenseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoryOptionDto>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryOptionDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAndActiveAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExpenseCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && c.IsActive, cancellationToken);
    }
}
