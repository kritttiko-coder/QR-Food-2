using Android.App;
using Android.Content.PM;
using Microsoft.Maui;

namespace FoodExpiryApp;

[Activity(
    
    Name = "<ApplicationId>com.example.foodexpiryapp</ApplicationId>",
    Theme = "@style/Maui.SplashTheme",
    Exported = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
