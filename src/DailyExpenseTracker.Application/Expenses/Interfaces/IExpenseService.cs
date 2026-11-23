using DailyExpenseTracker.Application.Expenses.DTOs;

namespace DailyExpenseTracker.Application.Expenses.Interfaces;

public interface IExpenseService
{
    Task<int> CreateExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseDetailsDto?> GetExpenseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateExpenseAsync(UpdateExpenseRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseListItemDto>> GetExpensesAsync(CancellationToken cancellationToken = default);
}
