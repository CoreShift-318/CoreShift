using System.Runtime.InteropServices;

namespace CoreShift.Game;

/// <summary>
/// Writes crash/startup-failure details to the user data directory so problems on Windows/macOS
/// can be diagnosed without a debugger attached.
/// </summary>
public static class CrashLog
{
    public static string Directory { get; } = Path.Combine(UserPaths.DataDirectory, "logs");

    public static void Write(string context, Exception exception)
    {
        try
        {
            System.IO.Directory.CreateDirectory(Directory);
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            string path = Path.Combine(Directory, $"crash-{stamp}.log");
            File.WriteAllText(path, Build(context, exception));
            Console.Error.WriteLine($"[CoreShift] {context}: {exception.Message}");
            Console.Error.WriteLine($"[CoreShift] crash log: {path}");
        }
        catch
        {
            // Logging must never throw.
        }
    }

    private static string Build(string context, Exception exception) =>
        "CoreShift crash log\n" +
        $"Time (UTC): {DateTime.UtcNow:o}\n" +
        $"Context:    {context}\n" +
        $"OS:         {RuntimeInformation.OSDescription}\n" +
        $"Arch:       {RuntimeInformation.ProcessArchitecture}\n" +
        $"Framework:  {RuntimeInformation.FrameworkDescription}\n" +
        $"\n{exception}\n";
}
