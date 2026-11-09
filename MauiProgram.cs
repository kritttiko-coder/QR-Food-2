using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace FoodExpiryApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()         
            .UseBarcodeReader() // <--- Для сканера
            .UseOcr();          // <--- Для распознавания текста

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
