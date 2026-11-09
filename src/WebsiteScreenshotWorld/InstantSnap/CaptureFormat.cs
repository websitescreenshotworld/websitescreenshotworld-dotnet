using System.Text.Json.Serialization;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>The file format of a capture. Each format belongs to one <see cref="MediaCategory"/>.</summary>
/// <remarks>
/// Values the API adds later come through unchanged, so compare with the static properties rather than
/// switching on a closed set. Comparison ignores case.
/// </remarks>
[JsonConverter(typeof(Converter))]
public readonly struct CaptureFormat : IEquatable<CaptureFormat>
{
    private readonly string? _value;

    /// <summary>Creates a format from the value the API uses.</summary>
    /// <param name="value">The API value, such as <c>png</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public CaptureFormat(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary><c>png</c>: PNG image.</summary>
    public static CaptureFormat Png { get; } = new("png");

    /// <summary><c>jpeg</c>: JPEG image.</summary>
    public static CaptureFormat Jpeg { get; } = new("jpeg");

    /// <summary><c>webp</c>: WebP image.</summary>
    public static CaptureFormat WebP { get; } = new("webp");

    /// <summary><c>tiff</c>: TIFF image.</summary>
    public static CaptureFormat Tiff { get; } = new("tiff");

    /// <summary><c>mp4</c>: MP4 video.</summary>
    public static CaptureFormat Mp4 { get; } = new("mp4");

    /// <summary><c>webm</c>: WebM video.</summary>
    public static CaptureFormat WebM { get; } = new("webm");

    /// <summary><c>mov</c>: QuickTime video.</summary>
    public static CaptureFormat Mov { get; } = new("mov");

    /// <summary><c>gif</c>: Animated GIF.</summary>
    public static CaptureFormat Gif { get; } = new("gif");

    /// <summary><c>image_pdf</c>: A PDF holding the page as an image.</summary>
    public static CaptureFormat ImagePdf { get; } = new("image_pdf");

    /// <summary><c>document_pdf</c>: A PDF printed from the page, with selectable text.</summary>
    public static CaptureFormat DocumentPdf { get; } = new("document_pdf");

    /// <summary>Converts an API value to a format.</summary>
    /// <param name="value">The API value.</param>
    public static implicit operator CaptureFormat(string value) => FromString(value);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator ==(CaptureFormat left, CaptureFormat right) => left.Equals(right);

    /// <summary>Compares two values, ignoring case.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    public static bool operator !=(CaptureFormat left, CaptureFormat right) => !left.Equals(right);

    /// <summary>Creates a format from the value the API uses.</summary>
    /// <param name="value">The API value.</param>
    /// <returns>The token.</returns>
    public static CaptureFormat FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(CaptureFormat other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CaptureFormat other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(_value);

    /// <summary>Returns the value as the API spells it.</summary>
    public override string ToString() => _value ?? string.Empty;

    internal sealed class Converter : StringTokenConverter<CaptureFormat>
    {
        protected override CaptureFormat Create(string value) => new(value);
    }
}
