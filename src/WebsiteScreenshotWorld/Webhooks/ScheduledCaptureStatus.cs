using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.Webhooks;

/// <summary>Where an Auto Schedule capture is in its life.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct ScheduledCaptureStatus : IEquatable<ScheduledCaptureStatus>
{
    private readonly string? _value;

    /// <summary>Creates a status from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>succeeded</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public ScheduledCaptureStatus(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>succeeded</c>: Captured.</summary>
    public static ScheduledCaptureStatus Succeeded { get; } = new("succeeded");

    /// <summary><c>failed</c>: The page could not be captured.</summary>
    public static ScheduledCaptureStatus Failed { get; } = new("failed");

    /// <summary><c>media_delivered</c>: Captured and delivered to every destination.</summary>
    public static ScheduledCaptureStatus MediaDelivered { get; } = new("media_delivered");

    /// <summary>Converts an API value to a status.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator ScheduledCaptureStatus(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(ScheduledCaptureStatus left, ScheduledCaptureStatus right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(ScheduledCaptureStatus left, ScheduledCaptureStatus right) => !left.Equals(right);

    /// <summary>Creates a status from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static ScheduledCaptureStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(ScheduledCaptureStatus other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ScheduledCaptureStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<ScheduledCaptureStatus>
    {
        protected override ScheduledCaptureStatus Create(string value) => new(value);
    }
}
