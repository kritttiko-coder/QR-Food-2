namespace FoodExpiryApp.Services.Maps;

public sealed class StoreSearchService
{
    private readonly IReadOnlyList<IPlacesProvider> _providers;
    private readonly PlacesCache _cache;

    public StoreSearchService(IEnumerable<IPlacesProvider> providers, PlacesCache cache)
    {
        _providers = providers.ToList();
        _cache = cache;
    }

    public async Task<IReadOnlyList<NearbyStore>> FindAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        var cached = await _cache.GetAsync(lat, lng, radiusMeters);
        if (cached is { Count: > 0 }) return cached;

        var preferred = MapConfig.Preferred;
        var ordered = _providers
            .Where(p => p.IsConfigured)
            .OrderBy(p => p.Type == preferred ? 0 : p.Type == MapProviderType.Osm ? 2 : 1);

        foreach (var p in ordered)
        {
            try
            {
                var stores = await p.SearchStoresAsync(lat, lng, radiusMeters, ct);
                if (stores.Count > 0)
                {
                    await _cache.SetAsync(lat, lng, radiusMeters, stores);
                    return stores;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Places] {p.Type} failed: {ex.Message}"); }
        }
        return Array.Empty<NearbyStore>();
    }
}
