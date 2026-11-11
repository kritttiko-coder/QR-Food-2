using Microsoft.Maui.Controls;

namespace FoodExpiryApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            ShowCrashAlert(ex?.Message ?? "Unknown error", ex?.StackTrace ?? "");
        };
        
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            ShowCrashAlert(e.Exception.InnerException?.Message ?? e.Exception.Message, e.Exception.InnerException?.StackTrace ?? "");
            e.SetObserved();
        };

        ThemeManager.Apply();

        MainPage = new SplashPage();
    }

    private void ShowCrashAlert(string message, string stackTrace)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH] {message}\n{stackTrace}");
                
                if (Current?.MainPage != null)
                {
                    await Current.MainPage.DisplayAlert("ОШИБКА ЗАПУСКА", $"{message}\n\n(Подробности в логах)", "ОК");
                }
            }
            catch { }
        });
    }
}
