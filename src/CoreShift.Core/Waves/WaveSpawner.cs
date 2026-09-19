using CoreShift.Core.Math;

namespace CoreShift.Core.Waves;

public sealed class WaveSpawner
{
    private readonly WaveTable _table;
    private float _timer;
    private int _spawned;

    public WaveSpawner(WaveTable? table = null)
    {
        _table = table ?? WaveTable.Default;
    }

    public int Wave { get; private set; } = 1;

    public void Update(World world, float dt)
    {
        _timer -= dt;
        if (_timer > 0f) return;

        var plan = _table.PlanFor(Wave);
        var spec = plan.Composition.Pick(world.Rng);
        EnemyFactory.Spawn(world, spec, RandomEdgePosition(world), Wave, plan.HealthMultiplier, plan.SpeedMultiplier);

        _spawned++;
        _timer = plan.SpawnInterval;

        if (_spawned >= plan.Count)
        {
            Wave++;
            _spawned = 0;
        }
    }

    private static Vec2 RandomEdgePosition(World world)
    {
        float halfWidth = world.Arena.Width * 0.5f;
        float halfHeight = world.Arena.Height * 0.5f;
        int side = world.Rng.NextInt(0, 4);

        return side switch
        {
            0 => new Vec2(world.Rng.Range(-halfWidth, halfWidth), halfHeight),
            1 => new Vec2(world.Rng.Range(-halfWidth, halfWidth), -halfHeight),
            2 => new Vec2(-halfWidth, world.Rng.Range(-halfHeight, halfHeight)),
            _ => new Vec2(halfWidth, world.Rng.Range(-halfHeight, halfHeight)),
        };
    }
}
