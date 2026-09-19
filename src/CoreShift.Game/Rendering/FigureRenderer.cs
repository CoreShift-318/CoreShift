using System.Numerics;
using Raylib_cs;

namespace CoreShift.Game;

public enum FigureStyle
{
    Player,
    Grunt,
    Brute,
    Wisp,
}

/// <summary>
/// Draws top-down humanoid figures from primitives. The figure rotates to face a direction and
/// its legs animate with a walk cycle. No art assets are required.
/// </summary>
public static class FigureRenderer
{
    public static void Draw(Vector2 position, float angleDegrees, float walkPhase, Color color,
        FigureStyle style, bool moving, float scale)
    {
        if (scale <= 0.5f) scale = 0.5f;

        float radians = angleDegrees * (MathF.PI / 180f);
        var forward = new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        var perpendicular = new Vector2(-forward.Y, forward.X);

        float bob = style == FigureStyle.Wisp
            ? MathF.Sin(walkPhase * 3f) * scale * 0.22f
            : (moving ? MathF.Sin(walkPhase * 2f) * scale * 0.10f : MathF.Sin(walkPhase) * scale * 0.04f);
        position += new Vector2(0f, bob);

        GetMetrics(style, scale, out float torsoLen, out float torsoWid, out float headRadius,
            out float headOffset, out float shoulder, out float legLength, out float legSpread, out float gunLength);

        Color outline = Shade(color, 0.4f);
        Color bright = Brighten(color, 0.55f);

        if (legLength > 0f)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var hip = position + perpendicular * (legSpread * side) - forward * (torsoLen * 0.15f);
                float swing = moving ? MathF.Sin(walkPhase + (side > 0 ? 0f : MathF.PI)) * 0.5f : 0f;
                var foot = hip + Rotate(-forward, swing) * legLength;
                Raylib.DrawLineEx(hip, foot, scale * 0.30f, outline);
                Raylib.DrawCircleV(foot, scale * 0.16f, outline);
            }
        }
        else
        {
            // Floating wisps get an energy tail.
            var tail = position - forward * (torsoLen * 0.9f);
            Raylib.DrawCircleV(tail, scale * 0.22f, Alpha(bright, 140));
            Raylib.DrawCircleV(position - forward * (torsoLen * 0.5f), scale * 0.32f, Alpha(color, 120));
        }

        var origin = new Vector2(torsoLen * 0.5f, torsoWid * 0.5f);
        float pad = scale * 0.16f;
        float outlineW = torsoLen + pad * 2f;
        float outlineH = torsoWid + pad * 2f;
        Raylib.DrawRectanglePro(
            new Rectangle(position.X - outlineW * 0.5f, position.Y - outlineH * 0.5f, outlineW, outlineH),
            origin + new Vector2(pad, pad), angleDegrees, outline);
        Raylib.DrawRectanglePro(
            new Rectangle(position.X - torsoLen * 0.5f, position.Y - torsoWid * 0.5f, torsoLen, torsoWid),
            origin, angleDegrees, color);
        Raylib.DrawLineEx(
            position - forward * (torsoLen * 0.32f),
            position + forward * (torsoLen * 0.32f),
            scale * 0.16f, bright);

        var leftShoulder = position + perpendicular * -shoulder + forward * (torsoLen * 0.05f);
        var rightShoulder = position + perpendicular * shoulder + forward * (torsoLen * 0.05f);
        Raylib.DrawCircleV(leftShoulder, scale * 0.22f, outline);
        Raylib.DrawCircleV(leftShoulder, scale * 0.15f, bright);
        Raylib.DrawCircleV(rightShoulder, scale * 0.22f, outline);
        Raylib.DrawCircleV(rightShoulder, scale * 0.15f, bright);

        var gunStart = position + forward * (torsoLen * 0.35f);
        var gunTip = gunStart + forward * gunLength;
        var grip = gunStart + forward * (gunLength * 0.35f);
        Raylib.DrawLineEx(leftShoulder, grip, scale * 0.22f, color);
        Raylib.DrawLineEx(rightShoulder, grip, scale * 0.22f, color);
        Raylib.DrawLineEx(gunStart, gunTip, scale * 0.26f, bright);

        var head = position + forward * headOffset;
        Raylib.DrawCircleV(head, headRadius, bright);
        Raylib.DrawCircleLines((int)head.X, (int)head.Y, headRadius, outline);
    }

    private static void GetMetrics(FigureStyle style, float scale, out float torsoLen, out float torsoWid,
        out float headRadius, out float headOffset, out float shoulder, out float legLength, out float legSpread,
        out float gunLength)
    {
        switch (style)
        {
            case FigureStyle.Brute:
                torsoLen = 1.25f; torsoWid = 1.60f; headRadius = 0.40f; headOffset = 0.72f;
                shoulder = 0.80f; legLength = 0.70f; legSpread = 0.52f; gunLength = 0.55f;
                break;
            case FigureStyle.Wisp:
                torsoLen = 0.95f; torsoWid = 0.80f; headRadius = 0.40f; headOffset = 0.55f;
                shoulder = 0.42f; legLength = 0f; legSpread = 0f; gunLength = 0.35f;
                break;
            case FigureStyle.Player:
                torsoLen = 1.35f; torsoWid = 0.95f; headRadius = 0.34f; headOffset = 0.80f;
                shoulder = 0.50f; legLength = 0.90f; legSpread = 0.38f; gunLength = 1.50f;
                break;
            default:
                torsoLen = 1.25f; torsoWid = 0.95f; headRadius = 0.34f; headOffset = 0.72f;
                shoulder = 0.50f; legLength = 0.85f; legSpread = 0.36f; gunLength = 0.85f;
                break;
        }

        torsoLen *= scale; torsoWid *= scale; headRadius *= scale; headOffset *= scale;
        shoulder *= scale; legLength *= scale; legSpread *= scale; gunLength *= scale;
    }

    private static Vector2 Rotate(Vector2 value, float radians)
    {
        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);
        return new Vector2(value.X * c - value.Y * s, value.X * s + value.Y * c);
    }

    public static Color Shade(Color color, float factor) =>
        new((byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor), color.A);

    public static Color Brighten(Color color, float factor) => new(
        (byte)System.Math.Min(255, color.R + (255 - color.R) * factor),
        (byte)System.Math.Min(255, color.G + (255 - color.G) * factor),
        (byte)System.Math.Min(255, color.B + (255 - color.B) * factor),
        color.A);

    private static Color Alpha(Color color, byte alpha) => new(color.R, color.G, color.B, alpha);
}
