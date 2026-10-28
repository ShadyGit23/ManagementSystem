using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClientApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
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

            // Емуляція валідації вхідних даних для Завдання 8 (повноцінний виклик API буде в Завданні 9)
            await Task.Delay(400);

            if (OnLoginSuccess != null)
            {
                await OnLoginSuccess.Invoke();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}