using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class SubscriptionViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty] private SubscriptionModel? _currentSubscription;
    [ObservableProperty] private bool _hasSubscription;
    [ObservableProperty] private string _statusBadgeText = string.Empty;
    [ObservableProperty] private Color _statusBadgeColor = Colors.Grey;
    [ObservableProperty] private string _renewalText = string.Empty;
    [ObservableProperty] private string _selectedBillingPeriod = "Monthly";
    [ObservableProperty] private bool _showMonthly = true;

    public ObservableCollection<SubscriptionPlanModel> Plans { get; } = new();
    public ObservableCollection<PaymentHistoryItem> PaymentHistory { get; } = new();

    public SubscriptionViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Subscription";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var plans = await _apiService.GetAsync<List<SubscriptionPlanModel>>("api/v1/subscriptions/plans");
            Plans.Clear();
            if (plans is not null)
                foreach (var p in plans.Where(p => p.BillingPeriod == SelectedBillingPeriod))
                    Plans.Add(p);

            try
            {
                CurrentSubscription = await _apiService.GetAsync<SubscriptionModel>("api/v1/subscriptions/my");
                HasSubscription = CurrentSubscription is not null;
                UpdateStatusDisplay();
            }
            catch
            {
                HasSubscription = false;
            }

            try
            {
                var history = await _apiService.GetAsync<List<PaymentHistoryItem>>("api/v1/payments?pageSize=5");
                PaymentHistory.Clear();
                if (history is not null)
                    foreach (var h in history) PaymentHistory.Add(h);
            }
            catch { /* history is optional */ }
        }, "Failed to load subscription data");
    }

    [RelayCommand]
    private async Task SelectBillingPeriodAsync(string period)
    {
        SelectedBillingPeriod = period;
        ShowMonthly = period == "Monthly";
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SubscribeAsync(SubscriptionPlanModel plan)
    {
        if (IsBusy) return;
        var confirmed = await Shell.Current.DisplayAlert(
            "Subscribe",
            $"Subscribe to {plan.Name} for {plan.Price:N2} {plan.Currency}/{plan.BillingPeriod.ToLower()}?" +
            (plan.TrialDays > 0 ? $"\n\n{plan.TrialDays}-day free trial included." : string.Empty),
            "Subscribe", "Cancel");

        if (!confirmed) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PostAsync<object>("api/v1/subscriptions", new { PlanId = plan.Id });
            await Shell.Current.DisplayAlert("Success", $"You are now subscribed to {plan.Name}!", "OK");
            await LoadAsync();
        }, "Failed to subscribe");
    }

    [RelayCommand]
    private async Task UpgradeAsync(SubscriptionPlanModel plan)
    {
        if (IsBusy) return;
        var confirmed = await Shell.Current.DisplayAlert(
            "Change Plan",
            $"Switch to {plan.Name} for {plan.Price:N2} {plan.Currency}/{plan.BillingPeriod.ToLower()}?",
            "Confirm", "Cancel");

        if (!confirmed) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PutAsync<object>("api/v1/subscriptions/upgrade", new { NewPlanId = plan.Id });
            await Shell.Current.DisplayAlert("Success", $"Your plan has been updated to {plan.Name}.", "OK");
            await LoadAsync();
        }, "Failed to change plan");
    }

    [RelayCommand]
    private async Task CancelSubscriptionAsync()
    {
        if (IsBusy) return;
        var action = await Shell.Current.DisplayActionSheet(
            "Cancel Subscription",
            "Keep Subscription",
            null,
            "Cancel at period end",
            "Cancel immediately");

        if (action is null or "Keep Subscription") return;

        bool atPeriodEnd = action == "Cancel at period end";
        var confirmed = await Shell.Current.DisplayAlert(
            "Confirm Cancellation",
            atPeriodEnd
                ? "Your subscription will be cancelled at the end of the current billing period."
                : "Your subscription will be cancelled immediately and you will lose access now.",
            "Confirm", "Back");

        if (!confirmed) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.DeleteAsync("api/v1/subscriptions");
            await Shell.Current.DisplayAlert("Cancelled", "Your subscription has been cancelled.", "OK");
            await LoadAsync();
        }, "Failed to cancel subscription");
    }

    private void UpdateStatusDisplay()
    {
        if (CurrentSubscription is null) return;

        (StatusBadgeText, StatusBadgeColor) = CurrentSubscription.StatusName switch
        {
            "Active" => ("Active", Color.FromArgb("#4CAF50")),
            "Trialing" => ($"Trial - {CurrentSubscription.DaysUntilRenewal}d left", Color.FromArgb("#2196F3")),
            "PastDue" => ("Past Due", Color.FromArgb("#FF9800")),
            "Cancelled" => ("Cancelled", Color.FromArgb("#9E9E9E")),
            "Expired" => ("Expired", Color.FromArgb("#F44336")),
            _ => (CurrentSubscription.StatusName, Colors.Grey)
        };

        RenewalText = CurrentSubscription.DaysUntilRenewal switch
        {
            0 => "Renews today",
            1 => "Renews tomorrow",
            > 0 => $"Renews in {CurrentSubscription.DaysUntilRenewal} days",
            _ => "Subscription ended"
        };
    }
}
