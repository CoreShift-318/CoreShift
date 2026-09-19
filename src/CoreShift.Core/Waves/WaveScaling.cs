namespace CoreShift.Core.Waves;

public static class WaveScaling
{
    public static float HealthMultiplier(int wave) => 1f + 0.15f * (wave - 1);

    public static float SpeedMultiplier(int wave) => 1f + 0.03f * (wave - 1);

    public static int EnemyCount(int wave)
    {
        int count = 5 + 2 * (wave - 1);
        return System.Math.Clamp(count, 5, 200);
    }

    public static float SpawnInterval(int wave) => MathF.Max(0.25f, 1.2f - 0.05f * (wave - 1));
}
