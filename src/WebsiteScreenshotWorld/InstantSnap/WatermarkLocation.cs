using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>Where the timestamp watermark strip goes.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct WatermarkLocation : IEquatable<WatermarkLocation>
{
    private readonly string? _value;

    /// <summary>Creates a watermark location from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>top</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public WatermarkLocation(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>top</c>: Above the page.</summary>
    public static WatermarkLocation Top { get; } = new("top");

    /// <summary><c>bottom</c>: Below the page.</summary>
    public static WatermarkLocation Bottom { get; } = new("bottom");

    /// <summary>Converts an API value to a watermark location.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator WatermarkLocation(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(WatermarkLocation left, WatermarkLocation right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(WatermarkLocation left, WatermarkLocation right) => !left.Equals(right);

    /// <summary>Creates a watermark location from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static WatermarkLocation FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(WatermarkLocation other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WatermarkLocation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<WatermarkLocation>
    {
        protected override WatermarkLocation Create(string value) => new(value);
    }
}
