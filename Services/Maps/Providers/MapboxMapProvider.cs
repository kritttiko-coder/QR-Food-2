using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class MapboxMapProvider : IMapProvider
{
    private readonly string _accessToken;

    public MapboxMapProvider(string accessToken) => _accessToken = accessToken;

    public MapProviderType Type => MapProviderType.Mapbox;
    public string DisplayName => "Mapbox";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_accessToken);

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        if (!IsConfigured)
            return "<html><body style='background:#111116;color:#888;display:flex;align-items:center;justify-content:center;font-family:sans-serif;text-align:center;padding:20px;'>Mapbox не настроен.<br>Добавьте токен в API-ключах.</body></html>";

        var markersJson = JsonSerializer.Serialize(markers.Select(m => new
        {
            lat = m.Lat,
            lng = m.Lng,
            title = m.Title,
            sub = m.Subtitle ?? "",
            isUser = m.IsUser
        }));

        var ci = CultureInfo.InvariantCulture;

        return $$"""
<!DOCTYPE html><html><head>
<meta name="viewport" content="width=device-width,initial-scale=1,user-scalable=no">
<link href="https://api.mapbox.com/mapbox-gl-js/v3.6.0/mapbox-gl.css" rel="stylesheet">
<style>html,body,#map{margin:0;padding:0;height:100%;width:100%;background:#111116;}</style>
<script src="https://api.mapbox.com/mapbox-gl-js/v3.6.0/mapbox-gl.js"></script>
</head><body>
<div id="map"></div>
<script>
mapboxgl.accessToken = '{{_accessToken}}';

const map = new mapboxgl.Map({
    container: 'map',
    style: 'mapbox://styles/mapbox/dark-v11',
    center: [{{lng.ToString(ci)}}, {{lat.ToString(ci)}}],
    zoom: {{zoom}},
    attributionControl: false
});

// Текущее местоположение
new mapboxgl.Marker({ color: '#3188FF' })
    .setLngLat([{{lng.ToString(ci)}}, {{lat.ToString(ci)}}])
    .addTo(map);

// Магазины
const MARKERS = {{markersJson}};
MARKERS.forEach(m => {
    if (m.isUser) return;
    new mapboxgl.Marker({ color: '#A978F5' })
        .setLngLat([m.lng, m.lat])
        .setPopup(new mapboxgl.Popup({ offset: 25 })
            .setHTML('<b>' + m.title + '</b>' + (m.sub ? '<br>' + m.sub : '')))
        .addTo(map);
});
</script>
</body></html>
""";
    }
}
