using System.Text.Json;

namespace FoodExpiryApp.Services;

public static class SecretsLoader
{
    public static async Task LoadAsync()
    {
        // Пробуем прочитать локальный файл (для разработчика)
        try
        {
            var file = Path.Combine(FileSystem.AppDataDirectory, "appsettings.local.json");
            if (!File.Exists(file))
            {
                // На эмуляторе/устройстве файла нет — пробуем ресурс сборки (тоже gitignored)
                using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.local.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                await ApplyKeysAsync(json);
                return;
            }
            var localJson = await File.ReadAllTextAsync(file);
            await ApplyKeysAsync(localJson);
        }
        catch
        {
            // Файла нет — это нормально, ключи введутся через UI
        }
    }

    private static async Task ApplyKeysAsync(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("Maps", out var maps))
        {
            if (maps.TryGetProperty("Yandex", out var y))
            {
                var js = y.TryGetProperty("JsApiKey", out var k1) ? k1.GetString() : null;
                var gs = y.TryGetProperty("GeosearchKey", out var k2) ? k2.GetString() : null;
                if (!string.IsNullOrEmpty(js)) await SecureStorage.Default.SetAsync("map_yandex_js_key", js);
                if (!string.IsNullOrEmpty(gs)) await SecureStorage.Default.SetAsync("map_yandex_geosearch_key", gs);
            }
            if (maps.TryGetProperty("TwoGis", out var g))
            {
                var mg = g.TryGetProperty("MapGlKey", out var k3) ? k3.GetString() : null;
                var cat = g.TryGetProperty("CatalogKey", out var k4) ? k4.GetString() : null;
                if (!string.IsNullOrEmpty(mg)) await SecureStorage.Default.SetAsync("map_2gis_mapgl_key", mg);
                if (!string.IsNullOrEmpty(cat)) await SecureStorage.Default.SetAsync("map_2gis_catalog_key", cat);
            }
            if (maps.TryGetProperty("Mapbox", out var mb))
            {
                var t = mb.TryGetProperty("Token", out var k5) ? k5.GetString() : null;
                if (!string.IsNullOrEmpty(t)) await SecureStorage.Default.SetAsync("map_mapbox_token", t);
            }
        }
    }
}
