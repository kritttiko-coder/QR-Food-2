using Android.Content;
using Android.Content.PM;

namespace FoodExpiryApp;

public static class AppIconManager
{
    private static readonly string[] Keys = { "classic", "winter", "halloween", "monochrome", "neon" };
    private static readonly string[] Aliases =
    {
        "FoodExpiryApp.LauncherClassic",
        "FoodExpiryApp.LauncherWinter",
        "FoodExpiryApp.LauncherHalloween",
        "FoodExpiryApp.LauncherMonochrome",
        "FoodExpiryApp.LauncherNeon"
    };

    public static string Current => Preferences.Default.Get("app_icon", "classic");

    public static void Apply(string key)
    {
#if ANDROID
        int selected = Array.IndexOf(Keys, key);
        if (selected < 0) selected = 0;

        var context = Android.App.Application.Context;
        var pm = context.PackageManager ?? throw new InvalidOperationException("PackageManager недоступен.");

        // Сначала включаем выбранный alias, затем выключаем остальные.
        pm.SetComponentEnabledSetting(
            new ComponentName(context, Aliases[selected]),
            ComponentEnabledState.Enabled,
            ComponentEnableOption.DontKillApp);

        for (int i = 0; i < Aliases.Length; i++)
        {
            if (i == selected) continue;
            pm.SetComponentEnabledSetting(
                new ComponentName(context, Aliases[i]),
                ComponentEnabledState.Disabled,
                ComponentEnableOption.DontKillApp);
        }

        Preferences.Default.Set("app_icon", Keys[selected]);
#endif
    }
}
