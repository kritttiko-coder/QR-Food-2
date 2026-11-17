namespace FoodExpiryApp.Services.Maps;

public interface IMapProvider
{
    MapProviderType Type { get; }
    string DisplayName { get; }
    bool IsConfigured { get; }
    string BuildMapHtml(double centerLat, double centerLng, int zoom, IReadOnlyList<MapMarker> markers);
}
