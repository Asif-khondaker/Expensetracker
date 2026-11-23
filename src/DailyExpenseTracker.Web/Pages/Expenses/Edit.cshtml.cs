using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyExpenseTracker.Web.Pages.Expenses;

public class EditModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IExpenseService expenseService,
        ICategoryService categoryService,
        ILogger<EditModel> logger)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [BindProperty]
    public UpdateExpenseRequest Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; private set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; private set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        Input = new UpdateExpenseRequest
        {
            Id = expense.Id,
            ExpenseDate = expense.ExpenseDate,
            Amount = expense.Amount,
            CategoryId = expense.CategoryId,
            PaymentMethod = expense.PaymentMethod,
            Description = expense.Description
        };

        await LoadDropdownOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Input.Id = id;

        if (!ModelState.IsValid)
        {
            await LoadDropdownOptionsAsync();
            return Page();
        }

        try
        {
            await _expenseService.UpdateExpenseAsync(Input);
            _logger.LogInformation("Expense {ExpenseId} updated successfully.", id);

            TempData["SuccessMessage"] = $"Expense #{id} was updated successfully!";
            return RedirectToPage("/Expenses/Index");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(
                        string.IsNullOrEmpty(error.Key) ? string.Empty : $"Input.{error.Key}",
                        message);
                }
            }

            await LoadDropdownOptionsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating expense {ExpenseId}.", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the expense. Please try again.");
            await LoadDropdownOptionsAsync();
            return Page();
        }
    }

    private async Task LoadDropdownOptionsAsync()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        CategoryOptions = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        PaymentMethodOptions = Enum.GetValues<PaymentMethod>()
            .Select(pm => new SelectListItem
            {
                Value = pm.ToString(),
                Text = pm switch
                {
                    PaymentMethod.CreditCard => "Credit Card",
                    PaymentMethod.DebitCard => "Debit Card",
                    PaymentMethod.MobileBanking => "Mobile Banking (bKash/Nagad/Rocket)",
                    _ => pm.ToString()
                }
            }).ToList();
    }
}
