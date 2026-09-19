using CoreShift.Data.Content;
using CoreShift.Game;

string? screenshot = null;
string scene = "play";
int ticks = 1800;
uint seed = 12345;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--screenshot":
            if (i + 1 < args.Length) screenshot = args[++i];
            break;
        case "--scene":
            if (i + 1 < args.Length) scene = args[++i];
            break;
        case "--ticks":
            if (i + 1 < args.Length) ticks = int.Parse(args[++i]);
            break;
        case "--seed":
            if (i + 1 < args.Length) seed = uint.Parse(args[++i]);
            break;
        case "--help":
        case "-h":
            PrintHelp();
            return;
    }
}

var content = LoadContent();
var app = new GameApp(content) { Seed = seed };

if (screenshot is not null)
{
    app.RunScreenshot(screenshot, scene, ticks);
    System.Console.WriteLine($"screenshot saved: {System.IO.Path.GetFullPath(screenshot)}");
    return;
}

app.Run();

static GameContent LoadContent()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 8 && directory is not null; i++)
    {
        string candidate = Path.Combine(directory.FullName, "content");
        if (Directory.Exists(candidate))
        {
            try
            {
                return ContentLoader.Load(candidate);
            }
            catch (ContentLoadException ex)
            {
                System.Console.WriteLine("Warning: content load failed (" + ex.Message + "). Using defaults.");
                return GameContent.Default();
            }
        }
        directory = directory.Parent;
    }
    return GameContent.Default();
}

static void PrintHelp()
{
    System.Console.WriteLine("CoreShift: Chrono-Survival (graphical build)");
    System.Console.WriteLine("  dotnet run --project src/CoreShift.Game                       # play");
    System.Console.WriteLine("  dotnet run --project src/CoreShift.Game -- --screenshot shots/play.png --scene play --ticks 900");
    System.Console.WriteLine("Scenes: menu, hub, play, levelup, gameover");
}
