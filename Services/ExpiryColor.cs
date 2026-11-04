using FoodExpiryApp.Services;
using Microsoft.Maui.Graphics;

namespace FoodExpiryApp;

public static class ExpiryColor
{
    public static Color For(DateTime d)
    {
        int n = (d.Date - AppClock.Now.Date).Days;

        if (n < 0)   return Color.FromArgb("#B00035"); // просрочен
        if (n <= 3)  return Color.FromArgb("#FF414D"); // 0–3
        if (n <= 7)  return Color.FromArgb("#FF962E"); // 4–7
        if (n <= 14) return Color.FromArgb("#FF962E"); // 8–14
        if (n <= 21) return Color.FromArgb("#E8C928"); // 15–21
        return Color.FromArgb("#39C879");              // 22+
    }

    public static string Label(DateTime d)
    {
        int n = (d.Date - AppClock.Now.Date).Days;

        if (n < 0)  return $"Просрочен на {Math.Abs(n)} дн.";
        if (n == 0) return "Сегодня";
        if (n == 1) return "1 день";
        return $"{n} дней";
    }
}
