using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Domain.Entities;

namespace DailyExpenseTracker.Application.Expenses.Interfaces;

public interface IExpenseRepository
{
    Task<int> AddAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Expense?> GetTrackingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpensePageDto> GetPageAsync(DateTime? fromDate, DateTime? toDate, int? categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryExpenseTotalDto>> GetCategoryTotalsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseListItemDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
