namespace WebsiteScreenshotWorld.Internal;

/// <summary>
/// Writes a download to a file through a temporary file beside it, so a failed download leaves any existing file
/// untouched and no partial file behind.
/// </summary>
internal static class FileTarget
{
    public const int BufferSize = 81920;

    public static async Task WriteAsync(string path, Func<Stream, CancellationToken, Task> write, CancellationToken cancellationToken)
    {
        var temporary = $"{path}.{Guid.NewGuid():N}.partial";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, useAsync: true))
            {
                await write(file, cancellationToken).ConfigureAwait(false);
                await file.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            Replace(temporary, path);
        }
        catch
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }

            throw;
        }
    }

    private static void Replace(string source, string destination)
    {
#if NET
        File.Move(source, destination, overwrite: true);
#else
        if (File.Exists(destination))
        {
            File.Delete(destination);
        }

        File.Move(source, destination);
#endif
    }
}
