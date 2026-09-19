using CoreShift.Core.Progression;

namespace CoreShift.Data.Serialization;

public interface ISaveStore
{
    void Save(SaveData data, string path);
    SaveData Load(string path);
}
