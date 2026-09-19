using CoreShift.Data;
using CoreShift.App;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class GameRunnerSmokeTests
{
    [Fact]
    public void AutoRun_CompletesWithoutException()
    {
        using var writer = new StringWriter();
        var progression = new ProgressionState();

        var snapshot = GameRunner.Run(
            new AutoPilotInputSource(),
            writer,
            maxTicks: 1800,
            seed: 42,
            progression: progression);

        Assert.Equal(100f, snapshot.MaxHealth);
        Assert.True(snapshot.Health >= 0f);
    }

    [Fact]
    public void ScriptedInput_ReturnsNoneAfterExhaustion()
    {
        var source = new ScriptedInputSource(new[]
        {
            InputState.Move(1f, 0f),
        });

        Assert.Equal(1f, source.Poll().MoveX);

        var exhausted = source.Poll();
        Assert.Equal(0f, exhausted.MoveX);
        Assert.Equal(0f, exhausted.MoveY);
    }

    [Fact]
    public void Renderer_ProducesGridOfRequestedSize()
    {
        var world = GameFactory.Build(1, Data.Content.GameContent.Default());
        string rendered = AsciiRenderer.Render(world, 20, 10);
        string[] lines = rendered.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, lines.Length);
        Assert.Equal(20, lines[0].Length);
        Assert.Contains('@', rendered);
    }
}
