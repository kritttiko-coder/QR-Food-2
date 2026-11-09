using System.Text.RegularExpressions;

namespace FoodExpiryApp.Services;

public sealed class DataMatrixCaptureService
{
    // Ищем 01 + 14 цифр + 21 + до 13 символов. 
    // Убираем жесткую привязку к началу строки (^), так как ZXing может добавлять префиксы.
    private static readonly Regex CisPattern = new(
        @"01(?<gtin>\d{14})21(?<serial>[A-Za-z0-9\-/]{1,13})",
        RegexOptions.Compiled);

    public (string Gtin, string Serial, string FullCode)? TryParse(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue)) return null;

        // Удаляем непечатные символы (например, Group Separator \u001D) и префиксы типа ]d2
        var clean = Regex.Replace(rawValue, @"[^\x20-\x7E]", "");
        if (clean.StartsWith("]d2")) clean = clean.Substring(3);

        var m = CisPattern.Match(clean);
        if (!m.Success) return null;

        return (m.Groups["gtin"].Value, m.Groups["serial"].Value, rawValue);
    }
}
