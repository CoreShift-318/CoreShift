using CoreShift.Core.Upgrades;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class LevelUpScene : Scene
{
    private readonly PlayScene _play;
    private List<UpgradeDef> _offers = new();
    private int _selected;

    public LevelUpScene(PlayScene play)
    {
        _play = play;
    }

    public void Enter(GameApp app)
    {
        _selected = 0;
        _offers = new List<UpgradeDef>(app.UpgradeSystem!.CurrentOffers);
    }

    public void Update(GameApp app, float dt)
    {
        if (_offers.Count == 0)
        {
            app.ChangeScene(_play);
            return;
        }

        if (InputMapper.UpPressed()) _selected = (_selected - 1 + _offers.Count) % _offers.Count;
        if (InputMapper.DownPressed() || Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right))
            _selected = (_selected + 1) % _offers.Count;
        if (Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left))
            _selected = (_selected - 1 + _offers.Count) % _offers.Count;

        int number = InputMapper.NumberPressed();
        if (number >= 0 && number < _offers.Count)
        {
            Choose(app, number);
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.R))
        {
            app.UpgradeSystem!.Reroll(app.World!);
            _offers = new List<UpgradeDef>(app.UpgradeSystem.CurrentOffers);
            _selected = 0;
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.B))
        {
            app.UpgradeSystem!.Banish(app.World!, _selected);
            _offers = new List<UpgradeDef>(app.UpgradeSystem.CurrentOffers);
            _selected = 0;
            return;
        }

        if (InputMapper.ConfirmPressed()) Choose(app, _selected);
    }

    public void Draw(GameApp app)
    {
        _play.Draw(app);

        Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Palette.Alpha(Palette.Background, 205));
        MenuUi.DrawTitle("LEVEL UP", 60);
        MenuUi.DrawSubtitle("Choose an upgrade  (1-3 or arrows + Enter)    R reroll    B banish", 145);
        MenuUi.DrawUpgradeCards(_offers, _selected);
    }

    private void Choose(GameApp app, int index)
    {
        app.UpgradeSystem!.Choose(app.World!, index);
        app.ChangeScene(_play);
    }
}
