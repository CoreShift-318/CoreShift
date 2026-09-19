namespace CoreShift.Core.Progression;

public readonly struct GameSnapshot
{
    public float Health { get; init; }
    public float MaxHealth { get; init; }
    public int Level { get; init; }
    public int Xp { get; init; }
    public float XpToNext { get; init; }
    public int Wave { get; init; }
    public int EntityCount { get; init; }
    public int Kills { get; init; }
    public int RunCredits { get; init; }
    public bool IsGameOver { get; init; }

    public float HealthFraction => MaxHealth <= 0f ? 0f : Health / MaxHealth;
    public float XpFraction => XpToNext <= 0f ? 0f : Xp / XpToNext;

    public override string ToString() =>
        $"Wave {Wave} | HP {Health:0}/{MaxHealth:0} | Lv {Level} | XP {Xp}/{XpToNext:0} | Entities {EntityCount} | Kills {Kills}"
        + (IsGameOver ? " | GAME OVER" : string.Empty);
}
