using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;
using ClientApp.Services;

namespace ClientApp.ViewModels;

public partial class ItemsViewModel : ObservableObject
{
private readonly ApiService _apiService = new();

[ObservableProperty]
private ObservableCollection<ItemModel> _items = new();

[ObservableProperty]
private ItemModel? _selectedItem;

[ObservableProperty]
private bool _isRefreshing;

[ObservableProperty]
private string _userGreeting = "Користувач";

public event Func<ItemModel, Task>? OnNavigateToAction;
public event Func<Task>? OnLogout;

public ItemsViewModel()
{
    string name = Preferences.Default.Get("user_name", "Гравець");
    UserGreeting = $"Привіт, {name}!";
    _ = LoadDataAsync();
}

[RelayCommand]
public async Task LoadDataAsync()
{
    try
    {
        IsRefreshing = true;
        var serverItems = await _apiService.GetItemsAsync();

        Items.Clear();
        foreach (var item in serverItems)
        {
            Items.Add(item);
        }
    }
    finally
    {
        IsRefreshing = false;
    }
}

[RelayCommand]
private async Task ItemSelectedAsync(ItemModel item)
{
    if (item == null) return;
    SelectedItem = null;

    if (OnNavigateToAction != null)
    {
        await OnNavigateToAction.Invoke(item);
    }
}

[RelayCommand]
private async Task LogoutAsync()
{
    ApiService.Logout();

    if (OnLogout != null)
    {
        await OnLogout.Invoke();
    }
}
}