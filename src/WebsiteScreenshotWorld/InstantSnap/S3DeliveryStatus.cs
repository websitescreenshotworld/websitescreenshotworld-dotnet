using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The state of an upload to the caller's bucket.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct S3DeliveryStatus : IEquatable<S3DeliveryStatus>
{
    private readonly string? _value;

    /// <summary>Creates a delivery status from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>not_started</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public S3DeliveryStatus(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>not_started</c>: The upload has not started.</summary>
    public static S3DeliveryStatus NotStarted { get; } = new("not_started");

    /// <summary><c>in_progress</c>: The upload is running.</summary>
    public static S3DeliveryStatus InProgress { get; } = new("in_progress");

    /// <summary><c>completed</c>: The file is in the bucket.</summary>
    public static S3DeliveryStatus Completed { get; } = new("completed");

    /// <summary><c>failed</c>: The upload failed; see <see cref="CaptureS3Delivery.Error"/>.</summary>
    public static S3DeliveryStatus Failed { get; } = new("failed");

    /// <summary>Converts an API value to a delivery status.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator S3DeliveryStatus(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(S3DeliveryStatus left, S3DeliveryStatus right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(S3DeliveryStatus left, S3DeliveryStatus right) => !left.Equals(right);

    /// <summary>Creates a delivery status from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static S3DeliveryStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(S3DeliveryStatus other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is S3DeliveryStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<S3DeliveryStatus>
    {
        protected override S3DeliveryStatus Create(string value) => new(value);
    }
}
