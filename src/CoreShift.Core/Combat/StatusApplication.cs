namespace CoreShift.Core.Combat;

public enum StatusKind
{
    None,
    Burn,
    Poison,
    Slow,
    Shock,
    Stun,
}

public readonly struct StatusApplication
{
    public readonly StatusKind Kind;
    public readonly float Magnitude;
    public readonly float Duration;
    public readonly float Chance;

    public StatusApplication(StatusKind kind, float magnitude, float duration, float chance = 1f)
    {
        Kind = kind;
        Magnitude = magnitude;
        Duration = duration;
        Chance = chance;
    }

    public static readonly StatusApplication None = default;

    public bool IsNone => Kind == StatusKind.None;
}
