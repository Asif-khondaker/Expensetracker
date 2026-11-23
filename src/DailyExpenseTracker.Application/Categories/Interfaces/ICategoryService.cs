using DailyExpenseTracker.Application.Categories.DTOs;

namespace DailyExpenseTracker.Application.Categories.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryOptionDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
}
