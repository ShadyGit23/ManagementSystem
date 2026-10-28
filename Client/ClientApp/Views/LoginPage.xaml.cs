using ClientApp.ViewModels;

namespace ClientApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;

        _viewModel.OnLoginSuccess += async () =>
        {
            // Перехід на екран перегляду чемпіонів
            await Navigation.PushAsync(new ItemsPage());
        };
    }
}