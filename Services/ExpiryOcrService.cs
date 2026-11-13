using Plugin.Maui.OCR;
using System.Text.RegularExpressions;

namespace FoodExpiryApp.Services;

public sealed class ExpiryOcrService
{
    private const int ContextWindow = 40;

    private static readonly Dictionary<string, int> RuMonths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["января"] = 1, ["февраля"] = 2, ["марта"] = 3, ["апреля"] = 4,
        ["мая"] = 5, ["июня"] = 6, ["июля"] = 7, ["августа"] = 8,
        ["сентября"] = 9, ["октября"] = 10, ["ноября"] = 11, ["декабря"] = 12
    };

    private static readonly Regex NumericDate = new(@"(?<d>\d{1,2})[.\-/](?<m>\d{1,2})[.\-/](?<y>\d{2,4})", RegexOptions.Compiled);
    private static readonly Regex MonthNameDate = new(@"(?<d>\d{1,2})\s+(?<mon>[а-яё]+)\s+(?<y>\d{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly string[] ExpiryKeywords = { "годен до", "годен", "годн", "гд", "exp", "expiry", "use by", "срок годн" };
    private static readonly string[] ProductionKeywords = { "изготов", "произв", "пр-ва", "пр ва", "дата пр", "mfg", "prod", "дата изг" };

     // «18 месяцев», «6 мес.», «24 месяца»
private static readonly Regex MonthsRelative = new(
    @"(?<n>\d{1,2})\s*(?:месяц|мес)", 
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

// «180 дней», «30 дн.»
private static readonly Regex DaysRelative = new(
    @"(?<n>\d{1,3})\s*(?:дн|день|дней)", 
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

// «2 года», «3 лет»
private static readonly Regex YearsRelative = new(
    @"(?<n>\d{1,2})\s*(?:год|года|лет)", 
    RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    private static IOcrService? _ocr;

    public async Task<DateTime?> ReadFromPhotoAsync(byte[] imageBytes, CancellationToken ct = default)
{
    
    _ocr ??= IPlatformApplication.Current?.Services.GetService<IOcrService>();
    
    if (_ocr == null) return null;
    await _ocr.InitAsync();
    var result = await _ocr.RecognizeTextAsync(imageBytes, ct: ct);
    if (string.IsNullOrWhiteSpace(result?.AllText)) return null;
    return PickExpiryDate(result.AllText);
    }

    private static DateTime? PickExpiryDate(string text)
{
    // сначала ищем относительные сроки (месяцы / дни / годы)
    var monthMatch = MonthsRelative.Match(text);
    if (monthMatch.Success && int.TryParse(monthMatch.Groups["n"].Value, out var months))
    {
        return AppClock.Now.Date.AddMonths(months);
    }

    var yearMatch = YearsRelative.Match(text);
    if (yearMatch.Success && int.TryParse(yearMatch.Groups["n"].Value, out var years))
    {
        return AppClock.Now.Date.AddYears(years);
    }

    var dayMatch = DaysRelative.Match(text);
    if (dayMatch.Success && int.TryParse(dayMatch.Groups["n"].Value, out var days))
    {
        return AppClock.Now.Date.AddDays(days);
    }

    // Если относительных не нашли — идём по старым правилам (конкретные даты)
    var candidates = CollectDateCandidates(text);
    if (candidates.Count == 0) return null;

    foreach (var c in candidates)
        if (HasAny(c.ContextBefore, ExpiryKeywords)) return c.Date;

    var notProduction = candidates.Where(c => !HasAny(c.ContextBefore, ProductionKeywords)).ToList();
    if (notProduction.Count == 0) return null;
    if (notProduction.Count == 1) return notProduction[0].Date;
    return notProduction.MaxBy(c => c.Date)!.Date;
    }

    private static bool HasAny(string ctx, string[] words)
    {
        if (string.IsNullOrEmpty(ctx)) return false;
        var lower = ctx.ToLowerInvariant();
        return words.Any(lower.Contains);
    }

    private sealed record DateCandidate(DateTime Date, string ContextBefore);

    private static List<DateCandidate> CollectDateCandidates(string text)
    {
        var list = new List<DateCandidate>();

        foreach (Match m in NumericDate.Matches(text))
        {
            var d = int.Parse(m.Groups["d"].Value);
            var mo = int.Parse(m.Groups["m"].Value);
            var y = int.Parse(m.Groups["y"].Value);
            if (y < 100) y += 2000;
            if (!TryDate(d, mo, y, out var dt)) continue;
            var start = Math.Max(0, m.Index - ContextWindow);
            list.Add(new DateCandidate(dt, text.Substring(start, m.Index - start)));
        }

        foreach (Match m in MonthNameDate.Matches(text))
        {
            if (!RuMonths.TryGetValue(m.Groups["mon"].Value, out var mo)) continue;
            var d = int.Parse(m.Groups["d"].Value);
            var y = int.Parse(m.Groups["y"].Value);
            if (!TryDate(d, mo, y, out var dt)) continue;
            var start = Math.Max(0, m.Index - ContextWindow);
            list.Add(new DateCandidate(dt, text.Substring(start, m.Index - start)));
        }
        return list;
    }

    private static bool TryDate(int d, int mo, int y, out DateTime dt)
    {
        dt = default;
        if (d < 1 || d > 31 || mo < 1 || mo > 12 || y < 2000 || y > 2100) return false;
        try { dt = new DateTime(y, mo, d); return true; } catch { return false; }
    }
}
