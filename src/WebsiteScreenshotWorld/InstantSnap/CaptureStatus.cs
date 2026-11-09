using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>Where an Instant Snap capture is in its life.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct CaptureStatus : IEquatable<CaptureStatus>
{
    private readonly string? _value;

    /// <summary>Creates a status from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>pending</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public CaptureStatus(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>pending</c>: Not finished yet.</summary>
    public static CaptureStatus Pending { get; } = new("pending");

    /// <summary><c>queued</c>: An async capture accepted and waiting for a browser.</summary>
    public static CaptureStatus Queued { get; } = new("queued");

    /// <summary><c>running</c>: A browser is taking an async capture.</summary>
    public static CaptureStatus Running { get; } = new("running");

    /// <summary><c>succeeded</c>: Captured; the file can be downloaded until it expires.</summary>
    public static CaptureStatus Succeeded { get; } = new("succeeded");

    /// <summary><c>delivered</c>: A <c>by_format</c> capture whose file went back in the response and is not kept.</summary>
    public static CaptureStatus Delivered { get; } = new("delivered");

    /// <summary>
    /// <c>failed</c>: The page could not be captured; see <see cref="Capture.ErrorCode"/>. An async capture still
    /// waiting 15 minutes after it was accepted fails with <c>capture_expired</c> and is not charged.
    /// </summary>
    public static CaptureStatus Failed { get; } = new("failed");

    /// <summary><c>timed_out</c>: The page did not finish in time.</summary>
    public static CaptureStatus TimedOut { get; } = new("timed_out");

    /// <summary><c>expired</c>: The file has been deleted.</summary>
    public static CaptureStatus Expired { get; } = new("expired");

    /// <summary>Converts an API value to a status.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator CaptureStatus(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(CaptureStatus left, CaptureStatus right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(CaptureStatus left, CaptureStatus right) => !left.Equals(right);

    /// <summary>Creates a status from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static CaptureStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(CaptureStatus other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CaptureStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<CaptureStatus>
    {
        protected override CaptureStatus Create(string value) => new(value);
    }
}
