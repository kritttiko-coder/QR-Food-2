using Microsoft.Maui.Storage;

namespace FoodExpiryApp.Services.Maps;

public static class MapConfig
{
    public static string YandexJsApiKey => Preferences.Default.Get("map_yandex_js_key", "");
    public static string YandexGeosearchKey => Preferences.Default.Get("map_yandex_geosearch_key", "");
    public static string TwoGisMapGlKey => Preferences.Default.Get("map_2gis_mapgl_key", "");
    public static string TwoGisCatalogKey => Preferences.Default.Get("map_2gis_catalog_key", "");
    public static string MapboxToken => Preferences.Default.Get("map_mapbox_token", "");

    public static MapProviderType Preferred
    {
        get
        {
            var stored = Preferences.Default.Get("preferred_map", "Osm");
            return Enum.TryParse<MapProviderType>(stored, true, out var t) ? t : MapProviderType.Osm;
        }
    }

    public static void SetPreferred(MapProviderType type)
        => Preferences.Default.Set("preferred_map", type.ToString());
}
