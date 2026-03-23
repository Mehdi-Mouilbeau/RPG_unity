using NUnit.Framework;
using System.Collections.Generic;

public class CombatUIFoundationTests
{
    [Test]
    public void CharacterData_XPReward_DefaultsToZero()
    {
        var c = new CharacterData();
        Assert.AreEqual(0, c.XPReward);
    }

    [Test]
    public void CharacterData_SourceLootTable_DefaultsToNull()
    {
        var c = new CharacterData();
        Assert.IsNull(c.SourceLootTable);
    }

    [Test]
    public void Inventory_Consumables_EmptyByDefault()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        Assert.AreEqual(0, c.Inventory.Consumables.Count);
    }

    [Test]
    public void Inventory_AddConsumable_IncreasesCount()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        var potion = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion";
        potion.effectType = ConsumableEffectType.HealHP;
        potion.value = 0.3f;

        c.Inventory.AddConsumable(potion);

        Assert.AreEqual(1, c.Inventory.Consumables.Count);
        UnityEngine.Object.DestroyImmediate(potion);
    }

    [Test]
    public void Inventory_RemoveConsumable_DecreasesCount()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        var potion = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion";

        c.Inventory.AddConsumable(potion);
        c.Inventory.RemoveConsumable(potion);

        Assert.AreEqual(0, c.Inventory.Consumables.Count);
        UnityEngine.Object.DestroyImmediate(potion);
    }

    [Test]
    public void Inventory_AddNull_DoesNothing()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        c.Inventory.AddConsumable(null);
        Assert.AreEqual(0, c.Inventory.Consumables.Count);
    }

    [Test]
    public void BattleEndedEvent_IsClass_SupportsNullLoot()
    {
        var evt = new BattleEndedEvent { PlayerWon = true, XPGained = 50, Loot = null };
        Assert.IsTrue(evt.PlayerWon);
        Assert.AreEqual(50, evt.XPGained);
        Assert.IsNull(evt.Loot);
    }

    [Test]
    public void BattleEndedEvent_LootList_CanBePopulated()
    {
        var evt = new BattleEndedEvent
        {
            PlayerWon = true,
            XPGained = 100,
            Loot = new List<EquipmentSO>()
        };
        Assert.AreEqual(0, evt.Loot.Count);
    }
}
