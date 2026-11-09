using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.AutoSchedule;

/// <summary>What a trigger-now request did.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct TriggerStatus : IEquatable<TriggerStatus>
{
    private readonly string? _value;

    /// <summary>Creates a trigger status from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>triggered</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public TriggerStatus(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>triggered</c>: A capture was queued.</summary>
    public static TriggerStatus Triggered { get; } = new("triggered");

    /// <summary><c>already_pending</c>: A capture for the web page was already queued or running; nothing new was started or charged.</summary>
    public static TriggerStatus AlreadyPending { get; } = new("already_pending");

    /// <summary>Converts an API value to a trigger status.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator TriggerStatus(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(TriggerStatus left, TriggerStatus right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(TriggerStatus left, TriggerStatus right) => !left.Equals(right);

    /// <summary>Creates a trigger status from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static TriggerStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(TriggerStatus other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TriggerStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<TriggerStatus>
    {
        protected override TriggerStatus Create(string value) => new(value);
    }
}
