using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>How much of the page is captured.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct ScrollMode : IEquatable<ScrollMode>
{
    private readonly string? _value;

    /// <summary>Creates a scroll mode from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>viewport</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public ScrollMode(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>viewport</c>: Only what fits in the viewport.</summary>
    public static ScrollMode Viewport { get; } = new("viewport");

    /// <summary><c>scroll</c>: The full page, scrolled from top to bottom.</summary>
    public static ScrollMode Scroll { get; } = new("scroll");

    /// <summary>Converts an API value to a scroll mode.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator ScrollMode(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(ScrollMode left, ScrollMode right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(ScrollMode left, ScrollMode right) => !left.Equals(right);

    /// <summary>Creates a scroll mode from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static ScrollMode FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(ScrollMode other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ScrollMode other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<ScrollMode>
    {
        protected override ScrollMode Create(string value) => new(value);
    }
}
