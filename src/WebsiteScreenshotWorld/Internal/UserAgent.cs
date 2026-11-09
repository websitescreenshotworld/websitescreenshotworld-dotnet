using System.Reflection;
using System.Runtime.InteropServices;

namespace WebsiteScreenshotWorld.Internal;

/// <summary>The User-Agent sent with every request.</summary>
internal static class UserAgent
{
    public static string Value { get; } = Build();

    private static string Build()
    {
        var version = typeof(UserAgent).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        // The SDK appends "+<commit>" to the informational version.
        var plus = version.IndexOf('+');
        if (plus >= 0)
        {
            version = version.Substring(0, plus);
        }

        return $"websitescreenshotworld-dotnet/{version} ({RuntimeInformation.FrameworkDescription.Trim()})";
    }
}
