using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HouseBudget.Mobile.Models;
using HouseBudget.Mobile.Services;
using System.Collections.ObjectModel;

namespace HouseBudget.Mobile.ViewModels;

public sealed partial class CategoriesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomTab))]
    private bool _showSystem = true;

    [ObservableProperty] private string _newCategoryName = string.Empty;
    [ObservableProperty] private string _newCategoryType = "Expense";
    [ObservableProperty] private string _newCategoryColor = "#E94560";

    public bool IsCustomTab => !ShowSystem;

    public ObservableCollection<CategoryModel> SystemCategories { get; } = new();
    public ObservableCollection<CategoryModel> CustomCategories { get; } = new();

    public List<string> CategoryTypes { get; } = new() { "Income", "Expense", "Both" };

    public List<string> PresetColors { get; } = new()
    {
        "#E94560", "#4CAF50", "#2196F3", "#FF9800",
        "#9C27B0", "#00BCD4", "#F44336", "#607D8B"
    };

    public CategoriesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Categories";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var result = await _apiService.GetAsync<List<CategoryModel>>("api/v1/categories");
            if (result is null) return;

            SystemCategories.Clear();
            CustomCategories.Clear();

            foreach (var c in result)
            {
                if (c.IsSystem) SystemCategories.Add(c);
                else CustomCategories.Add(c);
            }
        }, "Failed to load categories");
    }

    [RelayCommand]
    private void SwitchTab(string tab)
    {
        ShowSystem = tab == "System";
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            await Shell.Current.DisplayAlert("Error", "Please enter a category name.", "OK");
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.PostAsync<CategoryModel>("api/v1/categories", new
            {
                Name = NewCategoryName,
                TypeName = NewCategoryType,
                Color = NewCategoryColor
            });

            NewCategoryName = string.Empty;
            NewCategoryType = "Expense";
            NewCategoryColor = "#E94560";

            await LoadAsync();
        }, "Failed to add category");
    }

    [RelayCommand]
    private void SelectColor(string color)
    {
        NewCategoryColor = color;
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Guid id)
    {
        var confirm = await Shell.Current.DisplayAlert("Delete", "Delete this category?", "Yes", "No");
        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            await _apiService.DeleteAsync($"api/v1/categories/{id}");
            var item = CustomCategories.FirstOrDefault(c => c.Id == id);
            if (item is not null) CustomCategories.Remove(item);
        }, "Failed to delete category");
    }
}
