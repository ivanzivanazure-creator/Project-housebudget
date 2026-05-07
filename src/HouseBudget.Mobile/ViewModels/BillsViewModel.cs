using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class BillsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private decimal _totalMonthlyAmount;
    [ObservableProperty] private int _dueSoonCount;

    public ObservableCollection<BillItem> Bills { get; } = new();

    public BillsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Bills";
    }

    [RelayCommand]
    public async Task LoadBillsAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var result = await _apiService.GetAsync<List<BillItem>>("api/v1/bills");
            if (result is null) return;

            Bills.Clear();
            foreach (var b in result) Bills.Add(b);

            TotalMonthlyAmount = Bills.Where(b => b.IsActive).Sum(b => b.Amount);
            DueSoonCount = Bills.Count(b => b.IsDueSoon || b.IsOverdue);
        }, "Failed to load bills");
    }

    [RelayCommand]
    private async Task MarkPaidAsync(Guid id)
    {
        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PostAsync<object>($"api/v1/bills/{id}/pay", new { });
            await LoadBillsAsync();
        }, "Failed to mark bill as paid");
    }

    [RelayCommand]
    private async Task CreateBillAsync()
        => await Shell.Current.GoToAsync("createBill");
}
