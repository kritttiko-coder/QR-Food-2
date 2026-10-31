namespace FoodExpiryApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        ThemeManager.Apply();

        MainPage = new SplashPage();
    }
}
