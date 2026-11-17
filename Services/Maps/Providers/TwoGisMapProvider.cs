using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class TwoGisMapProvider : IMapProvider
{
    public MapProviderType Type => MapProviderType.TwoGis;
    public string DisplayName => "2GIS";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(MapConfig.TwoGisMapGlKey);

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        var markersJson = JsonSerializer.Serialize(markers.Select(m => new { lat = m.Lat, lng = m.Lng, title = m.Title, sub = m.Subtitle ?? "", isUser = m.IsUser }));
        var ci = CultureInfo.InvariantCulture;
        return $$"""
<!DOCTYPE html><html><head>
<meta name="viewport" content="width=device-width,initial-scale=1,user-scalable=no">
<style>html,body,#map{margin:0;padding:0;height:100%;width:100%;}</style>
<script src="https://maps.api.2gis.ru/2.0/loader.js?pkg=full&apikey={{MapConfig.TwoGisMapGlKey}}"></script>
</head><body><div id="map"></div><script>
const MARKERS = {{markersJson}};
DG.then(function(){
  const m = DG.map('map',{center:[{{lat.ToString(ci)}},{{lng.ToString(ci)}}],zoom:{{zoom}},zoomControl:false});
  MARKERS.forEach(function(x){ DG.marker([x.lat,x.lng]).addTo(m).bindPopup(x.title+(x.sub?'<br>'+x.sub:'')); });
});
</script></body></html>
""";
    }
}
