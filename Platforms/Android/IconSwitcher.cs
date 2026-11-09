using Android.Content;
using Android.Content.PM;

namespace FoodExpiryApp.Platforms.Android;

public static class IconSwitcher
{
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
        if (ctx == null) return;

        var pm  = ctx.PackageManager;
        var pkg = ctx.PackageName;
        if (pm == null || string.IsNullOrEmpty(pkg)) return;

        if (!Aliases.TryGetValue(iconId, out var target))
            target = Aliases["classic"];

        // 1) Сначала включаем выбранный
        SetState(pm, pkg!, target, ComponentEnabledState.Enabled);

        // 2) Затем гасим остальные
        foreach (var name in Aliases.Values)
        {
            if (name == target) continue;
            SetState(pm, pkg!, name, ComponentEnabledState.Disabled);
        }
    }

    static void SetState(PackageManager pm, string pkg, string alias, ComponentEnabledState state)
    {
        try
        {
            var component = new ComponentName(pkg, $"{pkg}.{alias}");
            pm.SetComponentEnabledSetting(
                component,
                state,
                ComponentEnableOption.DontKillApp);
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Warn("IconSwitcher",
                $"Не удалось изменить {alias}: {ex.Message}");
        }
    }
}
