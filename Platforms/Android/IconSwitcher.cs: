using Android.Content;
using Android.Content.PM;

namespace FoodExpiryApp.Platforms.Android;

public static class IconSwitcher
{
    // id из CustomizationPage → имя activity-alias из AndroidManifest.xml
    static readonly Dictionary<string, string> Aliases = new()
    {
        ["classic"]   = "LauncherClassic",
        ["winter"]    = "LauncherWinter",
        ["halloween"] = "LauncherHalloween",
        ["mono"]      = "LauncherMonochrome",
        ["neon"]      = "LauncherNeon"
    };

    public static void Apply(string iconId)
    {
        var ctx = global::Android.App.Application.Context;
        var pm  = ctx.PackageManager;
        var pkg = ctx.PackageName;

        if (!Aliases.TryGetValue(iconId, out var target))
            target = Aliases["classic"];

        // 1) Сначала включаем выбранный — чтобы лаунчер не остался без точки входа
        SetState(pm, pkg, target, ComponentEnabledState.Enabled);

        // 2) Затем гасим остальные
        foreach (var name in Aliases.Values)
        {
            if (name == target) continue;
            SetState(pm, pkg, name, ComponentEnabledState.Disabled);
        }
    }

    static void SetState(
        PackageManager pm, string pkg, string alias, ComponentEnabledState state)
    {
        try
        {
            var component = new ComponentName(pkg, $"{pkg}.{alias}");
            // 1 = PackageManager.DONT_KILL_APP: не убивать процесс при смене состояния
            pm.SetComponentEnabledSetting(component, state, (ComponentEnabledState)1);
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Warn("IconSwitcher",
                $"Не удалось изменить {alias}: {ex.Message}");
        }
    }
}
