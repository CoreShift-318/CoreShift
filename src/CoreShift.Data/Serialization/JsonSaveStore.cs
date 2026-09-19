using System.Text.Json;
using CoreShift.Core.Progression;

namespace CoreShift.Data.Serialization;

public sealed class JsonSaveStore : ISaveStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public void Save(SaveData data, string path)
    {
        EnsureDirectory(path);
        File.WriteAllText(path, Serialize(data));
    }

    public SaveData Load(string path)
    {
        if (!File.Exists(path)) return new SaveData();

        try
        {
            return Deserialize(File.ReadAllText(path), path);
        }
        catch (SaveLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SaveLoadException("Failed to load JSON save '" + path + "': " + ex.Message, ex);
        }
    }

    public string Serialize(SaveData data) => JsonSerializer.Serialize(data, Options);

    public SaveData Deserialize(string text, string source = "<string>")
    {
        if (string.IsNullOrWhiteSpace(text)) throw new SaveLoadException("Save '" + source + "' was empty.");
        SaveData? data;
        try
        {
            data = JsonSerializer.Deserialize<SaveData>(text, Options);
        }
        catch (JsonException ex)
        {
            throw new SaveLoadException("Malformed JSON save '" + source + "': " + ex.Message, ex);
        }
        if (data is null) throw new SaveLoadException("Save '" + source + "' deserialized to null.");
        SaveValidation.CheckVersion(data, source);
        return data;
    }

    private static void EnsureDirectory(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
    }
}
