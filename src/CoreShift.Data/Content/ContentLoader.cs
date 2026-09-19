using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreShift.Data.Content;

public static class ContentLoader
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static GameContent Load(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) throw new ContentLoadException("Content directory was null or empty.");
        if (!Directory.Exists(directory)) throw new ContentLoadException("Content directory not found: '" + directory + "'.");

        try
        {
            string enemies = File.ReadAllText(Path.Combine(directory, "enemies.json"));
            string upgrades = File.ReadAllText(Path.Combine(directory, "upgrades.json"));
            string waves = File.ReadAllText(Path.Combine(directory, "waves.json"));
            return LoadFromJson(enemies, upgrades, waves);
        }
        catch (ContentLoadException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContentLoadException("Failed to read content from '" + directory + "': " + ex.Message, ex);
        }
    }

    public static GameContent LoadFromJson(string enemiesJson, string upgradesJson, string wavesJson)
    {
        var content = new GameContent
        {
            Enemies = Deserialize<List<EnemyDefinition>>(enemiesJson, "enemies"),
            Upgrades = Deserialize<List<UpgradeDefinition>>(upgradesJson, "upgrades"),
            Waves = Deserialize<List<WaveDefinition>>(wavesJson, "waves"),
        };
        content.Validate();
        return content;
    }

    public static string Serialize(GameContent content)
    {
        return JsonSerializer.Serialize(content, Options);
    }

    private static T Deserialize<T>(string json, string label)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options)
                   ?? throw new ContentLoadException("Content file '" + label + "' deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new ContentLoadException("Malformed JSON in " + label + " content: " + ex.Message, ex);
        }
    }
}
