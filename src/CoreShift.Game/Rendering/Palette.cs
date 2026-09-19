using Raylib_cs;

namespace CoreShift.Game;

public static class Palette
{
    public static readonly Color Background = new(10, 6, 24, 255);
    public static readonly Color BackgroundTop = new(38, 14, 70, 255);
    public static readonly Color Grid = new(70, 36, 130, 255);
    public static readonly Color Player = new(80, 240, 255, 255);
    public static readonly Color PlayerCore = new(235, 255, 255, 255);
    public static readonly Color Projectile = new(120, 255, 240, 255);
    public static readonly Color Text = new(222, 232, 255, 255);
    public static readonly Color TextDim = new(140, 150, 190, 255);
    public static readonly Color Accent = new(255, 90, 205, 255);
    public static readonly Color Danger = new(255, 70, 95, 255);
    public static readonly Color Xp = new(120, 200, 255, 255);
    public static readonly Color Health = new(80, 240, 160, 255);
    public static readonly Color Gold = new(255, 210, 90, 255);

    public static Color Alpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);

    public static Color FromArgb(int argb)
    {
        var a = (byte)((argb >> 24) & 0xFF);
        var r = (byte)((argb >> 16) & 0xFF);
        var g = (byte)((argb >> 8) & 0xFF);
        var b = (byte)(argb & 0xFF);
        if (a == 0) a = 255;
        return new Color(r, g, b, a);
    }

    public static Color RarityColor(int rarity) => rarity switch
    {
        >= 3 => Accent,
        2 => Gold,
        _ => Player,
    };

    public static Color Lerp(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t));
    }
}
