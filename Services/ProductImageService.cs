namespace FoodExpiryApp.Services;

public class ProductImageService
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly string _dir = Path.Combine(FileSystem.AppDataDirectory, "product-images");

    public ProductImageService()
    {
        try { Directory.CreateDirectory(_dir); } catch { /* Если нет доступа к папке — не падаем */ }
    }

    public async Task<string?> DownloadToCacheAsync(string? url, string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        
        try
        {
            var path = PathFor(key);
            if (File.Exists(path)) return path;

            var bytes = await _http.GetByteArrayAsync(url, ct);
            if (bytes.Length < 512) return null;

            await File.WriteAllBytesAsync(path, bytes, ct);
            return path;
        }
        catch { return null; } // Защита от отсутствия интернета или места
    }

    public async Task<string?> CaptureOwnPhotoAsync(string key, CancellationToken ct = default)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) return null;
            
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return null;

            var path = PathFor(key);
            await using (var src = await photo.OpenReadAsync())
            await using (var dst = File.Create(path))
            {
                await src.CopyToAsync(dst, ct);
            }
            return path;
        }
        catch 
        { 
            // Сюда попадаем, если пользователь запретил камеру или произошла ошибка нативного плеера
            return null; 
        } 
    }

    public ImageSource? ResolveImageSource(Models.Product? product)
    {
        if (product is null) return null;
        
        try
        {
            if (product.HasLocalImage) return ImageSource.FromFile(product.LocalImagePath);
            
            if (!string.IsNullOrWhiteSpace(product.RemoteImageUrl))
            {
                if (Uri.TryCreate(product.RemoteImageUrl, UriKind.Absolute, out var uri))
                {
                    return new UriImageSource { Uri = uri, CachingEnabled = true, CacheValidity = TimeSpan.FromDays(30) };
                }
            }
        }
        catch { }
        
        return null;
    }

    private string PathFor(string key)
    {
        var safe = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '-'));
        if (string.IsNullOrEmpty(safe)) safe = Guid.NewGuid().ToString("N");
        return Path.Combine(_dir, $"{safe}.jpg");
    }
}
