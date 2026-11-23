namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class GooglePlacesProvider : IPlacesProvider
{
    public MapProviderType Type => MapProviderType.Google;
    public bool IsConfigured => false;

    public Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(
        double lat, double lng, int radiusMeters, CancellationToken ct = default)
    {
        // Google Places API требует ключ, который нельзя защитить package name + SHA-1.
        // Единственный вариант — серверный прокси. Пока отключено.
        return Task.FromResult<IReadOnlyList<NearbyStore>>(Array.Empty<NearbyStore>());
    }
}
