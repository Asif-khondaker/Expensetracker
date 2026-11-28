namespace DailyExpenseTracker.Application.Expenses.DTOs;

public sealed class DashboardSummaryDto
{
    public decimal TodayTotal { get; init; }
    public decimal MonthTotal { get; init; }
    public decimal SelectedPeriodTotal { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public IReadOnlyList<CategoryExpenseTotalDto> CategoryTotals { get; init; } = [];
    public IReadOnlyList<ExpenseListItemDto> RecentExpenses { get; init; } = [];
}
