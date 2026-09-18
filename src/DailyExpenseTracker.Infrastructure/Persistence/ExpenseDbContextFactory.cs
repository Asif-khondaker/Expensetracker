using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DailyExpenseTracker.Infrastructure.Persistence;

public sealed class ExpenseDbContextFactory : IDesignTimeDbContextFactory<ExpenseDbContext>
{
    public ExpenseDbContext CreateDbContext(string[] args)
    {
        // Check for Web project directory or current working directory
        var basePath = Directory.GetCurrentDirectory();
        var webProjectPath = Path.Combine(basePath, "src", "DailyExpenseTracker.Web");
        if (Directory.Exists(webProjectPath))
        {
            basePath = webProjectPath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Standard fallback schema configuration during migrations generation
            connectionString = "server=localhost;database=DailyExpenseTrackerDb;user=root;password=;";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ExpenseDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)),
            mySqlOptions =>
            {
                mySqlOptions.MigrationsAssembly(typeof(ExpenseDbContext).Assembly.FullName);
            });

        return new ExpenseDbContext(optionsBuilder.Options);
    }
}
