using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.OCR;
using Microsoft.Maui.Controls.Maps; 
using BarcodeScanning;


namespace FoodExpiryApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // 1. Настраиваем сам MAUI и подключаем библиотеки (цепочка Use...)
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeScanning()   
          //  .UseMauiMaps()
            .UseOcr();

        // 2. Регистрируем наши сервисы (отдельные команды)
         builder.Services.AddSingleton<Services.ProductImageService>();
         builder.Services.AddSingleton<ProductRepository>();
         builder.Services.AddSingleton<Services.DataMatrixCaptureService>();
         builder.Services.AddSingleton<Services.ExpiryOcrService>();
         builder.Services.AddSingleton<Services.OpenFoodFactsClient>();
         builder.Services.AddSingleton<Services.PriceService>();
         builder.Services.AddSingleton<Services.StoreService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
