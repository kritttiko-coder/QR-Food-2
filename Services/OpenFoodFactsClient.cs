using System.Text.Json;

namespace FoodExpiryApp.Services;

public record ProductInfo(string? Name, string? Brand, string? Volume, string? ImageUrl);

public sealed class OpenFoodFactsClient
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private const string Url = "https://world.openfoodfacts.org/api/v2/product/{0}.json?fields=product_name,brands,quantity,image_url";

    public OpenFoodFactsClient()
    {
        _http.DefaultRequestHeaders.Add("User-Agent", "FoodExpiryApp/0.2");
    }

    public async Task<ProductInfo?> TryGetByGtinAsync(string gtin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(gtin)) return null;

        // GTIN-14 (с ведущим нулём) → EAN-13
        var ean = gtin.Length == 14 && gtin.StartsWith("0") ? gtin[1..] : gtin;

        try
        {
            var json = await _http.GetStringAsync(string.Format(Url, ean), ct);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.GetProperty("status").GetInt32() != 1) return null;

            var p = doc.RootElement.GetProperty("product");
            string? Get(string k) =>
                p.TryGetProperty(k, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

            return new ProductInfo(Get("product_name"), Get("brands"), Get("quantity"), Get("image_url"));
        }
        catch { return null; }
    }
}
