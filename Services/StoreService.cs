using FoodExpiryApp.Models;
using Microsoft.Maui.Devices.Sensors;

namespace FoodExpiryApp.Services;

public class StoreService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return null;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            return await Geolocation.Default.GetLocationAsync(request);
        }
        catch { return null; }
    }

    public async Task<string> GetCityNameAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            if (placemark != null)
            {
                return placemark.Locality ?? placemark.SubAdminArea ?? placemark.AdminArea ?? "Неизвестный город";
            }
        }
        catch { }
        return "Город не найден";
    }

    public List<StoreModel> GetNearbyStores(Location userLoc)
    {
        // В реальности тут был бы запрос к API (2GIS, Yandex). 
        // Сейчас мы генерируем магазины на расстоянии 300-700 метров от пользователя.
        return new List<StoreModel>
        {
            new() { Name = "Пятёрочка", Address = "ул. Ленина, 10", DistanceKm = 0.3, Logo = "5", Location = new Location(userLoc.Latitude + 0.002, userLoc.Longitude + 0.002) },
            new() { Name = "Магнит", Address = "пр. Победы, 45", DistanceKm = 0.5, Logo = "M", Location = new Location(userLoc.Latitude - 0.003, userLoc.Longitude + 0.001) },
            new() { Name = "Лента", Address = "ш. Кольцевое, 2", DistanceKm = 0.7, Logo = "Л", Location = new Location(userLoc.Latitude + 0.001, userLoc.Longitude - 0.004) }
        };
    }

    public List<ShoppingItem> GetTemplate(string templateName)
    {
        if (templateName == "Базовый")
        {
            return new List<ShoppingItem>
            {
                new() { Name = "Хлеб", Quantity = "1 шт.", Emoji = "🍞" },
                new() { Name = "Молоко", Quantity = "1 л", Emoji = "🥛" },
                new() { Name = "Яйца", Quantity = "10 шт.", Emoji = "🥚" }
            };
        }
        else if (templateName == "Для пикника")
        {
            return new List<ShoppingItem>
            {
                new() { Name = "Мясо", Quantity = "1 кг", Emoji = "🥩" },
                new() { Name = "Уголь", Quantity = "1 уп.", Emoji = "🔥" },
                new() { Name = "Кола", Quantity = "2 л", Emoji = "🥤" }
            };
        }
        return new List<ShoppingItem>();
    }
}
