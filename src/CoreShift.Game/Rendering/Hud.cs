using CoreShift.Core;
using CoreShift.Core.Progression;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class Hud
{
    // Cached strings: rebuilt only when the underlying value changes (avoids per-frame garbage).
    private int _level = int.MinValue;
    private string _levelText = "LV 1";
    private int _xp = int.MinValue;
    private float _xpNext = float.MinValue;
    private string _xpText = string.Empty;
    private float _health = float.MinValue;
    private float _maxHealth = float.MinValue;
    private string _healthText = string.Empty;
    private int _wave = int.MinValue;
    private int _kills = int.MinValue;
    private string _statsText = string.Empty;
    private int _credits = int.MinValue;
    private string _creditsText = string.Empty;

    public void Draw(World world, ProgressionState progression, float time)
    {
        var snapshot = world.Snapshot();
        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();

        DrawXpBar(snapshot, width);
        DrawHealthBar(snapshot, height);
        DrawStats(snapshot, progression, width);
        DrawAbility(world, width, height);
        DrawHints(height);
    }

    private void DrawXpBar(GameSnapshot snapshot, int width)
    {
        Raylib.DrawRectangle(0, 0, width, 12, Palette.Alpha(Palette.Background, 220));
        Raylib.DrawRectangle(0, 0, (int)(width * snapshot.XpFraction), 12, Palette.Xp);
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawRectangle(0, 0, (int)(width * snapshot.XpFraction), 12, Palette.Alpha(Palette.Xp, 60));
        Raylib.EndBlendMode();

        if (snapshot.Level != _level)
        {
            _level = snapshot.Level;
            _levelText = $"LV {_level}";
        }
        if (snapshot.Xp != _xp || snapshot.XpToNext != _xpNext)
        {
            _xp = snapshot.Xp;
            _xpNext = snapshot.XpToNext;
            _xpText = $"XP {_xp}/{_xpNext:0}";
        }

        Raylib.DrawText(_levelText, 14, 20, 26, Palette.Text);
        Raylib.DrawText(_xpText, 14, 50, 18, Palette.TextDim);
    }

    private void DrawHealthBar(GameSnapshot snapshot, int height)
    {
        const int barWidth = 300;
        const int barHeight = 24;
        int x = 24;
        int y = height - 54;

        Raylib.DrawRectangle(x - 2, y - 2, barWidth + 4, barHeight + 4, Palette.Alpha(Palette.Background, 220));
        Raylib.DrawRectangle(x, y, barWidth, barHeight, Palette.Alpha(Palette.Grid, 120));

        Color healthColor = snapshot.HealthFraction > 0.3f ? Palette.Health : Palette.Danger;
        Raylib.DrawRectangle(x, y, (int)(barWidth * snapshot.HealthFraction), barHeight, healthColor);

        if (snapshot.Health != _health || snapshot.MaxHealth != _maxHealth)
        {
            _health = snapshot.Health;
            _maxHealth = snapshot.MaxHealth;
            _healthText = $"HP {_health:0}/{_maxHealth:0}";
        }
        Raylib.DrawText(_healthText, x + 8, y + 4, 16, Palette.Text);
    }

    private void DrawStats(GameSnapshot snapshot, ProgressionState progression, int width)
    {
        if (snapshot.Wave != _wave || snapshot.Kills != _kills)
        {
            _wave = snapshot.Wave;
            _kills = snapshot.Kills;
            _statsText = $"WAVE {_wave}    KILLS {_kills}";
        }
        int infoWidth = Raylib.MeasureText(_statsText, 22);
        Raylib.DrawText(_statsText, width - infoWidth - 18, 22, 22, Palette.Text);

        if (progression.Data.Currency != _credits)
        {
            _credits = progression.Data.Currency;
            _creditsText = $"CREDITS {_credits}";
        }
        int creditsWidth = Raylib.MeasureText(_creditsText, 18);
        Raylib.DrawText(_creditsText, width - creditsWidth - 18, 50, 18, Palette.Gold);
    }

    private static void DrawAbility(World world, int width, int height)
    {
        string label = world.Stats.Ability == AbilityKind.Dash ? "SHIFT DASH" : "SHIFT CHRONO";
        bool ready = world.AbilityCooldownRemaining <= 0f;
        Color color = ready ? Palette.Player : Palette.TextDim;

        int x = width - 240;
        int y = height - 58;
        Raylib.DrawRectangle(x - 10, y - 6, 220, 34, Palette.Alpha(Palette.Background, 200));
        Raylib.DrawText(label, x, y, 20, color);

        float fraction = world.Stats.AbilityCooldown <= 0f
            ? 1f
            : 1f - Math.Clamp(world.AbilityCooldownRemaining / world.Stats.AbilityCooldown, 0f, 1f);
        Raylib.DrawRectangle(x, y + 24, (int)(200 * fraction), 4, ready ? Palette.Health : Palette.Accent);
    }

    private static void DrawHints(int height)
    {
        Raylib.DrawText("WASD move   mouse/arrows aim   click/SPACE fire   SHIFT ability   ESC pause   F12 shot", 24, height - 24, 16, Palette.TextDim);
    }
}
