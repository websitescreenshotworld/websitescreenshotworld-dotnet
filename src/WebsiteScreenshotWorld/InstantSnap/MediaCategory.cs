using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The kind of media a capture produces.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct MediaCategory : IEquatable<MediaCategory>
{
    private readonly string? _value;

    /// <summary>Creates a media category from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>image</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public MediaCategory(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>image</c>: A still image.</summary>
    public static MediaCategory Image { get; } = new("image");

    /// <summary><c>video</c>: A scrolling video.</summary>
    public static MediaCategory Video { get; } = new("video");

    /// <summary><c>pdf</c>: A PDF document.</summary>
    public static MediaCategory Pdf { get; } = new("pdf");

    /// <summary>Converts an API value to a media category.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator MediaCategory(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(MediaCategory left, MediaCategory right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(MediaCategory left, MediaCategory right) => !left.Equals(right);

    /// <summary>Creates a media category from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static MediaCategory FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(MediaCategory other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MediaCategory other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<MediaCategory>
    {
        protected override MediaCategory Create(string value) => new(value);
    }
}
