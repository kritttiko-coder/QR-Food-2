using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class YandexPlacesProvider : IPlacesProvider
{
    private readonly HttpClient _http;
    public MapProviderType Type => MapProviderType.Yandex;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.YandexGeosearchKey);

    public YandexPlacesProvider(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        if (!IsConfigured) return Array.Empty<NearbyStore>();
        var ci = CultureInfo.InvariantCulture;
        var url = $"https://search-maps.yandex.ru/v1/?apikey={MapConfig.YandexGeosearchKey}" +
                  $"&text=супермаркет&type=biz&lang=ru_RU" +
                  $"&ll={lng.ToString("F6", ci)},{lat.ToString("F6", ci)}" +
                  $"&spn=0.05,0.05&rspn=1&results=30";

        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return Array.Empty<NearbyStore>();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        if (!doc.RootElement.TryGetProperty("features", out var features)) return Array.Empty<NearbyStore>();

        var result = new List<NearbyStore>();
        foreach (var f in features.EnumerateArray())
        {
            try
            {
                var props = f.GetProperty("properties");
                if (!props.TryGetProperty("CompanyMetaData", out var company)) continue;
                var coords = f.GetProperty("geometry").GetProperty("coordinates");
                var sLat = coords[1].GetDouble();
                var sLng = coords[0].GetDouble();
                var dist = Haversine(lat, lng, sLat, sLng);
                if (dist > radiusMeters) continue;

                string? name = company.TryGetProperty("name", out var n) ? n.GetString() : null;
                if (string.IsNullOrEmpty(name)) continue;
                string? addr = company.TryGetProperty("address", out var a) ? a.GetString() : null;
                string id = company.TryGetProperty("id", out var i) ? i.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();

                result.Add(new NearbyStore(id, name, addr, sLat, sLng, dist, MapProviderType.Yandex));
            }
            catch { }
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
