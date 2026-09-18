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
            .AddUserSecrets("ebbb9a83-208c-4592-843e-eaecb9525780")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. Configure it securely using .NET User Secrets: " +
                "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<your-connection-string>\" --project src/DailyExpenseTracker.Web/DailyExpenseTracker.Web.csproj");
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
