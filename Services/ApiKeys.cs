public static class ApiKeys
{
    public static string YandexJs =>
        Environment.GetEnvironmentVariable("YANDEX_JS_KEY") ?? "";
    
    public static string YandexGeo =>
        Environment.GetEnvironmentVariable("YANDEX_GEO_KEY") ?? "";
    
    public static string TwoGisMap =>
        Environment.GetEnvironmentVariable("TWO_GIS_MAP_KEY") ?? "";
    
    public static string TwoGisCatalog =>
        Environment.GetEnvironmentVariable("TWO_GIS_CATALOG_KEY") ?? "";
    
    public static string Mapbox =>
        Environment.GetEnvironmentVariable("MAPBOX_KEY") ?? "";
}
