using System.Xml.Serialization;
using CoreShift.Core.Progression;

namespace CoreShift.Data.Serialization;

public sealed class XmlSaveStore : ISaveStore
{
    private static readonly XmlSerializer Serializer = new(typeof(SaveData));

    public void Save(SaveData data, string path)
    {
        EnsureDirectory(path);
        using var stream = File.Create(path);
        Serializer.Serialize(stream, data);
    }

    public SaveData Load(string path)
    {
        if (!File.Exists(path)) return new SaveData();

        try
        {
            using var stream = File.OpenRead(path);
            var data = Serializer.Deserialize(stream) as SaveData
                       ?? throw new SaveLoadException("XML save '" + path + "' deserialized to null.");
            SaveValidation.CheckVersion(data, path);
            return data;
        }
        catch (SaveLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SaveLoadException("Failed to load XML save '" + path + "': " + ex.Message, ex);
        }
    }

    private static void EnsureDirectory(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
    }
}
