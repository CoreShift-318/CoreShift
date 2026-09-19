namespace CoreShift.Core.Systems;

public struct InputState
{
    public float MoveX;
    public float MoveY;
    public float AimX;
    public float AimY;
    public bool Firing;
    public bool Ability;

    public static readonly InputState None = default;

    public static InputState Move(float x, float y) => new() { MoveX = x, MoveY = y };

    public static InputState Aim(float x, float y) => new() { AimX = x, AimY = y };

    /// <summary>
    /// Pure key-to-axis mapping. Movement is WASD only; arrow keys are aim only. Side-effect free
    /// so the separation between moving and aiming is unit-testable.
    /// </summary>
    public static (float MoveX, float MoveY, float AimX, float AimY) ComposeAxes(
        bool up, bool down, bool left, bool right,
        bool w, bool a, bool s, bool d)
    {
        float moveX = 0f;
        float moveY = 0f;
        if (a) moveX -= 1f;
        if (d) moveX += 1f;
        if (s) moveY -= 1f;
        if (w) moveY += 1f;

        float aimX = 0f;
        float aimY = 0f;
        if (left) aimX -= 1f;
        if (right) aimX += 1f;
        if (down) aimY -= 1f;
        if (up) aimY += 1f;

        return (moveX, moveY, aimX, aimY);
    }

    /// <summary>
    /// Chooses the aim direction. A held arrow key always wins; otherwise the mouse direction is
    /// used once the mouse has actually moved; otherwise no aim is produced.
    /// </summary>
    public static (float AimX, float AimY) SelectAim(
        float keyAimX, float keyAimY, float mouseAimX, float mouseAimY, bool mouseActive)
    {
        if (keyAimX != 0f || keyAimY != 0f) return (keyAimX, keyAimY);
        if (mouseActive) return (mouseAimX, mouseAimY);
        return (0f, 0f);
    }
}
