using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class GoalsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private decimal _totalSaved;
    [ObservableProperty] private decimal _totalTarget;

    public ObservableCollection<GoalItem> Goals { get; } = new();

    public GoalsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Savings Goals";
    }

    [RelayCommand]
    public async Task LoadGoalsAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var result = await _apiService.GetAsync<List<GoalItem>>("api/v1/goals");
            if (result is null) return;

            Goals.Clear();
            foreach (var g in result) Goals.Add(g);

            TotalSaved = Goals.Sum(g => g.CurrentAmount);
            TotalTarget = Goals.Sum(g => g.TargetAmount);
        }, "Failed to load goals");
    }

    [RelayCommand]
    private async Task ContributeAsync(GoalItem goal)
    {
        var amountStr = await Shell.Current.CurrentPage.DisplayPromptAsync("Contribute", $"Add to '{goal.Name}':", "Add", "Cancel", "Amount", keyboard: Keyboard.Numeric);
        if (!decimal.TryParse(amountStr, out var amount) || amount <= 0) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PostAsync<GoalItem>($"api/v1/goals/{goal.Id}/contribute", new { amount });
            await LoadGoalsAsync();
        }, "Contribution failed");
    }

    [RelayCommand]
    private async Task CreateGoalAsync()
        => await Shell.Current.GoToAsync("createGoal");
}
