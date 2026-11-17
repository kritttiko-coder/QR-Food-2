namespace FoodExpiryApp.Services.Maps;

public interface IPlacesProvider
{
    MapProviderType Type { get; }
    bool IsConfigured { get; }
    Task<IReadOnlyList<NearbyStore>> SearchStoresAsync(double lat, double lng, int radiusMeters, CancellationToken ct = default);
}
