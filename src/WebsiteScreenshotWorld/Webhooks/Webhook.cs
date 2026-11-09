using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>Verifies webhook deliveries and reads them into typed events.</summary>
public static class Webhook
{
    /// <summary>The header that carries the signature: <c>X-WSW-Signature</c>.</summary>
    public const string SignatureHeaderName = "X-WSW-Signature";

    /// <summary>How far the signature's timestamp may be from now unless told otherwise: 5 minutes.</summary>
    public static TimeSpan DefaultTolerance { get; } = TimeSpan.FromMinutes(5);

    /// <summary>Checks a delivery's signature and returns its event.</summary>
    /// <param name="body">The request body exactly as received. Text that was parsed and re-serialised will not verify.</param>
    /// <param name="signatureHeader">The value of the <c>X-WSW-Signature</c> header.</param>
    /// <param name="secret">The endpoint's signing secret from the dashboard.</param>
    /// <param name="tolerance">How far the signature's timestamp may be from now. Defaults to <see cref="DefaultTolerance"/>.</param>
    /// <param name="now">The current time, for tests. Defaults to the system clock.</param>
    /// <returns>The event, typed by its <c>type</c> and <c>product</c>.</returns>
    /// <exception cref="SignatureVerificationException">The signature is missing, malformed, too old or too new, or does not match.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">The body is signed but is not a webhook event.</exception>
    public static WebhookEvent Verify(
        string body,
        string signatureHeader,
        string secret,
        TimeSpan? tolerance = null,
        DateTimeOffset? now = null)
    {
        Guard.NotNull(body, nameof(body));
        return Verify(Encoding.UTF8.GetBytes(body).AsSpan(), signatureHeader, secret, tolerance, now);
    }

    /// <summary>Checks a delivery's signature and returns its event.</summary>
    /// <param name="body">The request body bytes exactly as received.</param>
    /// <param name="signatureHeader">The value of the <c>X-WSW-Signature</c> header.</param>
    /// <param name="secret">The endpoint's signing secret from the dashboard.</param>
    /// <param name="tolerance">How far the signature's timestamp may be from now. Defaults to <see cref="DefaultTolerance"/>.</param>
    /// <param name="now">The current time, for tests. Defaults to the system clock.</param>
    /// <returns>The event, typed by its <c>type</c> and <c>product</c>.</returns>
    /// <exception cref="SignatureVerificationException">The signature is missing, malformed, too old or too new, or does not match.</exception>
    /// <exception cref="WebsiteScreenshotWorldException">The body is signed but is not a webhook event.</exception>
    public static WebhookEvent Verify(
        ReadOnlySpan<byte> body,
        string signatureHeader,
        string secret,
        TimeSpan? tolerance = null,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentException("The signing secret is required.", nameof(secret));
        }

        var allowed = tolerance ?? DefaultTolerance;
        if (allowed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), allowed, "The tolerance cannot be negative.");
        }

        var signature = SignatureHeader.Parse(signatureHeader);
        var current = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        if (Math.Abs((double)current - signature.Timestamp) > allowed.TotalSeconds)
        {
            throw new SignatureVerificationException("The signature's timestamp is outside the tolerance.");
        }

        var expected = ComputeSignature(secret, signature.TimestampText, body);
        var matched = false;
        foreach (var candidate in signature.Signatures)
        {
            matched |= FixedTimeEquals(candidate, expected);
        }

        if (!matched)
        {
            throw new SignatureVerificationException("No signature matches the body.");
        }

        return WebhookEventReader.Read(body);
    }

    private static byte[] ComputeSignature(string secret, string timestamp, ReadOnlySpan<byte> body)
    {
        var prefix = Encoding.UTF8.GetBytes(timestamp + ".");
        var payload = new byte[prefix.Length + body.Length];
        prefix.CopyTo(payload, 0);
        body.CopyTo(payload.AsSpan(prefix.Length));

        var key = Encoding.UTF8.GetBytes(secret);
#if NET
        return HMACSHA256.HashData(key, payload);
#else
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(payload);
#endif
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
#if NET
        return CryptographicOperations.FixedTimeEquals(left, right);
#else
        if (left.Length != right.Length)
        {
            return false;
        }

        var difference = 0;
        for (var i = 0; i < left.Length; i++)
        {
            difference |= left[i] ^ right[i];
        }

        return difference == 0;
#endif
    }

    private sealed class SignatureHeader
    {
        private SignatureHeader(string timestampText, long timestamp, IReadOnlyList<byte[]> signatures)
        {
            TimestampText = timestampText;
            Timestamp = timestamp;
            Signatures = signatures;
        }

        public string TimestampText { get; }

        public long Timestamp { get; }

        public IReadOnlyList<byte[]> Signatures { get; }

        public static SignatureHeader Parse(string? header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                throw new SignatureVerificationException($"The {SignatureHeaderName} header is missing.");
            }

            string? timestampText = null;
            var signatures = new List<byte[]>();
            var sawSignature = false;

            foreach (var part in header!.Split(','))
            {
                var equals = part.IndexOf('=');
                if (equals < 0)
                {
                    continue;
                }

                var name = part.Substring(0, equals).Trim();
                var value = part.Substring(equals + 1).Trim();
                if (name == "t")
                {
                    timestampText ??= value;
                }
                else if (name == "v1")
                {
                    sawSignature = true;
                    if (TryParseHex(value, out var bytes))
                    {
                        signatures.Add(bytes);
                    }
                }
            }

            if (timestampText is null)
            {
                throw new SignatureVerificationException($"The {SignatureHeaderName} header has no timestamp.");
            }

            if (!long.TryParse(timestampText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var timestamp))
            {
                throw new SignatureVerificationException($"The {SignatureHeaderName} header's timestamp is not a whole number.");
            }

            if (!sawSignature)
            {
                throw new SignatureVerificationException($"The {SignatureHeaderName} header has no v1 signature.");
            }

            return new SignatureHeader(timestampText, timestamp, signatures);
        }

        private static bool TryParseHex(string value, out byte[] bytes)
        {
            bytes = [];
            if (value.Length == 0 || value.Length % 2 != 0)
            {
                return false;
            }

            var result = new byte[value.Length / 2];
            for (var i = 0; i < result.Length; i++)
            {
                var high = HexDigit(value[i * 2]);
                var low = HexDigit(value[(i * 2) + 1]);
                if (high < 0 || low < 0)
                {
                    return false;
                }

                result[i] = (byte)((high << 4) | low);
            }

            bytes = result;
            return true;
        }

        private static int HexDigit(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => -1,
        };
    }
}
