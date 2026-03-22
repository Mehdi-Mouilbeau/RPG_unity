using NUnit.Framework;
using UnityEngine;

public class RaritySystemTests
{
    [Test]
    public void GetStatMultiplier_Common_Returns1()
    {
        Assert.AreEqual(1.00f, RaritySystem.GetStatMultiplier(EquipmentRarity.Common), 0.001f);
    }

    [Test]
    public void GetStatMultiplier_Legendary_Returns1Point5()
    {
        Assert.AreEqual(1.50f, RaritySystem.GetStatMultiplier(EquipmentRarity.Legendary), 0.001f);
    }

    [Test]
    public void GetEffectSlots_Common_Returns0()
    {
        Assert.AreEqual(0, RaritySystem.GetEffectSlots(EquipmentRarity.Common));
    }

    [Test]
    public void GetEffectSlots_Rare_Returns1()
    {
        Assert.AreEqual(1, RaritySystem.GetEffectSlots(EquipmentRarity.Rare));
    }

    [Test]
    public void GetEffectSlots_Epic_Returns2()
    {
        Assert.AreEqual(2, RaritySystem.GetEffectSlots(EquipmentRarity.Epic));
    }

    [Test]
    public void Upgrade_Common_ReturnsUncommon()
    {
        Assert.AreEqual(EquipmentRarity.Uncommon, RaritySystem.Upgrade(EquipmentRarity.Common));
    }

    [Test]
    public void Upgrade_Legendary_StaysLegendary()
    {
        Assert.AreEqual(EquipmentRarity.Legendary, RaritySystem.Upgrade(EquipmentRarity.Legendary));
    }

    [Test]
    public void CanUpgrade_Legendary_ReturnsFalse()
    {
        Assert.IsFalse(RaritySystem.CanUpgrade(EquipmentRarity.Legendary));
    }

    [Test]
    public void CanEnchant_Common_ReturnsFalse()
    {
        Assert.IsFalse(RaritySystem.CanEnchant(EquipmentRarity.Common));
    }

    [Test]
    public void CanEnchant_Rare_ReturnsTrue()
    {
        Assert.IsTrue(RaritySystem.CanEnchant(EquipmentRarity.Rare));
    }

    [Test]
    public void GetRarityColor_Legendary_IsGold()
    {
        var color = RaritySystem.GetRarityColor(EquipmentRarity.Legendary);
        Assert.Greater(color.r, 0.9f);   // gold = R high
        Assert.Greater(color.g, 0.6f);   // gold = G medium-high
        Assert.Less(color.b, 0.2f);      // gold = B low
    }
}
