using System.Text.RegularExpressions;

namespace FoodExpiryApp.Services;

public sealed class DataMatrixCaptureService
{
    // GS1: 01 + 14 цифр GTIN, затем 21 + Serial (1-20 символов, включая спецсимволы)
    // \x1D (GS separator) используется как разделитель полей — убираем его заранее
    private static readonly Regex CisPattern = new(
        @"01(?<gtin>\d{14})21(?<serial>[!""%&'()*+,\-./:;<=>?_A-Za-z0-9]{1,20})",
        RegexOptions.Compiled);

    public (string Gtin, string Serial, string FullCode)? TryParse(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue)) return null;

        // Убираем непечатные символы (в т.ч. \x1D — разделитель GS1), но не трогаем видимые спецсимволы
        var clean = Regex.Replace(rawValue, @"[\x00-\x1F\x7F]", "");
        
        // Убираем префикс ]d2 (GS1 DataMatrix)
        if (clean.StartsWith("]d2")) clean = clean.Substring(3);

        var m = CisPattern.Match(clean);
        if (!m.Success) return null;

        return (m.Groups["gtin"].Value, m.Groups["serial"].Value, rawValue);
    }
}
