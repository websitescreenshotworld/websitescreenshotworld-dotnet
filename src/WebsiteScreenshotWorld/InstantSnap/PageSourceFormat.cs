using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The format the page source is kept in.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct PageSourceFormat : IEquatable<PageSourceFormat>
{
    private readonly string? _value;

    /// <summary>Creates a page source format from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>html</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public PageSourceFormat(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>html</c>: The rendered HTML.</summary>
    public static PageSourceFormat Html { get; } = new("html");

    /// <summary><c>json</c>: The page's JSON response.</summary>
    public static PageSourceFormat Json { get; } = new("json");

    /// <summary><c>text</c>: The page's visible text.</summary>
    public static PageSourceFormat Text { get; } = new("text");

    /// <summary>Converts an API value to a page source format.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator PageSourceFormat(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(PageSourceFormat left, PageSourceFormat right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(PageSourceFormat left, PageSourceFormat right) => !left.Equals(right);

    /// <summary>Creates a page source format from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static PageSourceFormat FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(PageSourceFormat other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PageSourceFormat other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<PageSourceFormat>
    {
        protected override PageSourceFormat Create(string value) => new(value);
    }
}
