using NUnit.Framework;
using System.IO;

public class SaveConsumablesTests
{
    [TearDown]
    public void TearDown()
    {
        // Garantit que OverridePath est réinitialisé même si un test échoue en cours de route
        SaveSystem.OverridePath = null;
    }

    // ── SaveData ──────────────────────────────────────────────────────────

    [Test]
    public void SaveData_ConsumableKeys_DefaultEmpty()
    {
        var data = new SaveData();
        Assert.IsNotNull(data.consumableKeys);
        Assert.AreEqual(0, data.consumableKeys.Count);
    }

    // ── GameDataRegistry ──────────────────────────────────────────────────

    [Test]
    public void GameDataRegistry_GetConsumable_ReturnsMatch()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        var potion   = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion de Soin";
        registry.consumables = new[] { potion };

        var result = registry.GetConsumable("Potion de Soin");

        Assert.AreEqual(potion, result);
        UnityEngine.Object.DestroyImmediate(potion);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    [Test]
    public void GameDataRegistry_GetConsumable_UnknownKey_ReturnsNull()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        registry.consumables = System.Array.Empty<ConsumableSO>();

        var result = registry.GetConsumable("Inexistant");

        Assert.IsNull(result);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    [Test]
    public void GameDataRegistry_GetConsumable_NullKey_ReturnsNull()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        registry.consumables = System.Array.Empty<ConsumableSO>();

        var result = registry.GetConsumable(null);

        Assert.IsNull(result);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    // ── SaveSystem round-trip ─────────────────────────────────────────────

    [Test]
    public void SaveData_ConsumableKeys_RoundTrip()
    {
        var data = new SaveData();
        data.consumableKeys.Add("Potion de Soin");
        data.consumableKeys.Add("Antidote");

        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "rpg_test_consumables.json");
        SaveSystem.Save(data);
        var loaded = SaveSystem.Load();
        SaveSystem.Delete();
        SaveSystem.OverridePath = null;

        Assert.AreEqual(2, loaded.consumableKeys.Count);
        Assert.AreEqual("Potion de Soin", loaded.consumableKeys[0]);
        Assert.AreEqual("Antidote",       loaded.consumableKeys[1]);
    }
}
