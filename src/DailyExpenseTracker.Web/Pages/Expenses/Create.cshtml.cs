using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DailyExpenseTracker.Web.Pages.Expenses;

public class CreateModel : PageModel
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IExpenseService expenseService,
        ICategoryService categoryService,
        ILogger<CreateModel> logger)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [BindProperty]
    public CreateExpenseRequest Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; private set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; private set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDropdownOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownOptionsAsync();
            return Page();
        }

        try
        {
            var createdId = await _expenseService.CreateExpenseAsync(Input);
            _logger.LogInformation("Expense {ExpenseId} created successfully.", createdId);

            SuccessMessage = $"Expense of ৳ {Input.Amount:N2} was recorded successfully!";
            return RedirectToPage();
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
            _logger.LogError(ex, "Unexpected error occurred while creating an expense.");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving the expense. Please try again.");
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
