using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>The product a webhook delivery comes from.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct WebhookProduct : IEquatable<WebhookProduct>
{
    private readonly string? _value;

    /// <summary>Creates a product from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>instant_snap</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public WebhookProduct(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>instant_snap</c>: Instant Snap.</summary>
    public static WebhookProduct InstantSnap { get; } = new("instant_snap");

    /// <summary><c>auto_schedule</c>: Auto Schedule.</summary>
    public static WebhookProduct AutoSchedule { get; } = new("auto_schedule");

    /// <summary>Converts an API value to a product.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator WebhookProduct(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(WebhookProduct left, WebhookProduct right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(WebhookProduct left, WebhookProduct right) => !left.Equals(right);

    /// <summary>Creates a product from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static WebhookProduct FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(WebhookProduct other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WebhookProduct other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<WebhookProduct>
    {
        protected override WebhookProduct Create(string value) => new(value);
    }
}
