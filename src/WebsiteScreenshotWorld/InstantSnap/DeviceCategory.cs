using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The kind of device the page is rendered for.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct DeviceCategory : IEquatable<DeviceCategory>
{
    private readonly string? _value;

    /// <summary>Creates a device category from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>desktop</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public DeviceCategory(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>desktop</c>: A desktop browser.</summary>
    public static DeviceCategory Desktop { get; } = new("desktop");

    /// <summary><c>mobile</c>: A mobile device; see <see cref="CaptureOptions.EmulatedDevice"/>.</summary>
    public static DeviceCategory Mobile { get; } = new("mobile");

    /// <summary>Converts an API value to a device category.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator DeviceCategory(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(DeviceCategory left, DeviceCategory right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(DeviceCategory left, DeviceCategory right) => !left.Equals(right);

    /// <summary>Creates a device category from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static DeviceCategory FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(DeviceCategory other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DeviceCategory other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<DeviceCategory>
    {
        protected override DeviceCategory Create(string value) => new(value);
    }
}
