using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

public class SaveSystemTests
{
    private string _testPath;

    [SetUp]
    public void SetUp()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "rpg_test_save.json");
        SaveSystem.OverridePath = _testPath;
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.OverridePath = null;
        if (File.Exists(_testPath)) File.Delete(_testPath);
    }

    [Test]
    public void SaveThenLoad_RestoresAllFields()
    {
        var data = new SaveData
        {
            characterName = "Kael",
            classKey      = "Guerrier",
            raceKey       = "Humain",
            level         = 5,
            experience    = 500,
            currentHP     = 120,
            currentMP     = 50,
            gold          = 200,
            companionKey  = "loup_des_ombres"
        };
        data.flags.Add(new ProgressionFlags.FlagEntry { key = "village_visited", value = true });
        data.equippedItemKeys.Add("iron_sword");

        SaveSystem.Save(data);
        var loaded = SaveSystem.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual("Kael",            loaded.characterName);
        Assert.AreEqual("Guerrier",        loaded.classKey);
        Assert.AreEqual("Humain",          loaded.raceKey);
        Assert.AreEqual(5,                 loaded.level);
        Assert.AreEqual(500,               loaded.experience);
        Assert.AreEqual(200,               loaded.gold);
        Assert.AreEqual("loup_des_ombres", loaded.companionKey);
        Assert.AreEqual(120,               loaded.currentHP);
        Assert.AreEqual(50,                loaded.currentMP);
        Assert.AreEqual(1,                 loaded.flags.Count);
        Assert.AreEqual("village_visited", loaded.flags[0].key);
        Assert.AreEqual(true,              loaded.flags[0].value);
        Assert.AreEqual(1,                 loaded.equippedItemKeys.Count);
        Assert.AreEqual("iron_sword",      loaded.equippedItemKeys[0]);
    }

    [Test]
    public void Load_NoFile_ReturnsNull()
    {
        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "nonexistent_rpg.json");
        Assert.IsNull(SaveSystem.Load());
    }

    [Test]
    public void HasSave_AfterSave_ReturnsTrue()
    {
        SaveSystem.Save(new SaveData { characterName = "Test" });
        Assert.IsTrue(SaveSystem.HasSave());
    }

    [Test]
    public void HasSave_NoFile_ReturnsFalse()
    {
        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "nonexistent2_rpg.json");
        Assert.IsFalse(SaveSystem.HasSave());
    }

    [Test]
    public void Delete_RemovesFile()
    {
        SaveSystem.Save(new SaveData { characterName = "Test" });
        SaveSystem.Delete();
        Assert.IsFalse(SaveSystem.HasSave());
    }
}
