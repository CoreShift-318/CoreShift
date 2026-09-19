using Raylib_cs;

namespace CoreShift.Game;

public sealed class PauseScene : Scene
{
    private static readonly string[] Items = { "Resume", "Settings", "Abandon Run", "Quit to Menu" };

    private readonly PlayScene _play;
    private int _selected;

    public PauseScene(PlayScene play)
    {
        _play = play;
    }

    public void Enter(GameApp app) => _selected = 0;

    public void Update(GameApp app, float dt)
    {
        if (InputMapper.UpPressed()) _selected = (_selected - 1 + Items.Length) % Items.Length;
        if (InputMapper.DownPressed()) _selected = (_selected + 1) % Items.Length;

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            app.ChangeScene(_play);
            return;
        }

        if (!InputMapper.ConfirmPressed()) return;

        switch (_selected)
        {
            case 0:
                app.ChangeScene(_play);
                break;
            case 1:
                app.ChangeScene(new SettingsScene(this));
                break;
            case 2:
                app.World!.IsGameOver = true;
                app.ChangeScene(new GameOverScene());
                break;
            case 3:
                app.ChangeScene(new MainMenuScene());
                break;
        }
    }

    public void Draw(GameApp app)
    {
        _play.Draw(app);
        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Palette.Alpha(Palette.Background, 205));
        MenuUi.DrawTitle("PAUSED", 120);
        MenuUi.DrawList(Items, _selected, 280);
    }
}
