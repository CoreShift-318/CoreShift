using CoreShift.Core.Waves;

namespace CoreShift.Data;

public static class WaveTableFactory
{
    public static WaveTable FromContent(Content.GameContent content)
    {
        var byId = new Dictionary<string, EnemySpec>(StringComparer.Ordinal);
        for (int i = 0; i < content.Enemies.Count; i++)
        {
            var spec = content.Enemies[i].ToSpec();
            byId[spec.Id] = spec;
        }

        var primary = content.PrimaryEnemySpec();
        var explicitWaves = new Dictionary<int, Content.WaveDefinition>();
        for (int i = 0; i < content.Waves.Count; i++)
        {
            explicitWaves[content.Waves[i].Wave] = content.Waves[i];
        }

        var table = new WaveTable();
        for (int wave = 1; wave <= 2000; wave++)
        {
            var composition = ComposeFor(wave, byId, primary);
            var plan = new WavePlan
            {
                Composition = composition,
                Count = WaveScaling.EnemyCount(wave),
                SpawnInterval = WaveScaling.SpawnInterval(wave),
                HealthMultiplier = WaveScaling.HealthMultiplier(wave),
                SpeedMultiplier = WaveScaling.SpeedMultiplier(wave),
            };

            if (explicitWaves.TryGetValue(wave, out var def))
            {
                if (def.Count > 0) plan.Count = def.Count;
                if (def.SpawnInterval > 0f) plan.SpawnInterval = def.SpawnInterval;
                if (def.HealthMultiplier > 0f) plan.HealthMultiplier = def.HealthMultiplier;
                if (def.SpeedMultiplier > 0f) plan.SpeedMultiplier = def.SpeedMultiplier;
            }

            table.Plans.Add(plan);
        }

        return table;
    }

    private static WaveComposition ComposeFor(int wave, Dictionary<string, EnemySpec> byId, EnemySpec primary)
    {
        var composition = new WaveComposition();

        Add(composition, byId, "grunt", 6);
        if (wave >= 3) Add(composition, byId, "wisp", System.Math.Min(4, wave - 2));
        if (wave >= 6) Add(composition, byId, "brute", System.Math.Min(3, (wave - 5) / 2 + 1));

        if (composition.Entries.Count == 0)
        {
            composition.Entries.Add(new WaveEntry { Spec = primary, Weight = 1 });
        }

        return composition;
    }

    private static void Add(WaveComposition composition, Dictionary<string, EnemySpec> byId, string id, int weight)
    {
        if (weight <= 0) return;
        if (byId.TryGetValue(id, out var spec)) composition.Entries.Add(new WaveEntry { Spec = spec, Weight = weight });
    }
}
