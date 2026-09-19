namespace CoreShift.Core.Systems;

public sealed class ProgressionSystem : ISystem
{
    private readonly Progression.ProgressionState _state;
    private bool _reported;

    public ProgressionSystem(Progression.ProgressionState state)
    {
        _state = state;
    }

    public int LastGain { get; private set; }

    public void Update(World world, float dt)
    {
        if (!world.IsGameOver || _reported) return;
        _reported = true;
        LastGain = _state.CompleteRun(world.Wave, world.Kills, world.ElapsedSeconds, world.RunCredits);
    }

    public void ResetReporting() => _reported = false;
}
