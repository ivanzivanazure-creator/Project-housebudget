using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class ReportsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private string _selectedPeriod = "This Month";
    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _netSavings;
    [ObservableProperty] private decimal _savingsRate;
    [ObservableProperty] private bool _showThisMonth = true;
    [ObservableProperty] private bool _showLastMonth;
    [ObservableProperty] private bool _showThisYear;

    public ObservableCollection<CategoryBreakdownItem> CategoryBreakdown { get; } = new();
    public ObservableCollection<MonthlyTrendItem> MonthlyTrend { get; } = new();

    public ReportsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Reports";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var today = DateTime.Today;

            if (SelectedPeriod == "This Year")
            {
                var annual = await _apiService.GetAsync<AnnualReportData>($"api/v1/reports/annual?year={today.Year}");
                if (annual is not null)
                {
                    TotalIncome = annual.TotalIncome;
                    TotalExpenses = annual.TotalExpenses;
                    NetSavings = annual.NetSavings;
                    SavingsRate = annual.SavingsRate;

                    CategoryBreakdown.Clear();
                    foreach (var c in annual.CategoryBreakdown) CategoryBreakdown.Add(c);

                    MonthlyTrend.Clear();
                    foreach (var m in annual.MonthlyTrend) MonthlyTrend.Add(m);
                }
            }
            else
            {
                int year = today.Year;
                int month = SelectedPeriod == "Last Month"
                    ? (today.Month == 1 ? 12 : today.Month - 1)
                    : today.Month;
                if (SelectedPeriod == "Last Month" && today.Month == 1)
                    year = today.Year - 1;

                var monthly = await _apiService.GetAsync<MonthlyReportData>($"api/v1/reports/monthly?year={year}&month={month}");
                if (monthly is not null)
                {
                    TotalIncome = monthly.TotalIncome;
                    TotalExpenses = monthly.TotalExpenses;
                    NetSavings = monthly.NetSavings;
                    SavingsRate = monthly.SavingsRate;

                    CategoryBreakdown.Clear();
                    foreach (var c in monthly.CategoryBreakdown) CategoryBreakdown.Add(c);

                    MonthlyTrend.Clear();
                    foreach (var m in monthly.MonthlyTrend) MonthlyTrend.Add(m);
                }
            }
        }, "Failed to load report");
    }

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period;
        ShowThisMonth = period == "This Month";
        ShowLastMonth = period == "Last Month";
        ShowThisYear = period == "This Year";
        await LoadAsync();
    }
}
