using Raylib_cs;

namespace CoreShift.Game;

public sealed class MainMenuScene : Scene
{
    private static readonly string[] Items = { "Play", "Character", "Progression Hub", "Settings", "Quit" };

    private int _selected;

    public void Enter(GameApp app) => _selected = 0;

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
                app.ChangeScene(new CharacterSelectScene());
                break;
            case 2:
                app.ChangeScene(new HubScene());
                break;
            case 3:
                app.ChangeScene(new SettingsScene());
                break;
            case 4:
                app.RequestQuit();
                break;
        }
    }

    public void Draw(GameApp app)
    {
        app.Renderer.DrawBackground(app.Time);

        MenuUi.DrawTitle("CORESHIFT", 90);
        MenuUi.DrawSubtitle("CHRONO-SURVIVAL", 182);

        int listTop = 268;
        MenuUi.DrawList(Items, _selected, listTop);

        var progression = app.Session.Progression;
        var character = CoreShift.Core.Progression.CharacterCatalog.Find(progression.Data.SelectedCharacter);
        string characterName = character?.Name ?? "Scout";
        MenuUi.DrawSubtitle($"Best wave {progression.Data.BestWave}    Credits {progression.Data.Currency}    Character {characterName}", 566);
        MenuUi.DrawSubtitle("W-S / arrows to select    Enter to confirm", 604);

        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        string version = "v" + GameApp.Version;
        int versionWidth = Raylib.MeasureText(version, 18);
        Raylib.DrawText(version, width - versionWidth - 18, height - 30, 18, Palette.TextDim);
    }
}
