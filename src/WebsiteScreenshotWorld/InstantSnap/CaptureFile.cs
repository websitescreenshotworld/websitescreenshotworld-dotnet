using System.Globalization;
using System.Net.Http.Headers;
using WebsiteScreenshotWorld.Internal;

namespace WebsiteScreenshotWorld.InstantSnap;

/// <summary>
/// A captured file streamed from the API, with its metadata. Read <see cref="Content"/> once or call
/// <see cref="SaveToAsync(string, CancellationToken)"/>, then dispose it to release the connection.
/// </summary>
public sealed class CaptureFile : IAsyncDisposable, IDisposable
{
    private readonly HttpResponseMessage _response;

    private CaptureFile(HttpResponseMessage response, Stream content)
    {
        _response = response;
        Content = content;

        var headers = response.Headers;
        Id = Guid.TryParse(Header(headers, "X-InstantSnap-Id"), out var id) ? id : null;
        Status = Header(headers, "X-InstantSnap-Status") is { } status ? new CaptureStatus(status) : default(CaptureStatus?);
        Width = ParseInt(Header(headers, "X-InstantSnap-Width"));
        Height = ParseInt(Header(headers, "X-InstantSnap-Height"));
        SizeBytes = ParseLong(Header(headers, "X-InstantSnap-Size-Bytes"));
        DurationMs = ParseLong(Header(headers, "X-InstantSnap-Duration-Ms"));
        PageHttpStatus = ParseInt(Header(headers, "X-InstantSnap-Page-Http-Status"));
        SpecVersion = Header(headers, "X-InstantSnap-Spec-Version");
        CapturedAt = Iso8601.TryParse(Header(headers, "X-InstantSnap-Captured-At"), out var capturedAt) ? capturedAt : null;
        FinalUrl = Header(headers, "X-InstantSnap-Final-Url");
        Sha256 = Header(headers, "X-InstantSnap-Sha256");
        PartCount = ParseInt(Header(headers, "X-InstantSnap-Part-Count"));
        ContentType = response.Content.Headers.ContentType?.ToString();
        FileName = FileNameOf(response.Content.Headers.ContentDisposition);
    }

    /// <summary>The file's bytes, read straight from the network. It can be read once.</summary>
    public Stream Content { get; }

    /// <summary>The file name the API suggests, such as <c>3f6c2a1e-….png</c>.</summary>
    public string? FileName { get; }

    /// <summary>The file's MIME type, such as <c>image/png</c>.</summary>
    public string? ContentType { get; }

    /// <summary>The capture id.</summary>
    public Guid? Id { get; }

    /// <summary>The capture status, <see cref="CaptureStatus.Delivered"/> for a file answer.</summary>
    public CaptureStatus? Status { get; }

    /// <summary>The width of the file in pixels.</summary>
    public int? Width { get; }

    /// <summary>The height of the file in pixels.</summary>
    public int? Height { get; }

    /// <summary>The file size in bytes.</summary>
    public long? SizeBytes { get; }

    /// <summary>How long the capture took, in milliseconds.</summary>
    public long? DurationMs { get; }

    /// <summary>The HTTP status the website answered with.</summary>
    public int? PageHttpStatus { get; }

    /// <summary>The set of defaults the capture was taken under, such as <c>v1</c>.</summary>
    public string? SpecVersion { get; }

    /// <summary>When the page was captured, in UTC.</summary>
    public DateTimeOffset? CapturedAt { get; }

    /// <summary>Where the browser ended up. Absent when that URL is not plain ASCII.</summary>
    public string? FinalUrl { get; }

    /// <summary>The SHA-256 digest of the file as lowercase hex.</summary>
    public string? Sha256 { get; }

    /// <summary>The number of parts, only set when the answer has more than one.</summary>
    public int? PartCount { get; }

    /// <summary>Writes the file to <paramref name="path"/>, replacing any file already there.</summary>
    /// <param name="path">The file to write.</param>
    /// <param name="cancellationToken">Stops the copy.</param>
    /// <returns>A task that completes when the file is written.</returns>
    public Task SaveToAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("A file path is required.", nameof(path));
        }

        return FileTarget.WriteAsync(path, (stream, token) => SaveToAsync(stream, token), cancellationToken);
    }

    /// <summary>Copies the file into <paramref name="destination"/>.</summary>
    /// <param name="destination">A writable stream. It is left open.</param>
    /// <param name="cancellationToken">Stops the copy.</param>
    /// <returns>A task that completes when the file is copied.</returns>
    public Task SaveToAsync(Stream destination, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(destination, nameof(destination));
        return Content.CopyToAsync(destination, FileTarget.BufferSize, cancellationToken);
    }

    /// <summary>Releases the stream and the connection behind it.</summary>
    public void Dispose()
    {
        Content.Dispose();
        _response.Dispose();
    }

    /// <summary>Releases the stream and the connection behind it.</summary>
    /// <returns>A task that completes when both are released.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }

    internal static async Task<CaptureFile> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStreamCompatAsync(cancellationToken).ConfigureAwait(false);
        return new CaptureFile(response, content);
    }

    private static string? Header(HttpResponseHeaders headers, string name) =>
        headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;

    private static long? ParseLong(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;

    private static string? FileNameOf(ContentDispositionHeaderValue? disposition)
    {
        var name = disposition?.FileNameStar ?? disposition?.FileName;
        return name?.Trim('"');
    }
}
