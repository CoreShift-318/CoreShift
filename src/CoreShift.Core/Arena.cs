using CoreShift.Core.Math;

namespace CoreShift.Core;

public readonly struct Arena
{
    public readonly float Width;
    public readonly float Height;

    public Arena(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public Vec2 Center => Vec2.Zero;

    public Vec2 Clamp(Vec2 position)
    {
        float halfW = Width * 0.5f;
        float halfH = Height * 0.5f;
        float x = System.Math.Clamp(position.X, -halfW, halfW);
        float y = System.Math.Clamp(position.Y, -halfH, halfH);
        return new Vec2(x, y);
    }
}
