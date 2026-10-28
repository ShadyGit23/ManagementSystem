using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp.Services;

namespace ClientApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public event Func<Task>? OnLoginSuccess;

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введіть Email та пароль!";
            return;
        }

        try
        {
            IsBusy = true;

            var (success, errorMsg) = await _apiService.LoginAsync(Email, Password);

            if (success)
            {
                if (OnLoginSuccess != null)
                {
                    await OnLoginSuccess.Invoke();
                }
            }
            else
            {
                ErrorMessage = errorMsg ?? "Помилка автентифікації.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}