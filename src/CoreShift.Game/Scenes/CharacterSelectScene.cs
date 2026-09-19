using CoreShift.Core.Progression;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class CharacterSelectScene : Scene
{
    private readonly List<CharacterDef> _characters = CharacterCatalog.Definitions();
    private int _selected;

    public void Enter(GameApp app)
    {
        _selected = 0;
        string current = app.Session.Progression.Data.SelectedCharacter;
        for (int i = 0; i < _characters.Count; i++)
        {
            if (_characters[i].Id == current) _selected = i;
        }
    }

    public void Update(GameApp app, float dt)
    {
        if (InputMapper.UpPressed()) _selected = (_selected - 1 + _characters.Count) % _characters.Count;
        if (InputMapper.DownPressed()) _selected = (_selected + 1) % _characters.Count;

        if (InputMapper.BackPressed())
        {
            app.ChangeScene(new MainMenuScene());
            return;
        }

        if (InputMapper.ConfirmPressed())
        {
            app.Session.Progression.Data.SelectedCharacter = _characters[_selected].Id;
            app.Session.Save();
            app.ChangeScene(new MainMenuScene());
        }
    }

    public void Draw(GameApp app)
    {
        app.Renderer.DrawBackground(app.Time);
        MenuUi.DrawTitle("SELECT CHARACTER", 70);
        MenuUi.DrawSubtitle("Affects starting stats, weapon, and ability", 150);

        int width = Raylib.GetScreenWidth();
        int startY = 230;
        string current = app.Session.Progression.Data.SelectedCharacter;

        for (int i = 0; i < _characters.Count; i++)
        {
            var character = _characters[i];
            bool selected = i == _selected;
            bool equipped = character.Id == current;
            string text = (selected ? "> " : "  ") + character.Name + (equipped ? "  [equipped]" : string.Empty);
            int textWidth = Raylib.MeasureText(text, 28);
            int x = width / 2 - 300;
            int y = startY + i * 70;
            Raylib.DrawText(text, x, y, 28, selected ? Palette.Text : Palette.TextDim);

            Color detail = selected ? Palette.TextDim : Palette.Alpha(Palette.TextDim, 120);
            Raylib.DrawText(character.Description, x + 24, y + 30, 18, detail);
        }

        MenuUi.DrawSubtitle("Enter to equip    Esc to go back", 560);
    }
}
