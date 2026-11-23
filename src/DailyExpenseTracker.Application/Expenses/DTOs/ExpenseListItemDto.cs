using DailyExpenseTracker.Domain.Enums;

namespace DailyExpenseTracker.Application.Expenses.DTOs;

public sealed class ExpenseListItemDto
{
    public int Id { get; init; }
    public DateTime ExpenseDate { get; init; }
    public decimal Amount { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public string? Description { get; init; }
}
