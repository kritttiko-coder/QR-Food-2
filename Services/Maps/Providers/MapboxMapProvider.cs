namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class MapboxMapProvider : IMapProvider
{
    public MapProviderType Type => MapProviderType.Mapbox;
    public string DisplayName => "Mapbox";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.MapboxToken);

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        if (!IsConfigured)
            return "<html><body style='background:#111116;color:#888;display:flex;align-items:center;justify-content:center;font-family:sans-serif;text-align:center;padding:20px;'>Mapbox не настроен.<br>Добавьте токен в настройках профиля.</body></html>";
        return string.Empty; // TODO: когда появится токен
    }
}

public sealed class MapboxPlacesProvider : IPlacesProvider
{
    public MapProviderType Type => MapProviderType.Mapbox;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.MapboxToken);

    public Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<NearbyStore>>(Array.Empty<NearbyStore>());
}
