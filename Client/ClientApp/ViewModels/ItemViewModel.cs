using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Models;

namespace ClientApp.ViewModels;

public partial class ItemsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemModel> _items = new();

    [ObservableProperty]
    private ItemModel? _selectedItem;

    [ObservableProperty]
    private bool _isRefreshing;

    public event Func<ItemModel, Task>? OnNavigateToAction;
    public event Func<Task>? OnLogout;

    public ItemsViewModel()
    {
        LoadMockData();
    }

    [RelayCommand]
    private void LoadMockData()
    {
        IsRefreshing = true;

        Items = new ObservableCollection<ItemModel>
        {
            new ItemModel
            {
                ID = 3,
                ChampionName = "Yasuo",
                Role = "Mid",
                CoreItemBuild = "Kraken Slayer -> Infinity Edge",
                RuneSetup = "Lethal Tempo + Resolve",
                AuthorName = "Faker"
            },
            new ItemModel
            {
                ID = 4,
                ChampionName = "Ahri",
                Role = "Mid",
                CoreItemBuild = "Malignance -> Horizon Focus",
                RuneSetup = "Electrocute + Sorcery",
                AuthorName = "Caps"
            },
            new ItemModel
            {
                ID = 5,
                ChampionName = "Aatrox",
                Role = "Top",
                CoreItemBuild = "Sundered Sky -> Eclipse",
                RuneSetup = "Conqueror + Domination",
                AuthorName = "Zeus"
            }
        };

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ItemSelectedAsync(ItemModel item)
    {
        if (item == null) return;
        SelectedItem = null; // скидаємо виділення

        if (OnNavigateToAction != null)
        {
            await OnNavigateToAction.Invoke(item);
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (OnLogout != null)
        {
            await OnLogout.Invoke();
        }
    }
}