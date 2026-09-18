namespace DailyExpenseTracker.Domain.Entities;

public sealed class ExpenseCategory
{
    public const int MaxNameLength = 100;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Expense> Expenses { get; private set; } = new List<Expense>();

    // For EF Core
    private ExpenseCategory() { }

    public ExpenseCategory(string name, bool isActive = true)
    {
        SetName(name);
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Category name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        Name = trimmed;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
