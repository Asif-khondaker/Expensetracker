using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Domain.Entities;
using DailyExpenseTracker.Domain.Enums;

namespace DailyExpenseTracker.Application.Expenses.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<int> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateExpenseData(request.Amount, request.PaymentMethod, request.Description);

        var isCategoryValid = await _categoryRepository.ExistsAndActiveAsync(request.CategoryId, cancellationToken);
        if (!isCategoryValid)
        {
            throw new ValidationException(nameof(request.CategoryId), "Selected category does not exist or is inactive.");
        }

        var expense = new Expense(
            expenseDate: request.ExpenseDate,
            amount: request.Amount,
            categoryId: request.CategoryId,
            paymentMethod: request.PaymentMethod,
            description: request.Description);

        return await _expenseRepository.AddAsync(expense, cancellationToken);
    }

    public async Task<ExpenseDetailsDto?> GetExpenseByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense == null)
        {
            return null;
        }

        return new ExpenseDetailsDto
        {
            Id = expense.Id,
            ExpenseDate = expense.ExpenseDate,
            Amount = expense.Amount,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category?.Name ?? string.Empty,
            PaymentMethod = expense.PaymentMethod,
            Description = expense.Description,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt
        };
    }

    public async Task UpdateExpenseAsync(
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateExpenseData(request.Amount, request.PaymentMethod, request.Description);

        var isCategoryValid = await _categoryRepository.ExistsAndActiveAsync(request.CategoryId, cancellationToken);
        if (!isCategoryValid)
        {
            throw new ValidationException(nameof(request.CategoryId), "Selected category does not exist or is inactive.");
        }

        var expense = await _expenseRepository.GetTrackingByIdAsync(request.Id, cancellationToken);
        if (expense == null)
        {
            throw new ValidationException(string.Empty, "Expense not found.");
        }

        expense.Update(
            expenseDate: request.ExpenseDate,
            amount: request.Amount,
            categoryId: request.CategoryId,
            paymentMethod: request.PaymentMethod,
            description: request.Description);

        await _expenseRepository.UpdateAsync(expense, cancellationToken);
    }

    public async Task DeleteExpenseAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ValidationException(nameof(id), "Invalid expense ID.");
        }

        var expense = await _expenseRepository.GetTrackingByIdAsync(id, cancellationToken);
        if (expense == null)
        {
            throw new ValidationException(string.Empty, "Expense not found.");
        }

        await _expenseRepository.DeleteAsync(expense, cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseListItemDto>> GetExpensesAsync(
        CancellationToken cancellationToken = default)
    {
        var expenses = await _expenseRepository.GetAllAsync(cancellationToken);

        return expenses.Select(e => new ExpenseListItemDto
        {
            Id = e.Id,
            ExpenseDate = e.ExpenseDate,
            Amount = e.Amount,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? string.Empty,
            PaymentMethod = e.PaymentMethod,
            Description = e.Description
        }).ToList();
    }

    public Task<ExpensePageDto> GetExpensesPageAsync(
        DateTime? fromDate, DateTime? toDate, int? categoryId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        if (categoryId is <= 0)
            throw new ValidationException(nameof(categoryId), "Category must be a valid category.");
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1 || pageSize > 100)
            pageSize = 20;

        return _expenseRepository.GetPageAsync(fromDate?.Date, toDate?.Date, categoryId, pageNumber, pageSize, cancellationToken);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
        DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(fromDate, toDate);
        var from = fromDate.Date;
        var to = toDate.Date;
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        return new DashboardSummaryDto
        {
            TodayTotal = await _expenseRepository.GetTotalAsync(today, today, cancellationToken),
            MonthTotal = await _expenseRepository.GetTotalAsync(monthStart, today, cancellationToken),
            SelectedPeriodTotal = await _expenseRepository.GetTotalAsync(from, to, cancellationToken),
            FromDate = from,
            ToDate = to,
            CategoryTotals = await _expenseRepository.GetCategoryTotalsAsync(from, to, cancellationToken),
            RecentExpenses = await _expenseRepository.GetRecentAsync(5, cancellationToken)
        };
    }

    private static void ValidateDateRange(DateTime? fromDate, DateTime? toDate)
    {
        if (!fromDate.HasValue || !toDate.HasValue)
            return;
        if (fromDate.Value.Date > toDate!.Value.Date)
            throw new ValidationException("DateRange", "Start date must be on or before end date.");
        if ((toDate.Value.Date - fromDate.Value.Date).TotalDays > 3660)
            throw new ValidationException("DateRange", "Date range cannot exceed 10 years.");
    }

    private static void ValidateExpenseData(decimal amount, PaymentMethod paymentMethod, string? description)
    {
        if (amount <= 0)
        {
            throw new ValidationException(nameof(amount), "Amount must be greater than zero.");
        }

        if (amount > Expense.MaxAmount)
        {
            throw new ValidationException(nameof(amount), $"Amount cannot exceed {Expense.MaxAmount:N0}.");
        }

        if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethod))
        {
            throw new ValidationException(nameof(paymentMethod), "Invalid payment method selected.");
        }

        if (description != null && description.Trim().Length > Expense.MaxDescriptionLength)
        {
            throw new ValidationException(nameof(description), $"Description cannot exceed {Expense.MaxDescriptionLength} characters.");
        }
    }
}
