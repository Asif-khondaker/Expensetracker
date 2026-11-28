using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DailyExpenseTracker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IExpenseService _expenseService;

    public IndexModel(IExpenseService expenseService) => _expenseService = expenseService;

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public DashboardSummaryDto Summary { get; private set; } = new();
    public string? FilterError { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var from = FromDate ?? new DateTime(today.Year, today.Month, 1);
        var to = ToDate ?? today;
        try
        {
            Summary = await _expenseService.GetDashboardSummaryAsync(from, to, cancellationToken);
            FromDate = Summary.FromDate;
            ToDate = Summary.ToDate;
        }
        catch (ValidationException ex)
        {
            FilterError = ex.Message;
            FromDate = new DateTime(today.Year, today.Month, 1);
            ToDate = today;
            Summary = await _expenseService.GetDashboardSummaryAsync(FromDate.Value, ToDate.Value, cancellationToken);
        }
    }
}
