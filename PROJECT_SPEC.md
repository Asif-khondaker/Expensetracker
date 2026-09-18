# Daily Expense Tracker — Project Specification

## 1. Project Goal

Build a simple, secure **Daily Expense Tracker** for recording personal daily expenses and viewing summaries.

The project is intentionally small and should be used as a learning project for:

- Codex / agentic development
- .NET 10
- Clean Architecture
- Entity Framework Core
- MySQL
- Secure database access
- Testing
- Git-based development

Do not over-engineer the application.

---

# 2. Technology Stack

Use:

- .NET 10
- ASP.NET Core
- Razor Pages or MVC
- Clean Architecture
- Entity Framework Core
- MySQL
- Pomelo.EntityFrameworkCore.MySql
- Bootstrap
- Dependency Injection
- Async/await
- Git

Database:

```text
MySQL
Database: DailyExpenseTrackerDb
```

---

# 3. Core Features

The application should initially support:

1. Add an expense.
2. Edit an expense.
3. Delete an expense.
4. View expenses.
5. Filter expenses by date.
6. Filter expenses by category.
7. Paginate expense records.
8. View today's total.
9. View monthly total.
10. View category-wise totals.
11. View recent expenses.
12. View a summary for a custom date range.

Authentication and multi-user support can be added later as a separate phase.

---

# 4. Clean Architecture

Use this solution structure:

```text
DailyExpenseTracker/
│
├── src/
│   ├── DailyExpenseTracker.Domain/
│   ├── DailyExpenseTracker.Application/
│   ├── DailyExpenseTracker.Infrastructure/
│   └── DailyExpenseTracker.Web/
│
├── tests/
│   ├── DailyExpenseTracker.Application.Tests/
│   └── DailyExpenseTracker.IntegrationTests/
│
├── docs/
│
├── .gitignore
├── README.md
└── PROJECT_SPEC.md
```

Dependency direction:

```text
                 ┌─────────────────────┐
                 │   Web / Presentation│
                 └──────────┬──────────┘
                            │
                            ▼
                 ┌─────────────────────┐
                 │     Application     │
                 └──────────┬──────────┘
                            │
                            ▼
                 ┌─────────────────────┐
                 │       Domain       │
                 └─────────────────────┘

                 Infrastructure
                       │
                       ▼
                  Application
                       │
                       ▼
                     Domain
```

Rules:

- Domain must not depend on Infrastructure.
- Domain must not depend on Web.
- Application must not depend on Web.
- Application should not depend on Infrastructure implementations.
- Infrastructure implements interfaces required by Application.
- Web is responsible for presentation and HTTP/UI concerns.

---

# 5. Domain Layer

The Domain project contains:

- Entities
- Enums
- Domain rules
- Value objects only when genuinely useful

Suggested structure:

```text
DailyExpenseTracker.Domain/
│
├── Entities/
│   ├── Expense.cs
│   └── ExpenseCategory.cs
│
└── Enums/
    └── PaymentMethod.cs
```

Do not put:

- EF Core attributes unnecessarily
- SQL queries
- ASP.NET Core code
- UI logic
- database connection strings

inside the Domain project.

---

# 6. Expense Entity

Create an `Expense` entity.

Suggested properties:

```text
Id
ExpenseDate
Amount
CategoryId
Description
PaymentMethod
CreatedAt
UpdatedAt
```

Use a numeric database ID initially.

Use:

```text
decimal
```

for money.

Do NOT use:

```text
float
double
```

for monetary amounts.

Database precision:

```text
decimal(18,2)
```

---

# 7. Expense Category

Create:

```text
ExpenseCategory
```

Suggested properties:

```text
Id
Name
IsActive
CreatedAt
```

Initial categories can include:

```text
Food
Transport
Shopping
Bills
Medical
Education
Entertainment
Rent
Family
Other
```

Categories should be stored in MySQL.

Do not duplicate category names throughout the application code.

---

# 8. Payment Method

Use an enum initially:

```csharp
Cash
Bank
CreditCard
DebitCard
MobileBanking
Other
```

Do not create a database table for payment methods unless there is a genuine requirement for dynamically managing them.

---

# 9. Application Layer

Suggested structure:

```text
DailyExpenseTracker.Application/
│
├── Common/
│
├── Expenses/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
└── Categories/
    ├── DTOs/
    ├── Interfaces/
    └── Services/
```

Possible interfaces:

```text
IExpenseService
IExpenseRepository
IExpenseCategoryRepository
```

Keep the interfaces focused.

Do not create abstractions simply because a tutorial uses them.

---

# 10. DTOs

Do not bind database entities directly to UI forms.

Use DTOs.

Suggested DTOs:

```text
CreateExpenseRequest
UpdateExpenseRequest
ExpenseListItemDto
ExpenseDetailsDto
ExpenseSummaryDto
CategoryDto
```

Example:

```csharp
public sealed class CreateExpenseRequest
{
    public DateTime ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
}
```

The Web layer communicates with Application services using DTOs.

---

# 11. Infrastructure Layer

Infrastructure contains:

- EF Core DbContext
- Entity configurations
- Repository implementations
- MySQL configuration
- EF Core migrations

Suggested structure:

```text
DailyExpenseTracker.Infrastructure/
│
├── Persistence/
│   ├── ExpenseDbContext.cs
│   ├── Configurations/
│   │   ├── ExpenseConfiguration.cs
│   │   └── ExpenseCategoryConfiguration.cs
│   │
│   └── Migrations/
│
└── Repositories/
    ├── ExpenseRepository.cs
    └── ExpenseCategoryRepository.cs
```

---

# 12. MySQL Configuration

Use:

```text
Pomelo.EntityFrameworkCore.MySql
```

The package version must be compatible with the project's EF Core version.

Before installing packages:

1. Inspect the current `.csproj` files.
2. Confirm the .NET version.
3. Confirm the EF Core version.
4. Select a compatible Pomelo version.
5. Do not blindly install unrelated packages.

Example configuration:

```csharp
builder.Services.AddDbContext<ExpenseDbContext>(options =>
{
    var connectionString =
        configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString));
});
```

Do not hard-code the connection string.

---

# 13. MySQL Connection String Security

Never put real database credentials into source code.

Never commit:

```text
username
password
API keys
tokens
private keys
production credentials
```

Do not write:

```csharp
options.UseMySql(
    "server=localhost;database=DailyExpenseTrackerDb;user=root;password=123456",
    ...);
```

Use configuration and User Secrets for local development.

Example:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;database=DailyExpenseTrackerDb;user=YOUR_USER;password=YOUR_PASSWORD;"
```

The actual password must never appear in:

- PROJECT_SPEC.md
- README.md
- Git repository
- source code
- screenshots
- logs

For production, use environment variables or an appropriate secret-management system.

---

# 14. MySQL Database

Database:

```text
DailyExpenseTrackerDb
```

Initial tables:

```text
ExpenseCategories
Expenses
```

Relationship:

```text
ExpenseCategories
       │
       │ 1
       │
       │ *
       ▼
    Expenses
```

Foreign key:

```text
Expenses.CategoryId
    ->
ExpenseCategories.Id
```

Use appropriate foreign-key constraints.

Avoid cascade delete unless there is a specific business requirement.

---

# 15. Database Design

Suggested `ExpenseCategories`:

```text
Id              INT
Name            VARCHAR(100)
IsActive        BOOLEAN
CreatedAt       DATETIME
```

Suggested `Expenses`:

```text
Id              INT
ExpenseDate     DATE
Amount          DECIMAL(18,2)
CategoryId      INT
Description     VARCHAR(500)
PaymentMethod   INT
CreatedAt       DATETIME
UpdatedAt       DATETIME NULL
```

Exact SQL types should be configured through EF Core entity configuration.

Do not rely on conventions for important money/string constraints.

---

# 16. Entity Configuration

Prefer Fluent API configurations.

Example:

```csharp
builder.Property(x => x.Amount)
    .HasPrecision(18, 2)
    .IsRequired();

builder.Property(x => x.Description)
    .HasMaxLength(500);

builder.Property(x => x.ExpenseDate)
    .IsRequired();
```

Configure relationships explicitly where useful.

---

# 17. SQL Injection Protection

SQL injection must be prevented.

Prefer EF Core LINQ:

```csharp
var expenses = await dbContext.Expenses
    .AsNoTracking()
    .Where(x => x.ExpenseDate >= fromDate &&
                x.ExpenseDate <= toDate)
    .ToListAsync();
```

Do NOT build SQL by concatenating user input.

Never do:

```csharp
var sql =
    "SELECT * FROM Expenses WHERE Description = '" +
    input +
    "'";
```

If raw SQL is genuinely required:

- Use parameterized APIs.
- Never concatenate untrusted input.
- Keep raw SQL isolated in Infrastructure.
- Review the query before using it.

---

# 18. Server-Side Validation

Never trust client-side validation alone.

Validate on the server.

Minimum validation:

## Amount

```text
Amount > 0
Amount <= reasonable maximum
```

Example maximum:

```text
10,000,000
```

unless the actual business requirement differs.

## Category

Verify that:

```text
CategoryId exists
Category is active
```

Do not assume a submitted CategoryId is valid.

## Description

Maximum:

```text
500 characters
```

## Date Range

Require:

```text
FromDate <= ToDate
```

Do not allow unnecessarily huge date ranges.

---

# 19. Never Trust Client Input

Treat all values coming from the client as untrusted:

- Form fields
- Query strings
- Route parameters
- JSON
- Hidden fields
- Cookies controlled by the client

For example:

```html
<input type="hidden" name="Amount" value="100">
```

does not make the value trustworthy.

Always validate on the server.

---

# 20. XSS Protection

Expense descriptions are user input.

Treat them as plain text.

Do not render user input as raw HTML.

Avoid unnecessary:

```csharp
Html.Raw(...)
```

Use normal Razor HTML encoding.

---

# 21. CSRF / Antiforgery

All state-changing operations must use antiforgery protection.

Especially:

```text
Create
Update
Delete
```

Do not disable antiforgery protection to solve a form error.

If a POST returns HTTP 400:

1. Inspect the request.
2. Check antiforgery token.
3. Check model binding.
4. Check validation.
5. Check routing.
6. Fix the actual problem.

Do not simply remove `[ValidateAntiForgeryToken]`.

---

# 22. Delete Security

Delete must not happen through GET.

Do not implement:

```text
GET /Expenses/Delete/15
```

as an immediate deletion operation.

Use:

```text
POST /Expenses/Delete
```

with antiforgery protection.

Ask for confirmation in the UI.

---

# 23. Pagination

The expense list must support server-side pagination.

Do not load thousands of records unnecessarily.

Use:

```text
Skip()
Take()
```

or equivalent database-side pagination.

Initial page size:

```text
20
```

The page size should have a reasonable upper limit.

---

# 24. EF Core Query Rules

For read-only queries prefer:

```csharp
AsNoTracking()
```

Example:

```csharp
var expenses = await dbContext.Expenses
    .AsNoTracking()
    .OrderByDescending(x => x.ExpenseDate)
    .ThenByDescending(x => x.Id)
    .ToListAsync();
```

Use tracking when entities are intentionally modified.

---

# 25. Async Database Operations

Use asynchronous EF Core methods:

```text
ToListAsync()
FirstOrDefaultAsync()
SingleOrDefaultAsync()
AnyAsync()
CountAsync()
SaveChangesAsync()
```

Avoid synchronous database calls inside normal HTTP request handling.

---

# 26. Efficient Summary Queries

Summary calculations should happen in the database.

Prefer:

```csharp
await dbContext.Expenses
    .Where(...)
    .SumAsync(x => x.Amount);
```

and:

```csharp
.GroupBy(...)
```

Do not load every expense into memory just to calculate totals.

Avoid:

```csharp
var allExpenses = await dbContext.Expenses.ToListAsync();
```

when only an aggregate is required.

---

# 27. Dashboard

Create a simple dashboard.

Show:

```text
Today's Expense
This Month
Selected Period
```

Example:

```text
-----------------------------------------
Daily Expense Tracker
-----------------------------------------

Today's Expense       This Month

৳ 850.00              ৳ 24,350.00

-----------------------------------------
Category Summary
-----------------------------------------

Food                  ৳ 8,500
Transport             ৳ 3,200
Bills                 ৳ 4,100

-----------------------------------------
Recent Expenses
-----------------------------------------
Date       Category       Amount
...
```

Values must come from MySQL.

Do not hard-code summary values.

---

# 28. Category Summary

Show category totals for the selected period.

Example:

```text
Category          Total
------------------------
Food              ৳ 8,500
Transport         ৳ 3,200
Shopping          ৳ 5,000
Bills             ৳ 4,100
Other             ৳ 3,550
```

Use database-side grouping and aggregation.

---

# 29. Date Filters

Support:

```text
Today
Yesterday
This Week
This Month
Custom Date Range
```

Custom range:

```text
From Date
To Date
```

Validate:

```text
From Date <= To Date
```

Apply reasonable limits to custom ranges.

---

# 30. Currency

Initial currency:

```text
BDT
```

Display:

```text
৳
```

Store the numeric amount only.

Correct:

```text
500.00
```

Incorrect:

```text
৳ 500
```

Do not store formatted currency strings in the Amount column.

---

# 31. Date and Time

Use timestamps consistently.

Prefer UTC for technical timestamps:

```csharp
DateTime.UtcNow
```

For presentation, convert to Bangladesh time when required.

ExpenseDate is a business date and should be treated separately from technical timestamps such as CreatedAt.

---

# 32. Logging

Use ASP.NET Core's built-in logging initially.

Log:

```text
Unexpected exceptions
Application errors
Important security events
Important operational events
```

Never log:

```text
Passwords
Connection strings
Access tokens
API keys
Credit card numbers
Sensitive secrets
```

Do not expose stack traces to normal users.

---

# 33. Exception Handling

Do not expose:

- SQL exception details
- Stack traces
- Connection strings
- Internal server paths
- Database details

to end users.

Use centralized exception handling where appropriate.

Log detailed errors server-side.

Show users a safe, generic error message.

---

# 34. Authentication — Initial Version

Authentication is optional for Version 1.

Version 1 may be a single-user local application.

Do not add a complicated authentication system merely for demonstration.

If authentication is added later, all expense records must be associated with a user.

Add:

```text
UserId
```

to `Expenses`.

Every read/update/delete operation must enforce ownership server-side.

---

# 35. IDOR Protection — Future Multi-User Version

If authentication exists, never trust an expense ID alone.

Bad:

```csharp
var expense = await repository.GetByIdAsync(id);
```

followed by returning the record without checking ownership.

Prefer an ownership-aware operation:

```csharp
var expense =
    await repository.GetForUserAsync(id, currentUserId);
```

The server must verify:

```text
Expense.UserId == CurrentUserId
```

before viewing, editing, or deleting the expense.

---

# 36. Repository Guidance

Repositories may be used where they provide useful abstraction.

Do not automatically create:

```text
IGenericRepository<T>
IUnitOfWork
IRepositoryFactory
IRepositoryManager
```

unless there is a real reason.

Avoid abstraction for abstraction's sake.

Keep the architecture understandable.

---

# 37. No Overengineering

This is a learning project.

Do NOT introduce the following in Version 1:

```text
Microservices
CQRS
MediatR
Event Sourcing
Domain Events
RabbitMQ
Kafka
Redis
Kubernetes
Docker
Generic Repository Framework
Complex Unit of Work
```

unless explicitly requested.

Clean Architecture should not become unnecessary complexity.

---

# 38. Dependency Management

Do not add NuGet packages without a reason.

Before adding a package:

1. Inspect the existing project.
2. Check whether .NET 10 already provides the required functionality.
3. Check package compatibility.
4. Explain why the package is needed.
5. Add the minimum required package.

Avoid unnecessary third-party dependencies.

---

# 39. Database Migrations

Use EF Core migrations.

Typical workflow:

```bash
dotnet ef migrations add InitialCreate
```

Then review the migration.

Do not blindly apply destructive migrations.

Before applying a migration:

1. Inspect the migration.
2. Understand schema changes.
3. Confirm the target database.
4. Apply only to the intended development/test database.

Never assume the database is disposable.

---

# 40. Database Safety

Codex must NOT automatically execute destructive database commands.

Never automatically run:

```text
DROP DATABASE
DROP TABLE
TRUNCATE TABLE
DELETE FROM table
```

If a destructive database operation is genuinely required:

1. Explain the operation.
2. Identify the target database.
3. Explain the risk.
4. Require explicit user approval.

Never touch production databases unless explicitly authorized.

---

# 41. Codex Workspace Safety

Codex should operate only inside the designated project directory.

Do not access unrelated files.

Do not inspect:

```text
Personal documents
Desktop
Downloads
Pictures
Browser profiles
SSH keys
Credential stores
Other projects
```

unless explicitly required and explicitly authorized.

---

# 42. Codex Command Safety

Do not execute destructive commands automatically.

Examples:

```text
Remove-Item -Recurse
rm -rf
git reset --hard
git clean -fd
DROP DATABASE
DROP TABLE
TRUNCATE TABLE
```

If such an operation is required, stop and request explicit confirmation.

---

# 43. Git Safety

Before modifying code:

```bash
git status
```

After modifications:

```bash
git diff
```

Review the diff before committing.

Codex must not automatically:

```text
git commit
git push
```

unless explicitly instructed.

Never push secrets.

---

# 44. Codex Development Workflow

For every meaningful task:

```text
1. Inspect
2. Understand
3. Plan
4. Implement
5. Build
6. Test
7. Review diff
8. Report
```

Do not make unrelated changes.

Do not rewrite large parts of the application without a reason.

---

# 45. Initial Codex Task

The first instruction to Codex should be an inspection task.

Use:

```text
Read PROJECT_SPEC.md.

Do not modify any files yet.

Inspect the current project directory and tell me:

1. Current project structure
2. Installed .NET SDK/runtime versions relevant to this project
3. Existing .csproj files
4. Existing NuGet packages
5. Git status
6. Whether a solution already exists
7. What is missing to implement Phase 1

Do not access files outside this project directory.

Do not execute destructive commands.

Do not create or modify the database.

Do not install packages yet.

Give me a short implementation plan for Phase 1.
```

---

# 46. Phase 1 — Solution Setup

Create:

```text
Domain
Application
Infrastructure
Web
```

Create the solution and project references.

Verify:

```bash
dotnet build
```

Do not implement business features yet.

---

# 47. Phase 2 — Domain

Implement:

```text
Expense
ExpenseCategory
PaymentMethod
```

Add appropriate domain rules.

Build the solution.

---

# 48. Phase 3 — MySQL / EF Core

Implement:

```text
ExpenseDbContext
Entity configurations
MySQL provider
Initial migration
```

Configure the connection string securely.

Do not put credentials in source code.

---

# 49. Phase 4 — Expense CRUD

Implement:

```text
Create
Read
Update
Delete
```

Use:

```text
DTOs
Application services
Repository/application abstractions where useful
EF Core
Server-side validation
Antiforgery
```

Build and test after implementation.

---

# 50. Phase 5 — Filtering and Pagination

Implement:

```text
Date filtering
Category filtering
Pagination
```

Ensure filtering and pagination happen at the database level.

---

# 51. Phase 6 — Dashboard

Implement:

```text
Today's total
Monthly total
Custom-period total
Category totals
Recent expenses
```

Use efficient database aggregation.

---

# 52. Phase 7 — Testing

Create tests for:

```text
Expense validation
Category validation
Create expense
Update expense
Delete expense
Date filtering
Date-range validation
Summary calculation
Pagination
```

Add integration tests for important database/UI behavior where practical.

Do not use a production database for tests.

---

# 53. Phase 8 — Security Review

Before considering Version 1 complete, verify:

```text
[ ] No hard-coded secrets
[ ] No credentials in Git
[ ] Connection string stored securely
[ ] No SQL string concatenation
[ ] EF queries are parameterized
[ ] Server-side validation exists
[ ] Antiforgery protection enabled
[ ] Delete uses POST
[ ] User input is safely encoded
[ ] Exceptions are not exposed
[ ] Secrets are not logged
[ ] Pagination exists
[ ] Read-only queries use AsNoTracking where appropriate
[ ] Database indexes are appropriate
[ ] Production database is not touched
[ ] Git diff reviewed
[ ] Build succeeds
[ ] Tests pass
```

---

# 54. Performance Guidelines

Avoid:

```text
N+1 queries
Unbounded queries
Loading entire tables unnecessarily
Unnecessary Include()
Large in-memory aggregation
```

Prefer projections:

```csharp
.Select(x => new ExpenseListItemDto
{
    Id = x.Id,
    ExpenseDate = x.ExpenseDate,
    Amount = x.Amount,
    CategoryName = x.Category.Name
})
```

Only retrieve the columns needed by the UI.

---

# 55. Database Indexes

Consider indexes for common queries.

Initially consider:

```text
ExpenseDate
CategoryId
```

If multi-user support is added:

```text
UserId
```

Potential composite indexes should be based on actual query patterns.

Do not add indexes blindly.

---

# 56. UI Pages

Required initial pages:

```text
Dashboard
Expenses
Create Expense
Edit Expense
Categories
```

Keep the UI simple.

Use Bootstrap.

Prioritize usability over visual complexity.

---

# 57. Accessibility

Use:

```text
Proper labels
Semantic HTML
Keyboard-friendly controls
Clear validation messages
Accessible form controls
```

Do not rely only on color to communicate errors.

---

# 58. Completion Criteria

Version 1 is complete when:

```text
1. Application builds successfully.
2. MySQL connection works.
3. Database migration works.
4. Expense CRUD works.
5. Validation works.
6. Filtering works.
7. Pagination works.
8. Dashboard summaries work.
9. Category summaries work.
10. Security checklist passes.
11. Tests pass.
12. No secrets are committed.
13. Git diff has been reviewed.
```

---

# 59. Final Engineering Principle

Keep the application:

```text
Simple
Secure
Understandable
Testable
Maintainable
```

Do not add technology merely because it is popular.

Every architectural abstraction should have a reason.

The objective is to learn how to use Codex as an engineering assistant while maintaining control over:

- Code
- Database
- Security
- Dependencies
- Git
- Deployment
