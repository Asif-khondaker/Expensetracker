using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Categories.Services;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using DailyExpenseTracker.Application.Expenses.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DailyExpenseTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
