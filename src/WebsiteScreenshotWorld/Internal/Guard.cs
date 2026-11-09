namespace WebsiteScreenshotWorld.Internal;

/// <summary>Argument checks that work on every target framework.</summary>
internal static class Guard
{
    public static T NotNull<T>(T? value, string name)
        where T : class =>
        value ?? throw new ArgumentNullException(name);
}
