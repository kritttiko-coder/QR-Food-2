using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class OsmPlacesProvider : IPlacesProvider
{
    private readonly HttpClient _http;
    public MapProviderType Type => MapProviderType.Osm;
    public bool IsConfigured => true;

    public OsmPlacesProvider(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        var ci = CultureInfo.InvariantCulture;
        var query = $"""
[out:json][timeout:15];
(
  node["shop"~"supermarket|convenience|grocery|mall"](around:{radiusMeters},{lat.ToString(ci)},{lng.ToString(ci)});
  way["shop"~"supermarket|convenience|grocery|mall"](around:{radiusMeters},{lat.ToString(ci)},{lng.ToString(ci)});
);
out center 30;
""";
        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
        using var resp = await _http.PostAsync("https://overpass-api.de/api/interpreter", content, ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<NearbyStore>();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        if (!doc.RootElement.TryGetProperty("elements", out var elements)) return Array.Empty<NearbyStore>();

        var result = new List<NearbyStore>();
        foreach (var el in elements.EnumerateArray())
        {
            var id = el.GetProperty("id").GetInt64().ToString();
            if (!el.TryGetProperty("tags", out var tags)) continue;
            string? name = tags.TryGetProperty("name", out var n) ? n.GetString() : null;
            if (string.IsNullOrEmpty(name)) continue;

            double sLat, sLng;
            if (el.TryGetProperty("lat", out var la)) { sLat = la.GetDouble(); sLng = el.GetProperty("lon").GetDouble(); }
            else if (el.TryGetProperty("center", out var c)) { sLat = c.GetProperty("lat").GetDouble(); sLng = c.GetProperty("lon").GetDouble(); }
            else continue;

            string? address = tags.TryGetProperty("addr:street", out var st) ? st.GetString() : null;
            if (address != null && tags.TryGetProperty("addr:housenumber", out var hn)) address += ", " + hn.GetString();

            result.Add(new NearbyStore(id, name, address, sLat, sLng, Haversine(lat, lng, sLat, sLng), MapProviderType.Osm));
        }
        return result.OrderBy(s => s.DistanceMeters).ToList();
    }

    private static double Haversine(double la1, double lo1, double la2, double lo2)
    {
        const double R = 6371000;
        double dLa = (la2 - la1) * Math.PI / 180, dLo = (lo2 - lo1) * Math.PI / 180;
        double a = Math.Sin(dLa / 2) * Math.Sin(dLa / 2) + Math.Cos(la1 * Math.PI / 180) * Math.Cos(la2 * Math.PI / 180) * Math.Sin(dLo / 2) * Math.Sin(dLo / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
