using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>How the API answers a capture request.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct ResponseType : IEquatable<ResponseType>
{
    private readonly string? _value;

    /// <summary>Creates a response type from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>json</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public ResponseType(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>json</c>: Metadata and a download link.</summary>
    public static ResponseType Json { get; } = new("json");

    /// <summary><c>by_format</c>: The file itself, in the response body.</summary>
    public static ResponseType ByFormat { get; } = new("by_format");

    /// <summary><c>cloud_storage</c>: Uploaded to the caller's own bucket.</summary>
    public static ResponseType CloudStorage { get; } = new("cloud_storage");

    /// <summary>Converts an API value to a response type.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator ResponseType(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(ResponseType left, ResponseType right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(ResponseType left, ResponseType right) => !left.Equals(right);

    /// <summary>Creates a response type from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static ResponseType FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(ResponseType other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ResponseType other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<ResponseType>
    {
        protected override ResponseType Create(string value) => new(value);
    }
}
