using Microsoft.Maui.Storage;

namespace FoodExpiryApp.Services;

public static class AppClock
{
    private const string ManualModeKey = "ManualTimeEnabled";
    private const string ManualTimeKey = "ManualTimeTicks";

    public static bool IsManual
    {
        get => Preferences.Get(ManualModeKey, false);
        set => Preferences.Set(ManualModeKey, value);
    }

    public static DateTime Now
    {
        get
        {
            if (!IsManual)
                return DateTime.Now;

            long ticks = Preferences.Get(
                ManualTimeKey,
                DateTime.Now.Ticks
            );

            return new DateTime(ticks, DateTimeKind.Local);
        }
    }

    public static void SetManualDateTime(DateTime value)
    {
        Preferences.Set(ManualTimeKey, value.Ticks);
        IsManual = true;
    }

    public static void UseSystemTime()
    {
        IsManual = false;
    }
}
