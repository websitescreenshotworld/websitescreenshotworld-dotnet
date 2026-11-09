using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.AutoSchedule;

/// <summary>Who started a scheduled capture.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct TriggeredBy : IEquatable<TriggeredBy>
{
    private readonly string? _value;

    /// <summary>Creates a trigger source from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>api</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public TriggeredBy(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>api</c>: The Auto Schedule API.</summary>
    public static TriggeredBy Api { get; } = new("api");

    /// <summary><c>ui</c>: The dashboard.</summary>
    public static TriggeredBy Ui { get; } = new("ui");

    /// <summary>Converts an API value to a trigger source.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator TriggeredBy(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(TriggeredBy left, TriggeredBy right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(TriggeredBy left, TriggeredBy right) => !left.Equals(right);

    /// <summary>Creates a trigger source from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static TriggeredBy FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(TriggeredBy other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TriggeredBy other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<TriggeredBy>
    {
        protected override TriggeredBy Create(string value) => new(value);
    }
}
