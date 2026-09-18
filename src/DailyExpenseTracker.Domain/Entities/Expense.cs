using DailyExpenseTracker.Domain.Enums;

namespace DailyExpenseTracker.Domain.Entities;

public sealed class Expense
{
    public const decimal MaxAmount = 10_000_000m;
    public const int MaxDescriptionLength = 500;

    public int Id { get; private set; }
    public DateTime ExpenseDate { get; private set; }
    public decimal Amount { get; private set; }
    public int CategoryId { get; private set; }
    public ExpenseCategory? Category { get; private set; }
    public string? Description { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // For EF Core
    private Expense() { }

    public Expense(
        DateTime expenseDate,
        decimal amount,
        int categoryId,
        PaymentMethod paymentMethod,
        string? description = null)
    {
        ValidateAmount(amount);
        ValidateCategoryId(categoryId);
        ValidateDescription(description);

        ExpenseDate = expenseDate.Date;
        Amount = amount;
        CategoryId = categoryId;
        PaymentMethod = paymentMethod;
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        DateTime expenseDate,
        decimal amount,
        int categoryId,
        PaymentMethod paymentMethod,
        string? description)
    {
        ValidateAmount(amount);
        ValidateCategoryId(categoryId);
        ValidateDescription(description);

        ExpenseDate = expenseDate.Date;
        Amount = amount;
        CategoryId = categoryId;
        PaymentMethod = paymentMethod;
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (amount > MaxAmount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount cannot exceed {MaxAmount:N0}.");
        }
    }

    private static void ValidateCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(categoryId), "A valid category must be selected.");
        }
    }

    private static void ValidateDescription(string? description)
    {
        if (description != null && description.Trim().Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
        }
    }
}
