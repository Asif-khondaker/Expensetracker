using DailyExpenseTracker.Application.Categories.DTOs;
using DailyExpenseTracker.Application.Categories.Interfaces;

namespace DailyExpenseTracker.Application.Categories.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly IExpenseCategoryRepository _categoryRepository;

    public CategoryService(IExpenseCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryOptionDto>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetActiveCategoriesAsync(cancellationToken);
    }
}
