using System.Globalization;
using System.Text.RegularExpressions;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>Parses the API's ISO 8601 timestamps leniently.</summary>
internal static class Iso8601
{
    // DateTimeOffset parsing accepts at most seven fractional digits.
    private static readonly Regex LongFraction = new(@"(\.\d{7})\d+", RegexOptions.CultureInvariant);

    public static bool TryParse(string? value, out DateTimeOffset result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = LongFraction.Replace(value!.Trim(), "$1");
        if (!DateTimeOffset.TryParse(
                trimmed,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            return false;
        }

        result = parsed.ToUniversalTime();
        return true;
    }
}
