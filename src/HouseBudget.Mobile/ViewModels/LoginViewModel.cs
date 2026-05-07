using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Services;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isRegisterMode;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter email and password.";
                return;
            }

            var success = await _authService.LoginAsync(Email, Password);
            if (success)
                await Shell.Current.GoToAsync("//dashboard");
            else
                ErrorMessage = "Invalid email or password. Please try again.";
        }, "Login failed");
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            var success = await _authService.RegisterAsync(FirstName, LastName, Email, Password);
            if (success)
                await Shell.Current.GoToAsync("//dashboard");
            else
                ErrorMessage = "Registration failed. Email may already be in use.";
        }, "Registration failed");
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
        Title = IsRegisterMode ? "Create Account" : "Sign In";
        ErrorMessage = null;
    }
}
