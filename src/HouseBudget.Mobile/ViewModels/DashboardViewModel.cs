using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class DashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly StorageService _storage;

    [ObservableProperty] private decimal _totalNetWorth;
    [ObservableProperty] private decimal _monthlyIncome;
    [ObservableProperty] private decimal _monthlyExpenses;
    [ObservableProperty] private decimal _savingsRate;
    [ObservableProperty] private string _currency = "USD";
    [ObservableProperty] private string _userName = string.Empty;

    public ObservableCollection<AccountSummary> Accounts { get; } = new();
    public ObservableCollection<RecentTransaction> RecentTransactions { get; } = new();
    public ObservableCollection<BudgetSummary> ActiveBudgets { get; } = new();
    public ObservableCollection<UpcomingBill> UpcomingBills { get; } = new();
    public ObservableCollection<GoalSummary> ActiveGoals { get; } = new();
    public ObservableCollection<CategorySpending> TopCategories { get; } = new();

    public DashboardViewModel(ApiService apiService, StorageService storage)
    {
        _apiService = apiService;
        _storage = storage;
        Title = "Dashboard";
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            UserName = await _storage.GetUserNameAsync() ?? "User";
            Currency = await _storage.GetCurrencyAsync() ?? "USD";

            var dashboard = await _apiService.GetAsync<DashboardData>("api/v1/dashboard");
            if (dashboard is null) return;

            TotalNetWorth = dashboard.TotalNetWorth;
            MonthlyIncome = dashboard.MonthlyIncome;
            MonthlyExpenses = dashboard.MonthlyExpenses;
            SavingsRate = dashboard.SavingsRate;

            Accounts.Clear();
            foreach (var a in dashboard.Accounts) Accounts.Add(a);

            RecentTransactions.Clear();
            foreach (var t in dashboard.RecentTransactions) RecentTransactions.Add(t);

            ActiveBudgets.Clear();
            foreach (var b in dashboard.ActiveBudgets) ActiveBudgets.Add(b);

            UpcomingBills.Clear();
            foreach (var b in dashboard.UpcomingBills) UpcomingBills.Add(b);

            ActiveGoals.Clear();
            foreach (var g in dashboard.ActiveGoals) ActiveGoals.Add(g);

            TopCategories.Clear();
            foreach (var c in dashboard.TopExpenseCategories) TopCategories.Add(c);
        }, "Failed to load dashboard");
    }

    [RelayCommand]
    private static async Task NavigateToAsync(string route)
        => await Shell.Current.GoToAsync(route);
}
