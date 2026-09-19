namespace CoreShift.Data.Content;

public sealed class WaveDefinition
{
    public int Wave { get; set; } = 1;
    public string EnemyId { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
    public float SpawnInterval { get; set; } = 1.2f;
    public float HealthMultiplier { get; set; } = 1f;
    public float SpeedMultiplier { get; set; } = 1f;
}
