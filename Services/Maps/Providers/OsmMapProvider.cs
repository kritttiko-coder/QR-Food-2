using System.Globalization;
using System.Text.Json;

namespace FoodExpiryApp.Services.Maps.Providers;

public sealed class OsmMapProvider : IMapProvider
{
    public MapProviderType Type => MapProviderType.Osm;
    public string DisplayName => "OpenStreetMap";
    public bool IsConfigured => true;

    public string BuildMapHtml(double lat, double lng, int zoom, IReadOnlyList<MapMarker> markers)
    {
        var markersJson = JsonSerializer.Serialize(markers.Select(m => new { lat = m.Lat, lng = m.Lng, title = m.Title, sub = m.Subtitle ?? "", isUser = m.IsUser }));
        var ci = CultureInfo.InvariantCulture;
        return $$"""
<!DOCTYPE html><html><head>
<meta name="viewport" content="width=device-width,initial-scale=1,user-scalable=no">
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
<style>html,body,#map{margin:0;padding:0;height:100%;width:100%;background:#111116;}</style>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
</head><body><div id="map"></div><script>
const map = L.map('map',{zoomControl:false,attributionControl:false}).setView([{{lat.ToString(ci)}},{{lng.ToString(ci)}}],{{zoom}});
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png',{maxZoom:19}).addTo(map);
const ui = L.divIcon({className:'u',html:"<div style='background:#3188FF;width:18px;height:18px;border-radius:50%;border:3px solid #A978F5;box-shadow:0 0 12px #A978F5;'></div>",iconSize:[18,18],iconAnchor:[9,9]});
L.marker([{{lat.ToString(ci)}},{{lng.ToString(ci)}}],{icon:ui}).addTo(map);
const MARKERS = {{markersJson}};
MARKERS.forEach(m=>{if(m.isUser)return;
 const si = L.divIcon({className:'s',html:"<div style='background:#A978F5;width:32px;height:32px;border-radius:50%;display:flex;align-items:center;justify-content:center;color:white;font-weight:bold;box-shadow:0 2px 8px rgba(0,0,0,.6);'>🛒</div>",iconSize:[32,32],iconAnchor:[16,16]});
 L.marker([m.lat,m.lng],{icon:si}).addTo(map).bindPopup('<b>'+m.title+'</b>'+(m.sub?'<br>'+m.sub:''));
});
</script></body></html>
""";
    }
}
