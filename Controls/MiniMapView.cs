using FoodExpiryApp.Services.Maps;
using Microsoft.Maui.Controls.Shapes;

namespace FoodExpiryApp.Controls;

public sealed class MiniMapView : ContentView
{
    private readonly WebView _web = new();
    private readonly IReadOnlyList<IMapProvider> _providers;

    public MiniMapView(IReadOnlyList<IMapProvider> providers)
    {
        _providers = providers;
        var border = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = 0,
            Content = _web
        };
        border.SetDynamicResource(Border.StrokeProperty, ThemeManager.Border);
        Content = border;
    }

    public void Show(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        var preferred = MapConfig.Preferred;
        var p = _providers.FirstOrDefault(x => x.Type == preferred && x.IsConfigured)
             ?? _providers.FirstOrDefault(x => x.Type == MapProviderType.Osm && x.IsConfigured)
             ?? _providers.FirstOrDefault(x => x.IsConfigured);
        if (p == null) return;

        _web.Source = new HtmlWebViewSource { Html = p.BuildMapHtml(lat, lng, zoom, markers) };
    }
}
