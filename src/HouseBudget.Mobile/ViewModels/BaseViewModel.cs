using CommunityToolkit.Mvvm.ComponentModel;

namespace HouseBudget.Mobile.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    protected async Task ExecuteSafeAsync(Func<Task> action, string? errorPrefix = null)
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{errorPrefix ?? "Error"}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
