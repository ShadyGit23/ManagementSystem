using ClientApp.Models;
using ClientApp.ViewModels;

namespace ClientApp.Views;

public partial class ActionPage : ContentPage
{
    private readonly ActionViewModel _viewModel;

    public ActionPage(ItemModel item)
    {
        InitializeComponent();
        _viewModel = new ActionViewModel();
        BindingContext = _viewModel;
        _viewModel.SetItem(item);

        _viewModel.OnActionSaved += async () =>
        {
            // Повернення назад до списку після збереження
            await Navigation.PopAsync();
        };
    }
}