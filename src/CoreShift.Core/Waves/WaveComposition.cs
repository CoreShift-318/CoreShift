using CoreShift.Core.Rng;

namespace CoreShift.Core.Waves;

public sealed class WaveEntry
{
    public EnemySpec Spec { get; set; } = new();
    public int Weight { get; set; } = 1;
}

public sealed class WaveComposition
{
    public List<WaveEntry> Entries { get; set; } = new();

    public EnemySpec Pick(DeterministicRng rng)
    {
        if (Entries.Count == 0) return new EnemySpec();

        int total = 0;
        for (int i = 0; i < Entries.Count; i++) total += System.Math.Max(1, Entries[i].Weight);

        int roll = rng.NextInt(0, total);
        int accumulated = 0;
        for (int i = 0; i < Entries.Count; i++)
        {
            accumulated += System.Math.Max(1, Entries[i].Weight);
            if (roll < accumulated) return Entries[i].Spec;
        }
        return Entries[^1].Spec;
    }
}

public sealed class WavePlan
{
    public WaveComposition Composition { get; set; } = new();
    public int Count { get; set; } = 5;
    public float SpawnInterval { get; set; } = 1.2f;
    public float HealthMultiplier { get; set; } = 1f;
    public float SpeedMultiplier { get; set; } = 1f;
}

public sealed class WaveTable
{
    public List<WavePlan> Plans { get; set; } = new();

    public WavePlan PlanFor(int wave)
    {
        if (Plans.Count == 0) return new WavePlan();
        int index = System.Math.Clamp(wave - 1, 0, Plans.Count - 1);
        return Plans[index];
    }

    public static WaveTable Default
    {
        get
        {
            var grunt = new EnemySpec { Id = "grunt" };
            var plans = new List<WavePlan>();

            for (int wave = 1; wave <= 2000; wave++)
            {
                var composition = new WaveComposition();
                composition.Entries.Add(new WaveEntry { Spec = grunt, Weight = 1 });
                plans.Add(new WavePlan
                {
                    Composition = composition,
                    Count = WaveScaling.EnemyCount(wave),
                    SpawnInterval = WaveScaling.SpawnInterval(wave),
                    HealthMultiplier = WaveScaling.HealthMultiplier(wave),
                    SpeedMultiplier = WaveScaling.SpeedMultiplier(wave),
                });
            }

            return new WaveTable { Plans = plans };
        }
    }
}
