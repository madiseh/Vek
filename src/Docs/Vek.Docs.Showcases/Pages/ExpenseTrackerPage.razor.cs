using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Vek;

namespace Vek.Docs.Showcases.Pages;

public partial class ExpenseTrackerPage : ComponentBase
{
    private const string StorageKey = "vek.expense-tracker.expenses.v1";
    private const decimal MonthlyIncomeEstimate = 5400m;

    private static readonly string[] CategoryNames =
    [
        "Food", "Home", "Transport", "Shopping", "Entertainment", "Health", "Travel", "Education", "Other"
    ];

    private List<ExpenseEntry> _expenses = CreateSampleExpenses();
    private bool _showExpenseForm;
    private bool _storageLoaded;
    private Guid? _editingExpenseId;
    private string _newDescription = string.Empty;
    private string _newMerchant = string.Empty;
    private string _newCategory = "Food";
    private DateTime? _newDate = DateTime.Today;
    private decimal _newAmount;
    private string _searchText = string.Empty;
    private string _categoryFilter = string.Empty;
    private string _feedback = string.Empty;
    private int _periodDays = 180;

    private DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private DateOnly RangeStart => Today.AddDays(-(_periodDays - 1));
    private string RangeLabel => _periodDays switch
    {
        30 => "Last 30 days",
        90 => "Last 90 days",
        _ => "Last 6 months"
    };

    private decimal Income => decimal.Round(MonthlyIncomeEstimate * _periodDays / 30m, 2);
    private decimal Expenses => PeriodExpenses.Sum(expense => expense.Amount);
    private decimal Balance => Income - Expenses;
    private bool CanSaveExpense => !string.IsNullOrWhiteSpace(_newDescription)
        && !string.IsNullOrWhiteSpace(_newMerchant)
        && _newAmount > 0
        && _newDate is DateTime date
        && date.Date <= DateTime.Today;

    private bool HasActiveFilters => !string.IsNullOrWhiteSpace(_searchText)
        || !string.IsNullOrWhiteSpace(_categoryFilter);

    private List<ExpenseEntry> PeriodExpenses => _expenses
        .Where(expense => expense.Date >= RangeStart && expense.Date <= Today)
        .OrderByDescending(expense => expense.Date)
        .ThenBy(expense => expense.Description, StringComparer.OrdinalIgnoreCase)
        .ToList();

    private List<ExpenseEntry> FilteredExpenses
    {
        get
        {
            IEnumerable<ExpenseEntry> query = PeriodExpenses;
            if (!string.IsNullOrWhiteSpace(_categoryFilter))
            {
                query = query.Where(expense => expense.Category == _categoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                query = query.Where(expense =>
                    expense.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                    || expense.Merchant.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }
    }

    private List<SpendingMonth> ChartMonths
    {
        get
        {
            var totals = PeriodExpenses
                .GroupBy(expense => new DateOnly(expense.Date.Year, expense.Date.Month, 1))
                .ToDictionary(group => group.Key, group => group.Sum(expense => expense.Amount));
            var firstMonth = new DateOnly(RangeStart.Year, RangeStart.Month, 1);
            var lastMonth = new DateOnly(Today.Year, Today.Month, 1);
            var months = new List<SpendingMonth>();

            for (var month = firstMonth; month <= lastMonth; month = month.AddMonths(1))
            {
                totals.TryGetValue(month, out var amount);
                months.Add(new SpendingMonth(
                    month.ToString("MMM", CultureInfo.GetCultureInfo("en-US")),
                    amount,
                    0,
                    month.Year == Today.Year && month.Month == Today.Month));
            }

            var maxAmount = months.Select(month => month.Amount).DefaultIfEmpty(0m).Max();
            return months.Select(month => month with
            {
                Percentage = maxAmount == 0 ? 0 : Math.Max(4, (int)Math.Round(month.Amount / maxAmount * 100m))
            }).ToList();
        }
    }

    private List<CategorySummary> CategorySummaries
    {
        get
        {
            var totals = PeriodExpenses
                .GroupBy(expense => expense.Category)
                .Select(group => new { Name = group.Key, Amount = group.Sum(expense => expense.Amount) })
                .OrderByDescending(category => category.Amount)
                .ToList();
            var totalExpenses = totals.Sum(category => category.Amount);
            var maxAmount = totals.Select(category => category.Amount).DefaultIfEmpty(0m).Max();

            return totals.Select(category => new CategorySummary(
                category.Name,
                category.Amount,
                totalExpenses == 0 ? 0 : (int)Math.Round(category.Amount / totalExpenses * 100m),
                maxAmount == 0 ? 0 : (int)Math.Round(category.Amount / maxAmount * 100m),
                GetCategoryColor(category.Name))).ToList();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            var savedJson = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (savedJson is not null)
            {
                _expenses = JsonSerializer.Deserialize<List<ExpenseEntry>>(savedJson) ?? [];
            }
        }
        catch (JsonException)
        {
            _feedback = "Saved demo data could not be read. The sample transactions are still available.";
        }
        catch (JSException)
        {
            _feedback = "Browser storage is unavailable. Changes will remain available for this session.";
        }

        _storageLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    private static List<ExpenseEntry> CreateSampleExpenses()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return
        [
            new(Guid.NewGuid(), "Weekly groceries", "Green Market", "Food", today.AddDays(-1), 86.42m),
            new(Guid.NewGuid(), "Monthly subscription", "Streamline", "Entertainment", today.AddDays(-2), 14.99m),
            new(Guid.NewGuid(), "Ride to work", "City Transit", "Transport", today.AddDays(-3), 12.50m),
            new(Guid.NewGuid(), "Coffee with Maya", "North Coffee", "Food", today.AddDays(-4), 18.75m),
            new(Guid.NewGuid(), "Household supplies", "Home Supply Co.", "Home", today.AddDays(-5), 42.30m),
            new(Guid.NewGuid(), "Dinner with friends", "The Copper Table", "Food", today.AddDays(-8), 74.80m),
            new(Guid.NewGuid(), "Prescription refill", "WellCare Pharmacy", "Health", today.AddDays(-12), 28.17m),
            new(Guid.NewGuid(), "Fuel", "Westside Fuel", "Transport", today.AddDays(-17), 54.60m),
            new(Guid.NewGuid(), "Groceries", "Green Market", "Food", today.AddDays(-23), 121.05m),
            new(Guid.NewGuid(), "Internet bill", "FiberHome", "Home", today.AddDays(-29), 69.99m),
            new(Guid.NewGuid(), "Music subscription", "Soundbox", "Entertainment", today.AddDays(-36), 10.99m),
            new(Guid.NewGuid(), "Running shoes", "Stride", "Shopping", today.AddDays(-43), 112.40m),
            new(Guid.NewGuid(), "Electricity bill", "City Power", "Home", today.AddDays(-51), 132.60m),
            new(Guid.NewGuid(), "Lunch near the office", "North Coffee", "Food", today.AddDays(-62), 24.70m),
            new(Guid.NewGuid(), "Transit pass", "City Transit", "Transport", today.AddDays(-71), 65.20m),
            new(Guid.NewGuid(), "Vet visit", "Oak Animal Clinic", "Health", today.AddDays(-84), 210m),
            new(Guid.NewGuid(), "Kitchen organizer", "Home Supply Co.", "Shopping", today.AddDays(-96), 48.90m),
            new(Guid.NewGuid(), "Weekend hotel", "Harbor Hotel", "Travel", today.AddDays(-108), 284m),
            new(Guid.NewGuid(), "Online course", "LearnWell", "Education", today.AddDays(-120), 159m),
            new(Guid.NewGuid(), "Monthly rent", "Morgan Properties", "Home", today.AddDays(-132), 1450m),
            new(Guid.NewGuid(), "Groceries", "Green Market", "Food", today.AddDays(-144), 98.30m),
            new(Guid.NewGuid(), "Car service", "Westside Auto", "Transport", today.AddDays(-156), 360m),
            new(Guid.NewGuid(), "Winter jacket", "North Outfitters", "Shopping", today.AddDays(-168), 174.50m),
            new(Guid.NewGuid(), "Home insurance", "Harbor Mutual", "Home", today.AddDays(-178), 118m),
            new(Guid.NewGuid(), "Concert tickets", "Civic Hall", "Entertainment", today.AddDays(-190), 96m),
            new(Guid.NewGuid(), "Annual checkup", "Oak Medical", "Health", today.AddDays(-205), 120m)
        ];
    }

    private static string FormatMoney(decimal amount) => amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

    private static Color GetCategoryColor(string category) => category switch
    {
        "Home" => Color.Primary,
        "Food" => Color.Success,
        "Transport" => Color.Info,
        "Shopping" => Color.Warning,
        "Health" => Color.Error,
        "Entertainment" => Color.Secondary,
        "Travel" => Color.Tertiary,
        _ => Color.Default
    };

    private bool IsCategorySelected(string category) => _categoryFilter == category;

    private void ToggleCategoryFilter(string category) =>
        _categoryFilter = IsCategorySelected(category) ? string.Empty : category;

    private void ClearFilters()
    {
        _searchText = string.Empty;
        _categoryFilter = string.Empty;
    }

    private void OpenNewExpenseForm()
    {
        ResetForm();
        _showExpenseForm = true;
        _feedback = string.Empty;
    }

    private void EditExpense(ExpenseEntry expense)
    {
        _editingExpenseId = expense.Id;
        _newDescription = expense.Description;
        _newMerchant = expense.Merchant;
        _newCategory = expense.Category;
        _newDate = expense.Date.ToDateTime(TimeOnly.MinValue);
        _newAmount = expense.Amount;
        _showExpenseForm = true;
        _feedback = string.Empty;
    }

    private void CloseExpenseForm()
    {
        _showExpenseForm = false;
        ResetForm();
    }

    private void ResetForm()
    {
        _editingExpenseId = null;
        _newDescription = string.Empty;
        _newMerchant = string.Empty;
        _newCategory = "Food";
        _newDate = DateTime.Today;
        _newAmount = 0;
    }

    private async Task SaveExpenseAsync()
    {
        if (!CanSaveExpense || _newDate is not DateTime expenseDate)
        {
            _feedback = "Enter a description, merchant, valid amount, and date before saving.";
            return;
        }

        var expense = new ExpenseEntry(
            _editingExpenseId ?? Guid.NewGuid(),
            _newDescription.Trim(),
            _newMerchant.Trim(),
            _newCategory,
            DateOnly.FromDateTime(expenseDate),
            _newAmount);
        var existingIndex = _editingExpenseId is Guid editingId
            ? _expenses.FindIndex(item => item.Id == editingId)
            : -1;
        var isEditing = existingIndex >= 0;

        if (isEditing)
        {
            _expenses[existingIndex] = expense;
        }
        else
        {
            _expenses.Add(expense);
        }

        await PersistExpensesAsync();
        _feedback = isEditing ? "Expense updated." : "Expense added.";
        _showExpenseForm = false;
        ResetForm();
    }

    private async Task DeleteExpenseAsync(ExpenseEntry expense)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Delete ‘{expense.Description}’ from your transactions?");
        if (!confirmed)
        {
            return;
        }

        _expenses.RemoveAll(item => item.Id == expense.Id);
        await PersistExpensesAsync();
        _feedback = "Expense deleted.";
    }

    private async Task PersistExpensesAsync()
    {
        if (!_storageLoaded)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(_expenses);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch (JSException)
        {
            _feedback = "This change is available for this session, but browser storage is unavailable.";
        }
    }

    private sealed record ExpenseEntry(Guid Id, string Description, string Merchant, string Category, DateOnly Date, decimal Amount);
    private sealed record SpendingMonth(string Label, decimal Amount, int Percentage, bool IsCurrent);
    private sealed record CategorySummary(string Name, decimal Amount, int Share, int RelativePercent, Color Color);
}
