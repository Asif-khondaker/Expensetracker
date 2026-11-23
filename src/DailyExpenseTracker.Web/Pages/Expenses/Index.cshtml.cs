using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DailyExpenseTracker.Web.Pages.Expenses;

public class IndexModel : PageModel
{
    private readonly IExpenseService _expenseService;

    public IndexModel(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    public IReadOnlyList<ExpenseListItemDto> Expenses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Expenses = await _expenseService.GetExpensesAsync();
    }
}
