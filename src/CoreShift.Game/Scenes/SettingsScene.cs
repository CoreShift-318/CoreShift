using Raylib_cs;

namespace CoreShift.Game;

public sealed class SettingsScene : Scene
{
    private static readonly string[] Labels =
    {
        "Master Volume",
        "SFX Volume",
        "Music Volume",
        "Screen Shake",
        "Aim Assist",
        "Show FPS",
        "Back",
    };

    private readonly Scene? _returnTo;
    private int _selected;

    public SettingsScene(Scene? returnTo = null)
    {
        _returnTo = returnTo;
    }

    public void Enter(GameApp app) => _selected = 0;

    public void Update(GameApp app, float dt)
    {
        if (InputMapper.UpPressed()) _selected = (_selected - 1 + Labels.Length) % Labels.Length;
        if (InputMapper.DownPressed()) _selected = (_selected + 1) % Labels.Length;

        if (InputMapper.LeftPressed() || InputMapper.RightPressed())
        {
            int direction = InputMapper.RightPressed() ? 1 : -1;
            Adjust(app, direction);
        }

        if (InputMapper.ConfirmPressed())
        {
            if (_selected == 6)
            {
                app.ChangeScene(_returnTo ?? (Scene)new MainMenuScene());
                return;
            }
            Adjust(app, 1);
        }

        if (InputMapper.BackPressed())
        {
            app.ChangeScene(_returnTo ?? (Scene)new MainMenuScene());
        }
    }

    private void Adjust(GameApp app, int direction)
    {
        var data = app.Session.Progression.Data;
        switch (_selected)
        {
            case 0:
                data.MasterVolume = Math.Clamp(data.MasterVolume + direction * 0.1f, 0f, 1f);
                app.ApplyAudioSettings();
                break;
            case 1:
                data.SfxVolume = Math.Clamp(data.SfxVolume + direction * 0.1f, 0f, 1f);
                app.ApplyAudioSettings();
                break;
            case 2:
                data.MusicVolume = Math.Clamp(data.MusicVolume + direction * 0.1f, 0f, 1f);
                app.ApplyAudioSettings();
                break;
            case 3:
                data.ShakeEnabled = !data.ShakeEnabled;
                break;
            case 4:
                data.AimAssistEnabled = !data.AimAssistEnabled;
                break;
            case 5:
                data.ShowFps = !data.ShowFps;
                break;
        }
        app.Session.Save();
    }

    public void Draw(GameApp app)
    {
        app.Renderer.DrawBackground(app.Time);
        MenuUi.DrawTitle("SETTINGS", 80);
        MenuUi.DrawSubtitle("Left / Right to change    Enter to toggle    Esc to go back", 168);

        var data = app.Session.Progression.Data;
        var values = new[]
        {
            $"{data.MasterVolume * 100f:0}%",
            $"{data.SfxVolume * 100f:0}%",
            $"{data.MusicVolume * 100f:0}%",
            data.ShakeEnabled ? "On" : "Off",
            data.AimAssistEnabled ? "On" : "Off",
            data.ShowFps ? "On" : "Off",
            string.Empty,
        };

        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        const int startY = 226;
        const int spacing = 48;
        int labelX = width / 2 - 330;
        int valueRight = width / 2 + 330;

        for (int i = 0; i < Labels.Length; i++)
        {
            bool selected = i == _selected;
            string text = (selected ? "> " : "  ") + Labels[i];
            int y = startY + i * spacing;
            Color color = selected ? Palette.Text : Palette.TextDim;
            Raylib.DrawText(text, labelX, y, 28, color);

            if (values[i].Length > 0)
            {
                Color valueColor = selected ? Palette.Player : Palette.TextDim;
                int valueWidth = Raylib.MeasureText(values[i], 28);
                Raylib.DrawText(values[i], valueRight - valueWidth, y, 28, valueColor);
            }
        }

        string version = "v" + GameApp.Version;
        int versionWidth = Raylib.MeasureText(version, 18);
        Raylib.DrawText(version, width - versionWidth - 18, height - 30, 18, Palette.TextDim);
    }
}
