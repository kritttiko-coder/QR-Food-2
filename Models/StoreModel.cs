using Microsoft.Maui.Devices.Sensors;

namespace FoodExpiryApp.Models;

public class StoreModel
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public Location Location { get; set; } = null!;
    public double DistanceKm { get; set; }
    public string Logo { get; set; } = ""; // Буква или текст для аватарки (например, "5", "M", "Л")
}
