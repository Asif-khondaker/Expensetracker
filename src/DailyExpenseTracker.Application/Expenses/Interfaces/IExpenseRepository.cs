using DailyExpenseTracker.Domain.Entities;

namespace DailyExpenseTracker.Application.Expenses.Interfaces;

public interface IExpenseRepository
{
    Task<int> AddAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Expense?> GetTrackingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
}
