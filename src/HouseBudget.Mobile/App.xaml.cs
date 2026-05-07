using HouseBudget.Mobile.Services;

namespace HouseBudget.Mobile;

public partial class App : Application
{
    private readonly StorageService _storage;

    public App(StorageService storage, AppShell shell)
    {
        InitializeComponent();
        _storage = storage;
        MainPage = shell;
    }

    protected override async void OnStart()
    {
        base.OnStart();
        var isLoggedIn = await _storage.IsLoggedInAsync();
        await Shell.Current.GoToAsync(isLoggedIn ? "//main/dashboard" : "//login");
    }
}
