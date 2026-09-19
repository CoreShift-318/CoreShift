namespace CoreShift.Core.Progression;

public sealed class ProgressionState
{
    public ProgressionState(SaveData? data = null)
    {
        Data = data ?? new SaveData();
    }

    public SaveData Data { get; private set; }

    public int LastRunCurrency { get; private set; }

    public void BeginRun() => LastRunCurrency = 0;

    public static int CurrencyFor(int waveReached, int kills) => waveReached * 10 + kills;

    public int CompleteRun(int waveReached, int kills, double seconds, int runCredits = 0)
    {
        int gain = CurrencyFor(waveReached, kills) + runCredits;
        LastRunCurrency = gain;

        Data.Currency += gain;
        Data.TotalKills += kills;
        Data.TotalPlaySeconds += seconds;
        if (waveReached > Data.BestWave) Data.BestWave = waveReached;

        Data.Runs.Add(new RunRecord { Wave = waveReached, Kills = kills, Seconds = seconds, Credits = gain });
        Data.Runs.Sort((a, b) => b.Wave != a.Wave ? b.Wave.CompareTo(a.Wave) : b.Kills.CompareTo(a.Kills));
        if (Data.Runs.Count > 8) Data.Runs.RemoveRange(8, Data.Runs.Count - 8);

        return gain;
    }

    public int GetPermanentRank(string id)
    {
        for (int i = 0; i < Data.PermanentUpgrades.Count; i++)
        {
            if (Data.PermanentUpgrades[i].Id == id) return Data.PermanentUpgrades[i].Rank;
        }
        return 0;
    }

    public void AddPermanentUpgrade(string id, int rank)
    {
        for (int i = 0; i < Data.PermanentUpgrades.Count; i++)
        {
            if (Data.PermanentUpgrades[i].Id == id)
            {
                Data.PermanentUpgrades[i].Rank += rank;
                return;
            }
        }
        Data.PermanentUpgrades.Add(new PermanentUpgrade { Id = id, Rank = rank });
    }

    public void ReplaceData(SaveData data) => Data = data;

    public int CostFor(PermanentUpgradeDef def)
    {
        int rank = GetPermanentRank(def.Id);
        float cost = def.BaseCost * MathF.Pow(def.CostGrowth, rank);
        return System.Math.Max(def.BaseCost, (int)MathF.Round(cost));
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || Data.Currency < amount) return false;
        Data.Currency -= amount;
        return true;
    }

    public bool TryPurchase(PermanentUpgradeDef def)
    {
        int rank = GetPermanentRank(def.Id);
        if (rank >= def.MaxRank) return false;
        if (!TrySpend(CostFor(def))) return false;
        AddPermanentUpgrade(def.Id, 1);
        return true;
    }

    public void ApplyTo(PlayerStats stats)
    {
        var definitions = PermanentUpgradeCatalog.Definitions();
        for (int i = 0; i < definitions.Count; i++)
        {
            var def = definitions[i];
            int rank = GetPermanentRank(def.Id);
            if (rank <= 0) continue;

            float bonus = def.MagnitudePerRank * rank;
            switch (def.Effect)
            {
                case PermanentUpgradeEffect.StartingDamage:
                    stats.Damage += bonus;
                    break;
                case PermanentUpgradeEffect.StartingHealth:
                    stats.MaxHealth += bonus;
                    break;
                case PermanentUpgradeEffect.StartingSpeed:
                    stats.MoveSpeed += bonus;
                    break;
            }
        }
    }
}
