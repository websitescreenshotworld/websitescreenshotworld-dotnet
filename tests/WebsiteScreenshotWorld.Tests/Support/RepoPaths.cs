namespace WebsiteScreenshotWorld.Tests.Support;

/// <summary>Finds the shared spec and conformance server in the spec submodule at the repository root.</summary>
internal static class RepoPaths
{
    private static readonly Lazy<string> DotnetFolder = new(FindDotnetFolder);

    public static string ConformanceServer => Path.GetFullPath(Path.Combine(DotnetFolder.Value, "spec", "conformance", "server.js"));

    public static string Spec(params string[] parts) =>
        Path.GetFullPath(Path.Combine([DotnetFolder.Value, "spec", .. parts]));

    private static string FindDotnetFolder()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WebsiteScreenshotWorld.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find the repository root above " + AppContext.BaseDirectory);
    }
}
