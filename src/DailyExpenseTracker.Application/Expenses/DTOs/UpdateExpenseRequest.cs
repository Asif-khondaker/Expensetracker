using System.ComponentModel.DataAnnotations;
using DailyExpenseTracker.Domain.Entities;
using DailyExpenseTracker.Domain.Enums;

namespace DailyExpenseTracker.Application.Expenses.DTOs;

public sealed class UpdateExpenseRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Expense date is required.")]
    [DataType(DataType.Date)]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, (double)Expense.MaxAmount, ErrorMessage = "Amount must be greater than 0 and cannot exceed 10,000,000.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }

    [StringLength(Expense.MaxDescriptionLength, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Payment method is required.")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}
