using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DailyExpenseTracker.Web.Pages.Expenses;

public class DeleteModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<DeleteModel> _logger;

    public DeleteModel(
        IExpenseService expenseService,
        ILogger<DeleteModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public ExpenseDetailsDto? Expense { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            return RedirectToPage("/Expenses/Index");
        }

        Expense = await _expenseService.GetExpenseByIdAsync(id.Value);
        if (Expense == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (id <= 0)
        {
            TempData["ErrorMessage"] = "Invalid expense selected for deletion.";
            return RedirectToPage("/Expenses/Index");
        }

        try
        {
            await _expenseService.DeleteExpenseAsync(id);
            _logger.LogInformation("Expense {ExpenseId} deleted successfully.", id);

            TempData["SuccessMessage"] = $"Expense #{id} was deleted successfully!";
            return RedirectToPage("/Expenses/Index");
        }
        catch (ValidationException ex)
        {
            var message = ex.Errors.SelectMany(e => e.Value).FirstOrDefault() ?? "Unable to delete the expense.";
            TempData["ErrorMessage"] = message;
            return RedirectToPage("/Expenses/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting expense {ExpenseId}.", id);
            TempData["ErrorMessage"] = "An unexpected error occurred while deleting the expense. Please try again.";
            return RedirectToPage("/Expenses/Index");
        }
    }
}
