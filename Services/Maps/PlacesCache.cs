using System.Text.Json;
using Microsoft.Maui.Storage;

namespace FoodExpiryApp.Services.Maps;

public sealed class PlacesCache
{
    private const string Key = "places_cache_v1";
    private const int TtlMinutes = 15;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private sealed class Entry { public DateTime CachedAt { get; set; } public List<NearbyStore> Stores { get; set; } = new(); }
    private sealed class Root { public Dictionary<string, Entry> Items { get; set; } = new(); }

    public async Task<IReadOnlyList<NearbyStore>?> GetAsync(double lat, double lng, int radius)
    {
        await _lock.WaitAsync();
        try
        {
            var root = Load();
            if (root.Items.TryGetValue(MakeKey(lat, lng, radius), out var e) && (DateTime.UtcNow - e.CachedAt).TotalMinutes < TtlMinutes)
                return e.Stores;
            return null;
        }
        finally { _lock.Release(); }
    }

    public async Task SetAsync(double lat, double lng, int radius, IReadOnlyList<NearbyStore> stores)
    {
        await _lock.WaitAsync();
        try
        {
            var root = Load();
            root.Items[MakeKey(lat, lng, radius)] = new Entry { CachedAt = DateTime.UtcNow, Stores = stores.ToList() };
            var cutoff = DateTime.UtcNow.AddHours(-1);
            foreach (var k in root.Items.Where(x => x.Value.CachedAt < cutoff).Select(x => x.Key).ToList())
                root.Items.Remove(k);
            Preferences.Default.Set(Key, JsonSerializer.Serialize(root));
        }
        finally { _lock.Release(); }
    }

    private Root Load()
    {
        try { var j = Preferences.Default.Get(Key, ""); return string.IsNullOrEmpty(j) ? new() : JsonSerializer.Deserialize<Root>(j) ?? new(); }
        catch { return new(); }
    }

    private static string MakeKey(double lat, double lng, int radius)
        => $"{Math.Round(lat, 3)}_{Math.Round(lng, 3)}_{radius / 100}";
}
