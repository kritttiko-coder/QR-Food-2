using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class MapboxPlacesProvider : IPlacesProvider
{
    private readonly HttpClient _http;
    private readonly string _accessToken;

    public MapboxPlacesProvider(HttpClient http, string accessToken)
    {
        _http = http;
        _accessToken = accessToken;
    }

    public MapProviderType Type => MapProviderType.Mapbox;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_accessToken);

    public async Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(
        double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<NearbyStore>();

        var ci = CultureInfo.InvariantCulture;
        // Mapbox Searchbox API — категория "supermarket"
        var url = "https://api.mapbox.com/search/searchbox/v1/category/supermarket" +
                  $"?proximity={lng.ToString("F6", ci)},{lat.ToString("F6", ci)}" +
                  $"&limit=25" +
                  $"&language=ru" +
                  $"&access_token={_accessToken}";

        try
        {
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<NearbyStore>();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            if (!doc.RootElement.TryGetProperty("features", out var features))
                return Array.Empty<NearbyStore>();

            var result = new List<NearbyStore>();
            foreach (var f in features.EnumerateArray())
            {
                try
                {
                    var props = f.GetProperty("properties");
                    string? name = props.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (string.IsNullOrEmpty(name)) continue;

                    if (!props.TryGetProperty("coordinates", out var coords)) continue;
                    var sLng = coords.GetProperty("longitude").GetDouble();
                    var sLat = coords.GetProperty("latitude").GetDouble();
                    var dist = Haversine(lat, lng, sLat, sLng);
                    if (dist > radiusMeters) continue;

                    string? address = props.TryGetProperty("full_address", out var a) ? a.GetString() : null;
                    string id = props.TryGetProperty("mapbox_id", out var mid)
                        ? mid.GetString() ?? Guid.NewGuid().ToString()
                        : Guid.NewGuid().ToString();

                    result.Add(new NearbyStore(id, name, address, sLat, sLng, dist, MapProviderType.Mapbox));
                }
                catch { }
            }
            return result.OrderBy(s => s.DistanceMeters).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Mapbox Places] {ex.Message}");
            return Array.Empty<NearbyStore>();
        }
    }

    private static double Haversine(double la1, double lo1, double la2, double lo2)
    {
        const double R = 6371000;
        double dLa = (la2 - la1) * Math.PI / 180;
        double dLo = (lo2 - lo1) * Math.PI / 180;
        double a = Math.Sin(dLa / 2) * Math.Sin(dLa / 2) +
                   Math.Cos(la1 * Math.PI / 180) * Math.Cos(la2 * Math.PI / 180) *
                   Math.Sin(dLo / 2) * Math.Sin(dLo / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
