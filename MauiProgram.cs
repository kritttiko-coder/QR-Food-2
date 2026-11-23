using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.OCR;
using BarcodeScanning;

namespace FoodExpiryApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // === MAUI + библиотеки ===
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeScanning()
            .UseOcr();

        // === Базовые сервисы ===
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ProductRepository>();
        builder.Services.AddSingleton<Services.ProductImageService>();
        builder.Services.AddSingleton<Services.DataMatrixCaptureService>();
        builder.Services.AddSingleton<Services.ExpiryOcrService>();
        builder.Services.AddSingleton<Services.OpenFoodFactsClient>();
        builder.Services.AddSingleton<Services.PriceService>();
        builder.Services.AddSingleton<Services.StoreService>();

        // === Карты: кеш и оркестратор ===
        builder.Services.AddSingleton<Services.Maps.PlacesCache>();
        builder.Services.AddSingleton<Services.Maps.StoreSearchService>();

        // Ключи
string yandexJs = ApiKeys.YandexJs;
string yandexGeo = ApiKeys.YandexGeo;
string twoGisMap = ApiKeys.TwoGisMap;
string twoGisCat = ApiKeys.TwoGisCatalog;
string mapboxToken = ApiKeys.Mapbox;

        // === Map-провайдеры ===
        builder.Services.AddSingleton<Services.Maps.IMapProvider, Services.Maps.Providers.OsmMapProvider>();
        builder.Services.AddSingleton<Services.Maps.IMapProvider, Services.Maps.Providers.YandexMapProvider>();
        builder.Services.AddSingleton<Services.Maps.IMapProvider, Services.Maps.Providers.TwoGisMapProvider>();
        builder.Services.AddSingleton<Services.Maps.IMapProvider, Services.Maps.Providers.MapboxMapProvider>();
        builder.Services.AddSingleton<Services.Maps.IMapProvider, Services.Maps.Providers.GoogleMapProvider>();

        // === Places-провайдеры ===
        builder.Services.AddSingleton<Services.Maps.IPlacesProvider, Services.Maps.Providers.OsmPlacesProvider>();
        builder.Services.AddSingleton<Services.Maps.IPlacesProvider, Services.Maps.Providers.YandexPlacesProvider>();
        builder.Services.AddSingleton<Services.Maps.IPlacesProvider, Services.Maps.Providers.TwoGisPlacesProvider>();
        builder.Services.AddSingleton<Services.Maps.IPlacesProvider, Services.Maps.Providers.MapboxPlacesProvider>();
        builder.Services.AddSingleton<Services.Maps.IPlacesProvider, Services.Maps.Providers.GooglePlacesProvider>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
