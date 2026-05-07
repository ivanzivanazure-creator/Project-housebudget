using HouseBudget.Mobile.ViewModels;

namespace HouseBudget.Mobile.Views;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;

    public GoalsPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadGoalsCommand.ExecuteAsync(null);
    }
}
