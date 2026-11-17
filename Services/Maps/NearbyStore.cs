namespace FoodExpiryApp.Services.Maps;

public sealed record NearbyStore(
    string Id,
    string Name,
    string? Address,
    double Lat,
    double Lng,
    double? DistanceMeters,
    MapProviderType Source);
