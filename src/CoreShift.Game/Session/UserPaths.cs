namespace CoreShift.Game;

/// <summary>
/// Resolves the per-user, writable data directory for saves and screenshots. Writing next to the
/// executable is not safe once installed (Program Files on Windows, inside a .app on macOS, or
/// read-only prefix dirs on Linux), so data goes to the platform's standard user location:
///   Windows: %APPDATA%\CoreShift
///   macOS:   ~/Library/Application Support/CoreShift
///   Linux:   $XDG_DATA_HOME/CoreShift or ~/.local/share/CoreShift
/// </summary>
public static class UserPaths
{
    public static string DataDirectory { get; } = ResolveDataDirectory();

    public static string SavesDirectory => Path.Combine(DataDirectory, "saves");

    public static string ScreenshotsDirectory => Path.Combine(DataDirectory, "screenshots");

    private static string ResolveDataDirectory()
    {
        string? root;
        if (OperatingSystem.IsWindows())
        {
            root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (OperatingSystem.IsMacOS())
        {
            root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support");
        }
        else
        {
            string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            root = !string.IsNullOrWhiteSpace(xdg)
                ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }

        if (string.IsNullOrWhiteSpace(root)) root = AppContext.BaseDirectory;
        return Path.Combine(root, "CoreShift");
    }
}
