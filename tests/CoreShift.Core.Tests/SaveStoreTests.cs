using CoreShift.Core.Progression;
using CoreShift.Data.Serialization;

namespace CoreShift.Core.Tests;

public class SaveStoreTests
{
    private static SaveData Sample()
    {
        var data = new SaveData
        {
            Currency = 250,
            BestWave = 12,
            TotalKills = 340,
            TotalPlaySeconds = 1234.5,
        };
        data.UnlockedUpgrades.Add("multishot");
        data.UnlockedUpgrades.Add("haste");
        data.PermanentUpgrades.Add(new PermanentUpgrade { Id = "power", Rank = 3 });
        data.PermanentUpgrades.Add(new PermanentUpgrade { Id = "vigor", Rank = 2 });
        return data;
    }

    [Fact]
    public void Json_RoundTrip_PreservesData()
    {
        string path = Path.Combine(Path.GetTempPath(), "coreshift-json-" + Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonSaveStore();
            var original = Sample();
            store.Save(original, path);
            var loaded = store.Load(path);
            AssertEquivalent(original, loaded);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Xml_RoundTrip_PreservesData()
    {
        string path = Path.Combine(Path.GetTempPath(), "coreshift-xml-" + Guid.NewGuid() + ".xml");
        try
        {
            var store = new XmlSaveStore();
            var original = Sample();
            store.Save(original, path);
            var loaded = store.Load(path);
            AssertEquivalent(original, loaded);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void JsonAndXml_ProduceEquivalentData()
    {
        string jsonPath = Path.Combine(Path.GetTempPath(), "coreshift-j-" + Guid.NewGuid() + ".json");
        string xmlPath = Path.Combine(Path.GetTempPath(), "coreshift-x-" + Guid.NewGuid() + ".xml");
        try
        {
            var original = Sample();
            new JsonSaveStore().Save(original, jsonPath);
            new XmlSaveStore().Save(original, xmlPath);
            var fromJson = new JsonSaveStore().Load(jsonPath);
            var fromXml = new XmlSaveStore().Load(xmlPath);
            AssertEquivalent(fromJson, fromXml);
            AssertEquivalent(original, fromJson);
        }
        finally
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [Fact]
    public void FutureSchemaVersion_IsRejected()
    {
        string jsonPath = Path.Combine(Path.GetTempPath(), "coreshift-future-" + Guid.NewGuid() + ".json");
        try
        {
            var store = new JsonSaveStore();
            var data = Sample();
            data.SchemaVersion = SaveData.CurrentSchemaVersion + 1;
            store.Save(data, jsonPath);
            Assert.Throws<SaveVersionException>(() => store.Load(jsonPath));
        }
        finally
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
        }
    }

    [Fact]
    public void MissingFile_ReturnsDefault()
    {
        string missing = Path.Combine(Path.GetTempPath(), "coreshift-missing-" + Guid.NewGuid() + ".json");
        var loaded = new JsonSaveStore().Load(missing);
        Assert.Equal(0, loaded.Currency);
        Assert.Empty(loaded.UnlockedUpgrades);
    }

    [Fact]
    public void CorruptJson_Throws()
    {
        string path = Path.Combine(Path.GetTempPath(), "coreshift-corrupt-" + Guid.NewGuid() + ".json");
        try
        {
            File.WriteAllText(path, "{ this is not valid json");
            Assert.Throws<SaveLoadException>(() => new JsonSaveStore().Load(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static void AssertEquivalent(SaveData a, SaveData b)
    {
        Assert.Equal(a.SchemaVersion, b.SchemaVersion);
        Assert.Equal(a.Currency, b.Currency);
        Assert.Equal(a.BestWave, b.BestWave);
        Assert.Equal(a.TotalKills, b.TotalKills);
        Assert.Equal(a.TotalPlaySeconds, b.TotalPlaySeconds, 3);
        Assert.Equal(a.UnlockedUpgrades, b.UnlockedUpgrades);
        Assert.Equal(a.PermanentUpgrades.Count, b.PermanentUpgrades.Count);
        for (int i = 0; i < a.PermanentUpgrades.Count; i++)
        {
            Assert.Equal(a.PermanentUpgrades[i].Id, b.PermanentUpgrades[i].Id);
            Assert.Equal(a.PermanentUpgrades[i].Rank, b.PermanentUpgrades[i].Rank);
        }
    }
}
