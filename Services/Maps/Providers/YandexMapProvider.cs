using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class YandexMapProvider : IMapProvider
{
    public MapProviderType Type => MapProviderType.Yandex;
    public string DisplayName => "Яндекс.Карты";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.YandexJsApiKey);

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        var markersJson = JsonSerializer.Serialize(markers.Select(m => new { lat = m.Lat, lng = m.Lng, title = m.Title, sub = m.Subtitle ?? "", isUser = m.IsUser }));
        var ci = CultureInfo.InvariantCulture;
        return $$"""
<!DOCTYPE html><html><head>
<meta name="viewport" content="width=device-width,initial-scale=1,user-scalable=no">
<style>html,body,#map{margin:0;padding:0;height:100%;width:100%;}</style>
<script src="https://api-maps.yandex.ru/2.1/?apikey={{MapConfig.YandexJsApiKey}}&lang=ru_RU"></script>
</head><body><div id="map"></div><script>
const MARKERS = {{markersJson}};
ymaps.ready(function(){
  const m = new ymaps.Map('map',{center:[{{lat.ToString(ci)}},{{lng.ToString(ci)}}],zoom:{{zoom}},controls:[]});
  MARKERS.forEach(function(x){
    const preset = x.isUser ? 'islands#blueDotIcon' : 'islands#violetDotIcon';
    m.geoObjects.add(new ymaps.Placemark([x.lat,x.lng],{balloonContentHeader:x.title,balloonContentBody:x.sub},{preset:preset}));
  });
});
</script></body></html>
""";
    }
}
