namespace FoodExpiryApp.Services;

public static class ProductImageService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly string _dir = Path.Combine(FileSystem.AppDataDirectory, "product-images");

    static ProductImageService() => Directory.CreateDirectory(_dir);

    public static async Task<string?> DownloadToCacheAsync(string? url, string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var path = PathFor(key);
        if (File.Exists(path)) return path;
        try
        {
            var bytes = await _http.GetByteArrayAsync(url, ct);
            if (bytes.Length < 512) return null;
            await File.WriteAllBytesAsync(path, bytes, ct);
            return path;
        }
        catch { return null; }
    }

    private static string PathFor(string key)
    {
        var safe = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '-'));
        if (string.IsNullOrEmpty(safe)) safe = Guid.NewGuid().ToString("N");
        return Path.Combine(_dir, $"{safe}.jpg");
    }
}
