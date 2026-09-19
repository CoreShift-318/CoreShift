using CoreShift.Core.Progression;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class HubScene : Scene
{
    private readonly List<PermanentUpgradeDef> _definitions = PermanentUpgradeCatalog.Definitions();
    private int _selected;

    public void Enter(GameApp app) => _selected = 0;

    public void Update(GameApp app, float dt)
    {
        if (_definitions.Count == 0) return;

        if (InputMapper.UpPressed()) _selected = (_selected - 1 + _definitions.Count) % _definitions.Count;
        if (InputMapper.DownPressed()) _selected = (_selected + 1) % _definitions.Count;

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            app.ChangeScene(new MainMenuScene());
            return;
        }

        if (!InputMapper.ConfirmPressed()) return;

        var progression = app.Session.Progression;
        if (progression.TryPurchase(_definitions[_selected]))
        {
            app.Session.Save();
        }
    }

    public void Draw(GameApp app)
    {
        app.Renderer.DrawBackground(app.Time);

        var progression = app.Session.Progression;
        MenuUi.DrawTitle("PROGRESSION HUB", 70);
        MenuUi.DrawSubtitle($"Credits {progression.Data.Currency}", 150);

        int width = Raylib.GetScreenWidth();
        int startY = 220;

        for (int i = 0; i < _definitions.Count; i++)
        {
            var def = _definitions[i];
            int rank = progression.GetPermanentRank(def.Id);
            int cost = progression.CostFor(def);
            bool maxed = rank >= def.MaxRank;
            bool selected = i == _selected;
            bool affordable = !maxed && progression.Data.Currency >= cost;
            bool canBuy = affordable;

            string costLabel = maxed ? "MAX" : $"{cost}";
            string text = $"{def.Name}   [{rank}/{def.MaxRank}]   {costLabel}";
            string prefix = selected ? "> " : "  ";
            Color color = selected ? Palette.Text : Palette.TextDim;
            if (selected && !canBuy) color = Palette.Danger;

            int textWidth = Raylib.MeasureText(prefix + text, 26);
            int x = (width - textWidth) / 2;
            int y = startY + i * 70;

            Raylib.DrawText(prefix + text, x, y, 26, color);
            Color detailColor = selected ? Palette.TextDim : Palette.Alpha(Palette.TextDim, 120);
            int detailWidth = Raylib.MeasureText(def.Description, 18);
            Raylib.DrawText(def.Description, (width - detailWidth) / 2, y + 30, 18, detailColor);
        }

        MenuUi.DrawSubtitle("Enter to buy    Esc to return", 560);
    }
}
