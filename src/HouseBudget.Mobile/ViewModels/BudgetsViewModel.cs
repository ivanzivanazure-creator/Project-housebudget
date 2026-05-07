using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class BudgetsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private decimal _totalBudgeted;
    [ObservableProperty] private decimal _totalSpent;
    [ObservableProperty] private decimal _totalRemaining;

    public ObservableCollection<BudgetItem> Budgets { get; } = new();

    public BudgetsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Budgets";
    }

    [RelayCommand]
    public async Task LoadBudgetsAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var result = await _apiService.GetAsync<List<BudgetItem>>("api/v1/budgets?activeOnly=false");
            if (result is null) return;

            Budgets.Clear();
            foreach (var b in result) Budgets.Add(b);

            TotalBudgeted = Budgets.Sum(b => b.TotalAmount);
            TotalSpent = Budgets.Sum(b => b.TotalSpent);
            TotalRemaining = TotalBudgeted - TotalSpent;
        }, "Failed to load budgets");
    }

    [RelayCommand]
    private async Task CreateBudgetAsync()
        => await Shell.Current.GoToAsync("createBudget");
}
