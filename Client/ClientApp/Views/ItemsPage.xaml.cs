using ClientApp.Models;
using ClientApp.ViewModels;

namespace ClientApp.Views;

public partial class ItemsPage : ContentPage
{
    private readonly ItemsViewModel _viewModel;

    public ItemsPage()
    {
        InitializeComponent();
        _viewModel = new ItemsViewModel();
        BindingContext = _viewModel;
    }

    private async void OnAddActionClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is ItemModel item)
        {
            await Navigation.PushAsync(new ActionPage(item));
        }
    }
}