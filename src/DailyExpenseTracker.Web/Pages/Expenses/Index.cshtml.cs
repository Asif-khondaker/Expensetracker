using System.Text.RegularExpressions;
using DailyExpenseTracker.Application.Categories.DTOs;
using DailyExpenseTracker.Application.Categories.Interfaces;
using DailyExpenseTracker.Application.Common.Exceptions;
using DailyExpenseTracker.Application.Expenses.DTOs;
using DailyExpenseTracker.Application.Expenses.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DailyExpenseTracker.Web.Pages.Expenses;

public class IndexModel : PageModel
{
    private const int PageSize = 10;
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;

    public IndexModel(IExpenseService expenseService, ICategoryService categoryService)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
    }

    private static readonly Dictionary<string, string> CategoryBadgeClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Food"] = "badge-cat-food",
        ["Transport"] = "badge-cat-transport",
        ["Shopping"] = "badge-cat-shopping",
        ["Bills"] = "badge-cat-bills",
        ["Medical"] = "badge-cat-medical",
        ["Education"] = "badge-cat-education",
        ["Entertainment"] = "badge-cat-entertainment",
        ["Rent"] = "badge-cat-rent",
        ["Family"] = "badge-cat-family",
        ["Other"] = "badge-cat-other"
    };

    private static readonly string[] FallbackBadgeClasses =
    [
        "badge-cat-food",
        "badge-cat-transport",
        "badge-cat-shopping",
        "badge-cat-bills",
        "badge-cat-medical",
        "badge-cat-education",
        "badge-cat-entertainment",
        "badge-cat-rent",
        "badge-cat-family"
    ];

    private static readonly (Regex Pattern, string IconClass)[] DescriptionIconRules =
    [
        (WordPattern("coffee|tea|chai|latte|espresso|cappuccino|cafe"), "bi-cup-hot text-warning-emphasis"),
        (WordPattern("juice|drink|drinks|soda|water|smoothie|milkshake|lassi|beverage"), "bi-cup-straw text-info"),
        (WordPattern("cake|pastry|bakery|dessert|sweets?|chocolate|ice cream|birthday"), "bi-cake2 text-danger"),
        (WordPattern("grocery|groceries|vegetables?|fruits?|market|bazar|bazaar|supermarket|milk|eggs?"), "bi-basket text-success"),
        (WordPattern("lunch|dinner|breakfast|meal|food|biryani|rice|pizza|burger|snacks?|restaurant|dining|chicken|fish|meat"), "bi-egg-fried text-warning"),
        (WordPattern("watch|smartwatch"), "bi-watch text-secondary"),
        (WordPattern("glasses|eyeglasses|sunglasses"), "bi-eyeglasses text-secondary"),
        (WordPattern("backpack|school bag"), "bi-backpack text-primary"),
        (WordPattern("shirts?|t-?shirts?|pants?|jeans|dress|clothes?|clothing|jacket|shoes?|sneakers?|bag|bags|shopping|mall|buy|purchase"), "bi-bag text-primary"),
        (WordPattern("order|cart|parcel|delivery|online"), "bi-cart text-primary"),
        (WordPattern("uber|pathao|taxi|cab|cng|rickshaw|ride"), "bi-taxi-front text-warning"),
        (WordPattern("bus|coach"), "bi-bus-front text-primary"),
        (WordPattern("train|metro|rail|railway"), "bi-train-front text-info"),
        (WordPattern("flight|plane|airline|airport"), "bi-airplane text-primary"),
        (WordPattern("fuel|petrol|gas|octane|diesel|parking|toll"), "bi-fuel-pump text-danger"),
        (WordPattern("bike|bicycle|cycle"), "bi-bicycle text-success"),
        (WordPattern("car|drive"), "bi-car-front text-primary"),
        (WordPattern("trip|tour|travel|hotel|luggage|vacation"), "bi-luggage text-info"),
        (WordPattern("electric|electricity|power|desco|dpdc"), "bi-lightning-charge text-warning"),
        (WordPattern("wasa"), "bi-droplet text-info"),
        (WordPattern("wifi|internet|broadband|mobile data"), "bi-wifi text-primary"),
        (WordPattern("phone|mobile|recharge|topup|sim"), "bi-phone text-success"),
        (WordPattern("rent|house|apartment|flat|room|home"), "bi-house text-success"),
        (WordPattern("bill|bills|utility|utilities|invoice|tax|subscription"), "bi-receipt text-secondary"),
        (WordPattern("medicine|medicines|tablet|tablets|pill|pills|capsule|pharmacy|drug"), "bi-capsule text-danger"),
        (WordPattern("doctor|hospital|clinic|checkup|dental|dentist|medical|health|lab test|blood test"), "bi-heart-pulse text-danger"),
        (WordPattern("book|books|novel|stationery|pen|notebook|notebooks|paper"), "bi-book text-primary"),
        (WordPattern("tuition|school|college|university|course|class|exam|study|seminar"), "bi-mortarboard text-primary"),
        (WordPattern("movie|movies|cinema|netflix|film|theater"), "bi-film text-purple"),
        (WordPattern("music|spotify|concert|song"), "bi-music-note-beamed text-primary"),
        (WordPattern("game|games|gaming|playstation|steam|xbox"), "bi-controller text-primary"),
        (WordPattern("tv|television|cable"), "bi-tv text-secondary"),
        (WordPattern("ticket|tickets|zoo|park|event"), "bi-ticket-perforated text-warning"),
        (WordPattern("laptop|computer|pc|keyboard|mouse|charger|ssd"), "bi-laptop text-secondary"),
        (WordPattern("headphones?|earphones?|earbuds?|airpods?|speaker"), "bi-headphones text-secondary"),
        (WordPattern("camera|photo|photography"), "bi-camera text-secondary"),
        (WordPattern("repair|fix|maintenance|plumber|service|tools?"), "bi-wrench text-secondary"),
        (WordPattern("haircut|salon|barber|grooming"), "bi-scissors text-secondary"),
        (WordPattern("gift|gifts|present|flowers?"), "bi-gift text-danger"),
        (WordPattern("family|mom|dad|mother|father|parents?|kids?|child|children"), "bi-people text-success"),
        (WordPattern("atm|bank|card|transfer|bkash|nagad|rocket|loan"), "bi-credit-card text-secondary"),
        (WordPattern("test|demo|sample"), "bi-ui-checks text-secondary")
    ];

    private static readonly Dictionary<string, string> CategoryFallbackIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Food"] = "bi-egg-fried text-warning",
        ["Transport"] = "bi-car-front text-primary",
        ["Shopping"] = "bi-bag text-primary",
        ["Bills"] = "bi-receipt text-secondary",
        ["Medical"] = "bi-heart-pulse text-danger",
        ["Education"] = "bi-book text-primary",
        ["Entertainment"] = "bi-film text-primary",
        ["Rent"] = "bi-house text-success",
        ["Family"] = "bi-people text-success"
    };

    private static Regex WordPattern(string alternation) =>
        new($@"\b(?:{alternation})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<ExpenseListItemDto> Expenses { get; private set; } = [];

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int TotalPages { get; private set; }
    public int TotalExpenses { get; private set; }
    public IReadOnlyList<CategoryOptionDto> Categories { get; private set; } = [];

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }
    public int FirstExpenseNumber => TotalExpenses == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
    public int LastExpenseNumber => Math.Min(PageNumber * PageSize, TotalExpenses);

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetActiveCategoriesAsync();
        ExpensePageDto result;
        try
        {
            result = await _expenseService.GetExpensesPageAsync(FromDate, ToDate, CategoryId, Math.Max(PageNumber, 1), PageSize);
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            FromDate = null;
            ToDate = null;
            CategoryId = null;
            result = await _expenseService.GetExpensesPageAsync(null, null, null, 1, PageSize);
        }

        TotalExpenses = result.TotalCount;
        TotalPages = (int)Math.Ceiling(TotalExpenses / (double)PageSize);
        PageNumber = result.PageNumber;
        Expenses = result.Items;
    }

    public string GetCategoryBadgeClass(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return "badge-cat-other";
        }

        var trimmed = categoryName.Trim();
        if (CategoryBadgeClasses.TryGetValue(trimmed, out var badgeClass))
        {
            return badgeClass;
        }

        uint hash = 2166136261;
        foreach (var ch in trimmed.ToUpperInvariant())
        {
            hash = (hash ^ ch) * 16777619;
        }

        return FallbackBadgeClasses[hash % FallbackBadgeClasses.Length];
    }

    public string GetDescriptionIconClass(string? description, string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "bi-dash text-muted";
        }

        foreach (var (pattern, iconClass) in DescriptionIconRules)
        {
            if (pattern.IsMatch(description))
            {
                return iconClass;
            }
        }

        if (!string.IsNullOrWhiteSpace(categoryName) &&
            CategoryFallbackIcons.TryGetValue(categoryName.Trim(), out var fallbackIcon))
        {
            return fallbackIcon;
        }

        return "bi-chat-left-text text-muted";
    }
}
