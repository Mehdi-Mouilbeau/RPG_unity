using NUnit.Framework;
using UnityEngine;

public class EnchantSystemTests
{
    private EquipmentSO MakeItem(EquipmentRarity rarity, string[] pool = null)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.rarity     = rarity;
        item.effectPool = pool ?? new[] { "crit_5", "atk_3", "agi_2", "lck_4", "mp_regen" };
        return item;
    }

    // Standard enchant resources (Pierre runique + Essence élémentaire per spec §5.4)
    private static readonly string[] GoodResources = { "PierreRunique", "EssenceElementaire" };
    private static readonly string[] RequiredResources = { "PierreRunique", "EssenceElementaire" };

    [Test]
    public void GetOptions_CommonItem_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Common);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_RareItem_SucceedsWithUpTo3Options()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Options);
        Assert.LessOrEqual(result.Options.Length, 3);
        Assert.Greater(result.Options.Length, 0);
    }

    [Test]
    public void GetOptions_SmallPool_ReturnsAllOptions()
    {
        var item   = MakeItem(EquipmentRarity.Rare, pool: new[] { "crit_5", "atk_3" });
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Options.Length); // only 2 in pool
    }

    [Test]
    public void GetOptions_InsufficientGold_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 100, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_MissingEnchantResource_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: new[] { "PierreRunique" },           // missing EssenceElementaire
            requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_EmptyPool_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare, pool: new string[0]);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Apply_SetsEnchantmentEffect()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var effect = new EquipmentEffect { effectId = "crit_5", displayName = "Critique +5%" };
        var result = EnchantSystem.Apply(item, effect);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("crit_5", item.enchantmentEffect.effectId);
    }

    [Test]
    public void Apply_CommonItem_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Common);
        var effect = new EquipmentEffect { effectId = "crit_5", displayName = "Critique +5%" };
        var result = EnchantSystem.Apply(item, effect);
        Assert.IsFalse(result.Success);
        Assert.IsNull(item.enchantmentEffect);
    }

    [Test]
    public void Apply_ReplacesExistingEnchantment()
    {
        var item    = MakeItem(EquipmentRarity.Rare);
        var effect1 = new EquipmentEffect { effectId = "crit_5",  displayName = "Critique +5%" };
        var effect2 = new EquipmentEffect { effectId = "atk_3",   displayName = "ATK +3" };
        EnchantSystem.Apply(item, effect1);
        EnchantSystem.Apply(item, effect2);
        Assert.AreEqual("atk_3", item.enchantmentEffect.effectId);
    }
}
