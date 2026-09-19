using CoreShift.Core.Progression;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class GameOverScene : Scene
{
    private static readonly string[] Items = { "Retry", "Progression Hub", "Main Menu" };

    private GameSnapshot _snapshot;
    private int _gain;
    private int _selected;
    private double _seconds;

    public void Enter(GameApp app)
    {
        _selected = 0;
        _snapshot = app.World!.Snapshot();
        _seconds = app.World.ElapsedSeconds;

        var progression = app.Session.Progression;
        if (progression.LastRunCurrency <= 0)
        {
            progression.CompleteRun(_snapshot.Wave, _snapshot.Kills, app.World.ElapsedSeconds);
        }

        _gain = progression.LastRunCurrency;
        app.Session.Save();
    }

    public void Update(GameApp app, float dt)
    {
        if (InputMapper.UpPressed()) _selected = (_selected - 1 + Items.Length) % Items.Length;
        if (InputMapper.DownPressed()) _selected = (_selected + 1) % Items.Length;

        if (!InputMapper.ConfirmPressed()) return;

        switch (_selected)
        {
            case 0:
                app.StartRun();
                break;
            case 1:
                app.ChangeScene(new HubScene());
                break;
            case 2:
                app.ChangeScene(new MainMenuScene());
                break;
        }
    }

    public void Draw(GameApp app)
    {
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Palette.Alpha(Palette.Background, 235));
        MenuUi.DrawTitle("GAME OVER", 90);
        MenuUi.DrawSubtitle($"Reached wave {_snapshot.Wave}   |   {_snapshot.Kills} kills   |   {_seconds:0}s   |   +{_gain} credits", 180);
        MenuUi.DrawList(Items, _selected, 300);
        DrawLeaderboard(app);
    }

    private static void DrawLeaderboard(GameApp app)
    {
        var runs = app.Session.Progression.Data.Runs;
        if (runs.Count == 0) return;

        int width = Raylib.GetScreenWidth();
        int y = 470;
        Raylib.DrawText("BEST RUNS", width / 2 - 150, y, 18, Palette.Gold);
        y += 26;

        int shown = System.Math.Min(3, runs.Count);
        for (int i = 0; i < shown; i++)
        {
            var run = runs[i];
            string line = $"{i + 1}.  wave {run.Wave}   {run.Kills} kills   {run.Seconds:0}s   +{run.Credits}";
            Raylib.DrawText(line, width / 2 - 150, y, 18, Palette.TextDim);
            y += 24;
        }
    }
}
