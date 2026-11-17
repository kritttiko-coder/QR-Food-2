namespace FoodExpiryApp.Services.Maps;

public sealed record MapMarker(double Lat, double Lng, string Title, string? Subtitle = null, bool IsUser = false);
