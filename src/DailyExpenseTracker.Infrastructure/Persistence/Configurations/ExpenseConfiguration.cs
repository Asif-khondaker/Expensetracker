using DailyExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExpenseDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.CategoryId)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(Expense.MaxDescriptionLength);

        builder.Property(e => e.PaymentMethod)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        // Foreign key relation - restrict delete to protect referential integrity
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Database indexes (Section 55)
        builder.HasIndex(e => e.ExpenseDate)
            .HasDatabaseName("IX_Expenses_ExpenseDate");

        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("IX_Expenses_CategoryId");
    }
}
