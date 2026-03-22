using NUnit.Framework;
using UnityEngine;

public class ForgeSystemTests
{
    private EquipmentSO MakeSword(EquipmentRarity rarity = EquipmentRarity.Common)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.itemName         = "Épée";
        item.slot             = EquipmentSlot.MainWeapon;
        item.rarity           = rarity;
        item.craftingGoldCost = 100;
        item.craftingMaterials = new[] { "FerBrut", "Charbon" };
        return item;
    }

    private string[] GoodMaterials() => new[] { "FerBrut", "Charbon", "Pierre" };

    [Test]
    public void Craft_Success_UpgradesRarity()
    {
        var sword  = MakeSword(EquipmentRarity.Common);
        var result = ForgeSystem.Craft(sword, gold: 200, playerMaterials: GoodMaterials());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EquipmentRarity.Uncommon, sword.rarity);
    }

    [Test]
    public void Craft_InsufficientGold_Fails()
    {
        var sword  = MakeSword();
        var result = ForgeSystem.Craft(sword, gold: 50, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EquipmentRarity.Common, sword.rarity); // unchanged
    }

    [Test]
    public void Craft_MissingMaterial_Fails()
    {
        var sword  = MakeSword();
        var result = ForgeSystem.Craft(sword, gold: 200,
            playerMaterials: new[] { "FerBrut" }); // missing Charbon
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EquipmentRarity.Common, sword.rarity);
    }

    [Test]
    public void Craft_LegendaryItem_Fails()
    {
        var sword  = MakeSword(EquipmentRarity.Legendary);
        var result = ForgeSystem.Craft(sword, gold: 1000, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Craft_NullItem_Fails()
    {
        var result = ForgeSystem.Craft(null, gold: 200, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Craft_Chain_CommonToUncommonToRare()
    {
        var sword = MakeSword(EquipmentRarity.Common);
        ForgeSystem.Craft(sword, 200, GoodMaterials());
        Assert.AreEqual(EquipmentRarity.Uncommon, sword.rarity);
        ForgeSystem.Craft(sword, 200, GoodMaterials());
        Assert.AreEqual(EquipmentRarity.Rare, sword.rarity);
    }
}
