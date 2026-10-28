using Android.App;
using Android.Content.PM;
using Android.OS;

namespace FoodExpiryApp;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, // ИМЕННО ЭТА СТРОКА выводит иконку на главный экран!
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
