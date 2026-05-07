using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Services;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class ProfileViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly StorageService _storage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _lastName = string.Empty;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _defaultCurrency = "USD";
    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    public string Initials =>
        $"{(string.IsNullOrEmpty(FirstName) ? "?" : FirstName[0].ToString().ToUpper())}{(string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString().ToUpper())}";

    public List<string> Currencies { get; } = new() { "USD", "EUR", "GBP", "CHF", "CAD", "AUD" };

    public ProfileViewModel(ApiService apiService, AuthService authService, StorageService storage)
    {
        _apiService = apiService;
        _authService = authService;
        _storage = storage;
        Title = "Profile";
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var profile = await _apiService.GetAsync<UserProfile>("api/v1/users/me");
            if (profile is null) return;

            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            PhoneNumber = profile.PhoneNumber ?? string.Empty;
            DefaultCurrency = profile.DefaultCurrency;
            NotificationsEnabled = profile.NotificationsEnabled;
        }, "Failed to load profile");
    }

    [RelayCommand]
    private async Task UpdateProfileAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PutAsync<object>("api/v1/users/me", new
            {
                FirstName,
                LastName,
                PhoneNumber,
                DefaultCurrency,
                NotificationsEnabled
            });
            await Shell.Current.DisplayAlert("Success", "Profile updated successfully.", "OK");
        }, "Failed to update profile");
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            await Shell.Current.DisplayAlert("Error", "Please fill in all password fields.", "OK");
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            await Shell.Current.DisplayAlert("Error", "New passwords do not match.", "OK");
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PostAsync<object>("api/v1/auth/change-password", new
            {
                CurrentPassword,
                NewPassword,
                ConfirmNewPassword = ConfirmPassword
            });
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            await Shell.Current.DisplayAlert("Success", "Password changed successfully.", "OK");
        }, "Failed to change password");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Logout", "Cancel");
        if (!confirmed) return;
        _authService.Logout();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Delete Account",
            "This will permanently delete your account and all data. This action cannot be undone.",
            "Delete", "Cancel");
        if (!confirmed) return;

        var reconfirmed = await Shell.Current.DisplayAlert(
            "Are you sure?",
            "All your financial data, budgets, goals, and transactions will be permanently removed.",
            "Yes, Delete Everything", "Cancel");
        if (!reconfirmed) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.DeleteAsync("api/v1/users/me");
            _authService.Logout();
            await Shell.Current.GoToAsync("//login");
        }, "Failed to delete account");
    }
}

public record UserProfile(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string DefaultCurrency,
    bool NotificationsEnabled
);
