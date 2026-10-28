using ClientApp.Views;

namespace ClientApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Пункт 6 методички: налаштування навігації між сторінками
        MainPage = new NavigationPage(new LoginPage())
        {
            BarBackgroundColor = Color.FromArgb("#1c2541"),
            BarTextColor = Colors.White
        };
    }
}