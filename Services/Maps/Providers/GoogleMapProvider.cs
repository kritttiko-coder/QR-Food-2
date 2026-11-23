using System.Globalization;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class GoogleMapProvider : IMapProvider
{
    public MapProviderType Type => MapProviderType.Google;
    public string DisplayName => "Google Maps";
    public bool IsConfigured => true; // embed работает без ключа

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        var ci = CultureInfo.InvariantCulture;
        // Google Maps embed iframe — без ключа, показывает центр и 1 маркер.
        // Кастомные маркеры магазинов тут не поддерживаются (ограничение Google embed).
        var embedUrl = $"https://maps.google.com/maps?q={lat.ToString(ci)},{lng.ToString(ci)}&z={zoom}&output=embed";

        return $$"""
<!DOCTYPE html><html><head>
<meta name="viewport" content="width=device-width,initial-scale=1,user-scalable=no">
<style>html,body,iframe{margin:0;padding:0;height:100%;width:100%;border:0;}</style>
</head><body>
<iframe src="{{embedUrl}}" allowfullscreen loading="lazy" referrerpolicy="no-referrer-when-downgrade"></iframe>
</body></html>
""";
    }
}
