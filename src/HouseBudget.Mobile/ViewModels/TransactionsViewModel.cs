using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class TransactionsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedFilter = "All";
    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpenses;

    public ObservableCollection<TransactionItem> Transactions { get; } = new();
    public List<string> FilterOptions { get; } = new() { "All", "Income", "Expenses", "This Month", "Last Month" };

    public TransactionsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Transactions";
    }

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var queryParams = BuildQueryParams();
            var result = await _apiService.GetAsync<PaginatedResult<TransactionItem>>($"api/v1/transactions?{queryParams}");
            if (result is null) return;

            Transactions.Clear();
            foreach (var t in result.Items) Transactions.Add(t);

            TotalIncome = Transactions.Where(t => t.TypeName == "Income").Sum(t => t.Amount);
            TotalExpenses = Transactions.Where(t => t.TypeName == "Expense").Sum(t => t.Amount);
        }, "Failed to load transactions");
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
        => await Shell.Current.GoToAsync("addTransaction");

    [RelayCommand]
    private async Task DeleteTransactionAsync(Guid id)
    {
        var confirm = await Shell.Current.CurrentPage.DisplayAlert("Delete", "Delete this transaction?", "Yes", "No");
        if (!confirm) return;
        await ExecuteSafeAsync(async () =>
        {
            await _apiService.DeleteAsync($"api/v1/transactions/{id}");
            var item = Transactions.FirstOrDefault(t => t.Id == id);
            if (item is not null) Transactions.Remove(item);
        }, "Failed to delete");
    }

    private string BuildQueryParams()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return SelectedFilter switch
        {
            "Income" => "type=Income",
            "Expenses" => "type=Expense",
            "This Month" => $"from={new DateOnly(today.Year, today.Month, 1):yyyy-MM-dd}&to={new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)):yyyy-MM-dd}",
            "Last Month" => BuildLastMonthQuery(today),
            _ => string.Empty
        };
    }

    private static string BuildLastMonthQuery(DateOnly today)
    {
        var lastMonth = today.AddMonths(-1);
        return $"from={new DateOnly(lastMonth.Year, lastMonth.Month, 1):yyyy-MM-dd}&to={new DateOnly(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)):yyyy-MM-dd}";
    }
}

public record PaginatedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);
