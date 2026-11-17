using System.Globalization;

namespace FoodExpiryApp.Services.Maps;

public static class MapRegionResolver
{
    private static readonly HashSet<string> CisCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "RU", "BY", "KZ", "UA", "UZ", "KG", "TJ", "AM", "AZ", "GE", "MD", "TR"
    };

    public static string? GuessIso2()
    {
        try
        {
            var r = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            return string.IsNullOrEmpty(r) ? null : r;
        }
        catch { return null; }
    }

    public static bool IsCisCountry() => GuessIso2() is { } iso && CisCountries.Contains(iso);
}
