namespace CoreShift.Core.Systems;

public sealed class WaveSystem : ISystem
{
    public WaveSystem(Waves.WaveSpawner? spawner = null)
    {
        Spawner = spawner ?? new Waves.WaveSpawner();
    }

    public Waves.WaveSpawner Spawner { get; }

    public void Update(World world, float dt)
    {
        Spawner.Update(world, dt);
        world.Wave = Spawner.Wave;
    }
}
