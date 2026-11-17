using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class TwoGisPlacesProvider : IPlacesProvider
{
    private readonly HttpClient _http;
    public MapProviderType Type => MapProviderType.TwoGis;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.TwoGisCatalogKey);

    public TwoGisPlacesProvider(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<NearbyStore>();
        var ci = CultureInfo.InvariantCulture;
        var url = $"https://catalog.api.2gis.com/3.0/items?q=супермаркет&type=branch" +
                  $"&point={lng.ToString("F6", ci)},{lat.ToString("F6", ci)}" +
                  $"&radius={radiusMeters}&page_size=30" +
                  $"&fields=items.point,items.name,items.address_name" +
                  $"&key={MapConfig.TwoGisCatalogKey}";

        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<NearbyStore>();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        if (!doc.RootElement.TryGetProperty("result", out var result) || !result.TryGetProperty("items", out var items))
            return Array.Empty<NearbyStore>();

        var stores = new List<NearbyStore>();
        foreach (var item in items.EnumerateArray())
        {
            try
            {
                string? name = item.TryGetProperty("name", out var n) ? n.GetString() : null;
                if (string.IsNullOrEmpty(name)) continue;
                if (!item.TryGetProperty("point", out var pt)) continue;
                var sLat = pt.GetProperty("lat").GetDouble();
                var sLng = pt.GetProperty("lon").GetDouble();
                string? addr = item.TryGetProperty("address_name", out var a) ? a.GetString() : null;
                string id = item.TryGetProperty("id", out var i) ? i.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();

                stores.Add(new NearbyStore(id, name, addr, sLat, sLng, Haversine(lat, lng, sLat, sLng), MapProviderType.TwoGis));
            }
            catch { }
        }
        return stores.OrderBy(s => s.DistanceMeters).ToList();
    }

    private static double Haversine(double la1, double lo1, double la2, double lo2)
    {
        const double R = 6371000;
        double dLa = (la2 - la1) * Math.PI / 180, dLo = (lo2 - lo1) * Math.PI / 180;
        double a = Math.Sin(dLa / 2) * Math.Sin(dLa / 2) + Math.Cos(la1 * Math.PI / 180) * Math.Cos(la2 * Math.PI / 180) * Math.Sin(dLo / 2) * Math.Sin(dLo / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
