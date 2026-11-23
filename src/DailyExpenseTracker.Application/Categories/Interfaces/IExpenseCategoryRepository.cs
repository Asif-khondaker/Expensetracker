using DailyExpenseTracker.Application.Categories.DTOs;

namespace DailyExpenseTracker.Application.Categories.Interfaces;

public interface IExpenseCategoryRepository
{
    Task<IReadOnlyList<CategoryOptionDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAndActiveAsync(int id, CancellationToken cancellationToken = default);
}
