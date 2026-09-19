using CoreShift.Core.Progression;
using CoreShift.Data.Serialization;

namespace CoreShift.Game;

public sealed class GameSession
{
    private readonly JsonSaveStore _store = new();

    public GameSession()
    {
        SavePath = Path.Combine(AppContext.BaseDirectory, "saves", "progress.json");
        Progression = new ProgressionState(Load());
    }

    public ProgressionState Progression { get; }
    public string SavePath { get; }

    public void Save()
    {
        try
        {
            _store.Save(Progression.Data, SavePath);
        }
        catch
        {
            // A failed save must never crash the game.
        }
    }

    private SaveData Load()
    {
        try
        {
            if (File.Exists(SavePath)) return _store.Load(SavePath);
        }
        catch
        {
            // Corrupt save: fall back to a fresh profile.
        }
        return new SaveData();
    }
}
