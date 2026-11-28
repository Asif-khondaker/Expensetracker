using DailyExpenseTracker.Application.Categories.DTOs;
using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Application.Expenses.Services;
using DailyExpenseTracker.Domain.Entities;
using DailyExpenseTracker.Domain.Enums;

namespace DailyExpenseTracker.Application.Tests;

public class ExpenseServiceTests
{
    private class FakeExpenseRepository : IExpenseRepository
    {
        public List<Expense> SavedExpenses { get; } = new();
        private int _nextId = 1;

        public Task<int> AddAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            // Use reflection to set Id on the entity for testing
            typeof(Expense).GetProperty(nameof(Expense.Id))?.SetValue(expense, _nextId++);
            SavedExpenses.Add(expense);
            return Task.FromResult(expense.Id);
        }

        public Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var exp = SavedExpenses.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(exp);
        }

        public Task<Expense?> GetTrackingByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var exp = SavedExpenses.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(exp);
        }

        public Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            SavedExpenses.Remove(expense);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Expense>>(SavedExpenses.AsReadOnly());
        }

        public Task<ExpensePageDto> GetPageAsync(DateTime? fromDate, DateTime? toDate, int? categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = SavedExpenses.AsEnumerable();
            if (fromDate.HasValue) query = query.Where(e => e.ExpenseDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(e => e.ExpenseDate <= toDate.Value.Date);
            if (categoryId.HasValue) query = query.Where(e => e.CategoryId == categoryId.Value);
            var matching = query.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id).ToList();
            var totalPages = (int)Math.Ceiling(matching.Count / (double)pageSize);
            var safePage = totalPages == 0 ? 1 : Math.Clamp(pageNumber, 1, totalPages);
            var items = matching.Skip((safePage - 1) * pageSize).Take(pageSize).Select(e => new ExpenseListItemDto
            {
                Id = e.Id, ExpenseDate = e.ExpenseDate, Amount = e.Amount, CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name ?? string.Empty, PaymentMethod = e.PaymentMethod, Description = e.Description
            }).ToList();
            return Task.FromResult(new ExpensePageDto { Items = items, TotalCount = matching.Count, PageNumber = safePage });
        }

        public Task<decimal> GetTotalAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default) =>
            Task.FromResult(SavedExpenses.Where(e => e.ExpenseDate >= fromDate.Date && e.ExpenseDate <= toDate.Date).Sum(e => e.Amount));

        public Task<IReadOnlyList<CategoryExpenseTotalDto>> GetCategoryTotalsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var totals = SavedExpenses.Where(e => e.ExpenseDate >= fromDate.Date && e.ExpenseDate <= toDate.Date)
                .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? string.Empty })
                .Select(g => new CategoryExpenseTotalDto { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Name, Total = g.Sum(e => e.Amount) })
                .ToList();
            return Task.FromResult<IReadOnlyList<CategoryExpenseTotalDto>>(totals);
        }

        public Task<IReadOnlyList<ExpenseListItemDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        {
            var recent = SavedExpenses.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id).Take(count)
                .Select(e => new ExpenseListItemDto { Id = e.Id, ExpenseDate = e.ExpenseDate, Amount = e.Amount, CategoryId = e.CategoryId, CategoryName = e.Category?.Name ?? string.Empty, PaymentMethod = e.PaymentMethod, Description = e.Description }).ToList();
            return Task.FromResult<IReadOnlyList<ExpenseListItemDto>>(recent);
        }
    }

    private class FakeExpenseCategoryRepository : IExpenseCategoryRepository
    {
        public HashSet<int> ActiveCategoryIds { get; } = new() { 1, 2, 3 };

        public Task<IReadOnlyList<CategoryOptionDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var list = ActiveCategoryIds.Select(id => new CategoryOptionDto { Id = id, Name = $"Category {id}" }).ToList();
            return Task.FromResult<IReadOnlyList<CategoryOptionDto>>(list);
        }

        public Task<bool> ExistsAndActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ActiveCategoryIds.Contains(id));
        }
    }

    private readonly FakeExpenseRepository _expenseRepo = new();
    private readonly FakeExpenseCategoryRepository _categoryRepo = new();
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _service = new ExpenseService(_expenseRepo, _categoryRepo);
    }

    [Fact]
    public async Task CreateExpenseAsync_WithValidData_ReturnsExpenseIdAndSavesExpense()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 500.50m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Grocery shopping"
        };

        // Act
        var resultId = await _service.CreateExpenseAsync(request);

        // Assert
        Assert.True(resultId > 0);
        Assert.Single(_expenseRepo.SavedExpenses);
        var saved = _expenseRepo.SavedExpenses[0];
        Assert.Equal(500.50m, saved.Amount);
        Assert.Equal(1, saved.CategoryId);
        Assert.Equal(PaymentMethod.Cash, saved.PaymentMethod);
        Assert.Equal("Grocery shopping", saved.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateExpenseAsync_WithZeroOrNegativeAmount_ThrowsValidationException(decimal invalidAmount)
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = invalidAmount,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateExpenseAsync(request));
        Assert.Contains("Amount must be greater than zero", ex.Message);
    }

    [Fact]
    public async Task CreateExpenseAsync_WithAmountExceedingLimit_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 10_000_001m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateExpenseAsync(request));
        Assert.Contains("cannot exceed", ex.Message);
    }

    [Fact]
    public async Task CreateExpenseAsync_WithNonexistentCategory_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 999, // Inactive / nonexistent
            PaymentMethod = PaymentMethod.Cash
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateExpenseAsync(request));
        Assert.Contains("category does not exist or is inactive", ex.Message);
    }

    [Fact]
    public async Task CreateExpenseAsync_WithTooLongDescription_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash,
            Description = new string('A', 501)
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateExpenseAsync(request));
        Assert.Contains("Description cannot exceed 500 characters", ex.Message);
    }

    [Fact]
    public async Task CreateExpenseAsync_WithInvalidPaymentMethod_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 1,
            PaymentMethod = (PaymentMethod)99
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateExpenseAsync(request));
        Assert.Contains("Invalid payment method", ex.Message);
    }

    [Fact]
    public async Task UpdateExpenseAsync_WithValidData_UpdatesExpenseSuccessfully()
    {
        // Arrange - create initial expense
        var createRequest = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today.AddDays(-1),
            Amount = 250m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Lunch"
        };
        var id = await _service.CreateExpenseAsync(createRequest);

        var updateRequest = new UpdateExpenseRequest
        {
            Id = id,
            ExpenseDate = DateTime.Today,
            Amount = 300m,
            CategoryId = 2,
            PaymentMethod = PaymentMethod.CreditCard,
            Description = "Dinner with friend"
        };

        // Act
        await _service.UpdateExpenseAsync(updateRequest);

        // Assert
        var updated = await _service.GetExpenseByIdAsync(id);
        Assert.NotNull(updated);
        Assert.Equal(300m, updated.Amount);
        Assert.Equal(2, updated.CategoryId);
        Assert.Equal(PaymentMethod.CreditCard, updated.PaymentMethod);
        Assert.Equal("Dinner with friend", updated.Description);
        Assert.Equal(DateTime.Today, updated.ExpenseDate);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task UpdateExpenseAsync_WithNonexistentExpense_ThrowsValidationException()
    {
        // Arrange
        var updateRequest = new UpdateExpenseRequest
        {
            Id = 9999,
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateExpenseAsync(updateRequest));
        Assert.Contains("Expense not found", ex.Message);
    }

    [Fact]
    public async Task UpdateExpenseAsync_WithInactiveCategory_ThrowsValidationException()
    {
        // Arrange - create initial expense
        var createRequest = new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash
        };
        var id = await _service.CreateExpenseAsync(createRequest);

        var updateRequest = new UpdateExpenseRequest
        {
            Id = id,
            ExpenseDate = DateTime.Today,
            Amount = 100m,
            CategoryId = 888, // Inactive/non-existent
            PaymentMethod = PaymentMethod.Cash
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.UpdateExpenseAsync(updateRequest));
        Assert.Contains("category does not exist or is inactive", ex.Message);
    }

    [Fact]
    public async Task GetExpensesAsync_ReturnsAllSavedExpenses()
    {
        // Arrange
        await _service.CreateExpenseAsync(new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 50m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash
        });
        await _service.CreateExpenseAsync(new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 75m,
            CategoryId = 2,
            PaymentMethod = PaymentMethod.Bank
        });

        // Act
        var list = await _service.GetExpensesAsync();

        // Assert
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task DeleteExpenseAsync_WithExistingExpense_RemovesExpense()
    {
        // Arrange
        var id = await _service.CreateExpenseAsync(new CreateExpenseRequest
        {
            ExpenseDate = DateTime.Today,
            Amount = 150m,
            CategoryId = 1,
            PaymentMethod = PaymentMethod.Cash,
            Description = "Snack"
        });

        // Act
        await _service.DeleteExpenseAsync(id);

        // Assert
        var deleted = await _service.GetExpenseByIdAsync(id);
        Assert.Null(deleted);
        Assert.Empty(_expenseRepo.SavedExpenses);
    }

    [Fact]
    public async Task DeleteExpenseAsync_WithNonexistentExpense_ThrowsValidationException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteExpenseAsync(9999));
        Assert.Contains("Expense not found", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DeleteExpenseAsync_WithInvalidId_ThrowsValidationException(int invalidId)
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.DeleteExpenseAsync(invalidId));
        Assert.Contains("Invalid expense ID", ex.Message);
    }
}
