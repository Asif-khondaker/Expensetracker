namespace DailyExpenseTracker.Application.Expenses.DTOs;

public sealed class CategoryExpenseTotalDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Total { get; init; }
}
