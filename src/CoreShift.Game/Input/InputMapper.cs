using System.Numerics;
using CoreShift.Core;
using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;
using Raylib_cs;
using Vec2 = CoreShift.Core.Math.Vec2;

namespace CoreShift.Game;

public static class InputMapper
{
    private const int Gamepad = 0;
    private const float StickDeadzone = 0.2f;

    private static bool _mouseActive;
    private static bool _mouseInitialized;
    private static Vector2 _lastMouse;

    public static bool MouseAiming => _mouseActive;

    public static InputState ReadGameplay(GameCamera camera, World world)
    {
        var (moveX, moveY, keyAimX, keyAimY) = InputState.ComposeAxes(
            up: Raylib.IsKeyDown(KeyboardKey.Up),
            down: Raylib.IsKeyDown(KeyboardKey.Down),
            left: Raylib.IsKeyDown(KeyboardKey.Left),
            right: Raylib.IsKeyDown(KeyboardKey.Right),
            w: Raylib.IsKeyDown(KeyboardKey.W),
            a: Raylib.IsKeyDown(KeyboardKey.A),
            s: Raylib.IsKeyDown(KeyboardKey.S),
            d: Raylib.IsKeyDown(KeyboardKey.D));

        bool firing = Raylib.IsKeyDown(KeyboardKey.Space) || Raylib.IsMouseButtonDown(MouseButton.Left);
        bool ability = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);

        // Gamepad: left stick moves (analog), right stick aims, A/RT fires, LB uses the ability.
        float padAimX = 0f;
        float padAimY = 0f;
        bool padAiming = false;
        if (Raylib.IsGamepadAvailable(Gamepad))
        {
            float lx = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.LeftX);
            float ly = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.LeftY);
            if (MathF.Abs(lx) > StickDeadzone || MathF.Abs(ly) > StickDeadzone)
            {
                moveX = lx;
                moveY = -ly;
            }

            float rx = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.RightX);
            float ry = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.RightY);
            if (MathF.Abs(rx) > StickDeadzone || MathF.Abs(ry) > StickDeadzone)
            {
                padAimX = rx;
                padAimY = -ry;
                padAiming = true;
            }

            firing |= Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.RightFaceDown)
                      || Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.RightTrigger2);
            ability |= Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.LeftTrigger2);
        }

        // Mouse aim activates only after the cursor actually moves.
        var mouse = Raylib.GetMousePosition();
        if (!_mouseInitialized)
        {
            _lastMouse = mouse;
            _mouseInitialized = true;
        }
        else if (mouse != _lastMouse)
        {
            _mouseActive = true;
            _lastMouse = mouse;
        }

        float mouseAimX = 0f;
        float mouseAimY = 0f;
        if (world.HasPlayer && world.Entities.Has<Transform2>(world.Player))
        {
            var playerTransform = world.Entities.Get<Transform2>(world.Player);
            var direction = camera.ToWorld(mouse) - new Vec2(playerTransform.X, playerTransform.Y);
            if (direction.Length > 1e-3f)
            {
                mouseAimX = direction.X;
                mouseAimY = direction.Y;
            }
        }

        // Precedence: arrow keys, then gamepad stick, then mouse.
        bool pointerActive = padAiming || _mouseActive;
        float pointerAimX = padAiming ? padAimX : mouseAimX;
        float pointerAimY = padAiming ? padAimY : mouseAimY;
        var (aimX, aimY) = InputState.SelectAim(keyAimX, keyAimY, pointerAimX, pointerAimY, pointerActive);

        return new InputState
        {
            MoveX = moveX,
            MoveY = moveY,
            AimX = aimX,
            AimY = aimY,
            Firing = firing,
            Ability = ability,
        };
    }

    public static bool ConfirmPressed()
    {
        bool pressed = Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.Space);
        if (pressed) Audio.Play(Sfx.UiConfirm);
        return pressed;
    }

    public static bool UpPressed() => MenuMove(Raylib.IsKeyPressed(KeyboardKey.W) || Raylib.IsKeyPressed(KeyboardKey.Up));

    public static bool DownPressed() => MenuMove(Raylib.IsKeyPressed(KeyboardKey.S) || Raylib.IsKeyPressed(KeyboardKey.Down));

    public static bool LeftPressed() => MenuMove(Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Left));

    public static bool RightPressed() => MenuMove(Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.Right));

    public static bool BackPressed() => Raylib.IsKeyPressed(KeyboardKey.Escape);

    private static bool MenuMove(bool pressed)
    {
        if (pressed) Audio.Play(Sfx.UiMove);
        return pressed;
    }

    public static int NumberPressed()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.One)) return 0;
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) return 1;
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) return 2;
        return -1;
    }
}
