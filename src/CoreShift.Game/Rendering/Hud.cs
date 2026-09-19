using CoreShift.Core;
using CoreShift.Core.Progression;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class Hud
{
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

    private static void DrawXpBar(GameSnapshot snapshot, int width)
    {
        Raylib.DrawRectangle(0, 0, width, 12, Palette.Alpha(Palette.Background, 220));
        Raylib.DrawRectangle(0, 0, (int)(width * snapshot.XpFraction), 12, Palette.Xp);
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawRectangle(0, 0, (int)(width * snapshot.XpFraction), 12, Palette.Alpha(Palette.Xp, 60));
        Raylib.EndBlendMode();

        Raylib.DrawText($"LV {snapshot.Level}", 14, 20, 26, Palette.Text);
        Raylib.DrawText($"XP {snapshot.Xp}/{snapshot.XpToNext:0}", 14, 50, 18, Palette.TextDim);
    }

    private static void DrawHealthBar(GameSnapshot snapshot, int height)
    {
        const int barWidth = 300;
        const int barHeight = 24;
        int x = 24;
        int y = height - 54;

        Raylib.DrawRectangle(x - 2, y - 2, barWidth + 4, barHeight + 4, Palette.Alpha(Palette.Background, 220));
        Raylib.DrawRectangle(x, y, barWidth, barHeight, Palette.Alpha(Palette.Grid, 120));

        Color healthColor = snapshot.HealthFraction > 0.3f ? Palette.Health : Palette.Danger;
        Raylib.DrawRectangle(x, y, (int)(barWidth * snapshot.HealthFraction), barHeight, healthColor);

        string label = $"HP {snapshot.Health:0}/{snapshot.MaxHealth:0}";
        Raylib.DrawText(label, x + 8, y + 4, 16, Palette.Text);
    }

    private static void DrawStats(GameSnapshot snapshot, ProgressionState progression, int width)
    {
        string info = $"WAVE {snapshot.Wave}    KILLS {snapshot.Kills}";
        int infoWidth = Raylib.MeasureText(info, 22);
        Raylib.DrawText(info, width - infoWidth - 18, 22, 22, Palette.Text);

        string credits = $"CREDITS {progression.Data.Currency}";
        int creditsWidth = Raylib.MeasureText(credits, 18);
        Raylib.DrawText(credits, width - creditsWidth - 18, 50, 18, Palette.Gold);
    }

    private static void DrawHints(int height)
    {
        Raylib.DrawText("WASD move   mouse/arrows aim   click/SPACE fire   SHIFT ability   ESC pause   F12 shot", 24, height - 24, 16, Palette.TextDim);
    }
}
