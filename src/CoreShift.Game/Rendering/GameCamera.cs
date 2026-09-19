using System.Numerics;
using CoreShift.Core.Math;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class GameCamera
{
    public GameCamera(float screenWidth, float screenHeight, float pixelsPerUnit)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        PixelsPerUnit = pixelsPerUnit;
    }

    public float ScreenWidth { get; }
    public float ScreenHeight { get; }
    public float PixelsPerUnit { get; }

    public Vector2 Offset;
    public Vec2 Center;

    public Vector2 ToScreen(Vec2 world) => new(
        ScreenWidth * 0.5f + (world.X - Center.X) * PixelsPerUnit + Offset.X,
        ScreenHeight * 0.5f - (world.Y - Center.Y) * PixelsPerUnit + Offset.Y);

    public Vec2 ToWorld(Vector2 screen) => new(
        (screen.X - Offset.X - ScreenWidth * 0.5f) / PixelsPerUnit + Center.X,
        Center.Y - (screen.Y - Offset.Y - ScreenHeight * 0.5f) / PixelsPerUnit);

    public Vec2 ClampToArena(Vec2 target, float arenaWidth, float arenaHeight)
    {
        float halfViewWidth = ScreenWidth * 0.5f / PixelsPerUnit;
        float halfViewHeight = ScreenHeight * 0.5f / PixelsPerUnit;
        float maxX = MathF.Max(0f, arenaWidth * 0.5f - halfViewWidth);
        float maxY = MathF.Max(0f, arenaHeight * 0.5f - halfViewHeight);
        return new Vec2(
            Math.Clamp(target.X, -maxX, maxX),
            Math.Clamp(target.Y, -maxY, maxY));
    }
}
