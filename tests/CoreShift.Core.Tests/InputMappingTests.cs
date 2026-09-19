using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class InputMappingTests
{
    [Fact]
    public void ArrowKeys_NeverMove_OnlyAim()
    {
        foreach (var arrow in new[] { "up", "down", "left", "right" })
        {
            var (moveX, moveY, aimX, aimY) = arrow switch
            {
                "up" => InputState.ComposeAxes(up: true, down: false, left: false, right: false, w: false, a: false, s: false, d: false),
                "down" => InputState.ComposeAxes(up: false, down: true, left: false, right: false, w: false, a: false, s: false, d: false),
                "left" => InputState.ComposeAxes(up: false, down: false, left: true, right: false, w: false, a: false, s: false, d: false),
                _ => InputState.ComposeAxes(up: false, down: false, left: false, right: true, w: false, a: false, s: false, d: false),
            };

            Assert.Equal(0f, moveX);
            Assert.Equal(0f, moveY);
            Assert.NotEqual(0f, aimX + aimY);
        }
    }

    [Fact]
    public void WasdKeys_MoveOnly_NeverAim()
    {
        var (moveX, moveY, aimX, aimY) = InputState.ComposeAxes(
            up: false, down: false, left: false, right: false,
            w: true, a: true, s: false, d: false);

        Assert.Equal(-1f, moveX);
        Assert.Equal(1f, moveY);
        Assert.Equal(0f, aimX);
        Assert.Equal(0f, aimY);
    }

    [Fact]
    public void ArrowUp_AimsUp_NotMoveUp()
    {
        var (moveX, moveY, aimX, aimY) = InputState.ComposeAxes(
            up: true, down: false, left: false, right: false,
            w: false, a: false, s: false, d: false);

        Assert.Equal(0f, moveY);
        Assert.Equal(1f, aimY);
    }

    [Fact]
    public void Combined_MoveAndAimAreIndependent()
    {
        var (moveX, moveY, aimX, aimY) = InputState.ComposeAxes(
            up: false, down: false, left: true, right: false,
            w: false, a: false, s: false, d: true);

        Assert.Equal(1f, moveX);
        Assert.Equal(0f, moveY);
        Assert.Equal(-1f, aimX);
        Assert.Equal(0f, aimY);
    }

    [Fact]
    public void SelectAim_KeyAimWinsOverMouse()
    {
        var (aimX, aimY) = InputState.SelectAim(keyAimX: 0f, keyAimY: 1f, mouseAimX: 5f, mouseAimY: 5f, mouseActive: true);
        Assert.Equal(0f, aimX);
        Assert.Equal(1f, aimY);
    }

    [Fact]
    public void SelectAim_UsesMouseWhenNoKeyAim()
    {
        var (aimX, aimY) = InputState.SelectAim(keyAimX: 0f, keyAimY: 0f, mouseAimX: 2f, mouseAimY: -3f, mouseActive: true);
        Assert.Equal(2f, aimX);
        Assert.Equal(-3f, aimY);
    }

    [Fact]
    public void SelectAim_NoAimWhenMouseInactiveAndNoKey()
    {
        var (aimX, aimY) = InputState.SelectAim(keyAimX: 0f, keyAimY: 0f, mouseAimX: 2f, mouseAimY: -3f, mouseActive: false);
        Assert.Equal(0f, aimX);
        Assert.Equal(0f, aimY);
    }
}
