using CoreShift.Data.Content;

namespace CoreShift.Core.Tests;

public class ContentLoaderTests
{
    private const string ValidEnemies = """
    [ { "id": "grunt", "name": "Grunt", "maxHealth": 20, "speed": 2, "damage": 5, "radius": 0.5, "xpReward": 1 } ]
    """;

    private const string ValidUpgrades = """
    [ { "id": "might", "name": "Might", "description": "+damage", "rarity": 1, "kind": "Damage", "magnitude": 2, "maxStacks": 5 } ]
    """;

    private const string ValidWaves = """
    [ { "wave": 1, "enemyId": "grunt", "count": 5, "spawnInterval": 1.2 } ]
    """;

    [Fact]
    public void DefaultContent_Validates()
    {
        var content = GameContent.Default();
        content.Validate();
        Assert.Equal(3, content.Enemies.Count);
        Assert.Equal(13, content.Upgrades.Count);
        Assert.Equal(4, content.Waves.Count);
    }

    [Fact]
    public void LoadFromJson_ReturnsContent()
    {
        var content = ContentLoader.LoadFromJson(ValidEnemies, ValidUpgrades, ValidWaves);
        Assert.Single(content.Enemies);
        Assert.Single(content.Upgrades);
        Assert.Single(content.Waves);
        Assert.Equal("grunt", content.PrimaryEnemySpec().Id);
    }

    [Fact]
    public void DuplicateEnemyId_Throws()
    {
        const string duplicated = """
        [ { "id": "grunt", "maxHealth": 20, "radius": 0.5 }, { "id": "grunt", "maxHealth": 30, "radius": 0.5 } ]
        """;
        Assert.Throws<ContentLoadException>(() => ContentLoader.LoadFromJson(duplicated, ValidUpgrades, ValidWaves));
    }

    [Fact]
    public void UnknownEnemyReference_Throws()
    {
        const string badWaves = """
        [ { "wave": 1, "enemyId": "ghost", "count": 5 } ]
        """;
        Assert.Throws<ContentLoadException>(() => ContentLoader.LoadFromJson(ValidEnemies, ValidUpgrades, badWaves));
    }

    [Fact]
    public void MalformedJson_Throws()
    {
        Assert.Throws<ContentLoadException>(() => ContentLoader.LoadFromJson("{ not json", ValidUpgrades, ValidWaves));
    }

    [Fact]
    public void Load_FromContentDirectory()
    {
        string? directory = FindContentDirectory();
        Assert.NotNull(directory);

        var content = ContentLoader.Load(directory!);
        Assert.True(content.Enemies.Count >= 3);
        Assert.True(content.Upgrades.Count >= 6);
    }

    private static string? FindContentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 8 && directory is not null; i++)
        {
            string candidate = Path.Combine(directory.FullName, "content");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        return null;
    }
}
