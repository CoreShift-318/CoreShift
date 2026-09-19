namespace CoreShift.Core.Progression;

public sealed class PermanentUpgrade
{
    public string Id { get; set; } = string.Empty;
    public int Rank { get; set; }
}

public sealed class RunRecord
{
    public int Wave { get; set; }
    public int Kills { get; set; }
    public double Seconds { get; set; }
    public int Credits { get; set; }
}

public sealed class SaveData
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public int Currency { get; set; }
    public List<string> UnlockedUpgrades { get; set; } = new();
    public int BestWave { get; set; }
    public int TotalKills { get; set; }
    public double TotalPlaySeconds { get; set; }
    public List<PermanentUpgrade> PermanentUpgrades { get; set; } = new();
    public List<RunRecord> Runs { get; set; } = new();
    public string SelectedCharacter { get; set; } = "scout";

    public float MasterVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 0.8f;
    public float MusicVolume { get; set; } = 0.5f;
    public bool ShakeEnabled { get; set; } = true;
    public bool ShowFps { get; set; }
    public bool AimAssistEnabled { get; set; } = true;

    public static SaveData CreateDefault() => new();
}
