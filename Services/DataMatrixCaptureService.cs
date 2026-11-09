using System.Text.RegularExpressions;

namespace FoodExpiryApp.Services;

public sealed class DataMatrixCaptureService
{
    private static readonly Regex CisPattern = new(
        @"^(?:01)(?<gtin>\d{14})(?:21)(?<serial>[A-Za-z0-9\-/]{1,13})",
        RegexOptions.Compiled);

    public (string Gtin, string Serial, string FullCode)? TryParse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var trimmed = raw.Trim();
        var m = CisPattern.Match(trimmed);
        if (!m.Success) return null;
        return (m.Groups["gtin"].Value, m.Groups["serial"].Value, trimmed);
    }
}
