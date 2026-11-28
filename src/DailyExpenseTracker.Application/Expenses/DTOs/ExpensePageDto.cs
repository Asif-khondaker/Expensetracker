namespace DailyExpenseTracker.Application.Expenses.DTOs;

public sealed class ExpensePageDto
{
    public IReadOnlyList<ExpenseListItemDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
}
