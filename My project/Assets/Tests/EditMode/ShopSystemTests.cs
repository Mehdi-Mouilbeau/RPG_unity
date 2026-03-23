using NUnit.Framework;

public class ShopSystemTests
{
    // ── ShopSO data ───────────────────────────────────────────────────────

    [Test]
    public void ShopSO_EquipmentEntry_HasItemAndPrice()
    {
        var shop      = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        var equipment = UnityEngine.ScriptableObject.CreateInstance<EquipmentSO>();
        equipment.itemName = "Épée";

        shop.equipmentItems = new[] { new ShopSO.EquipmentEntry { item = equipment, price = 100 } };

        Assert.AreEqual(1, shop.equipmentItems.Length);
        Assert.AreEqual("Épée", shop.equipmentItems[0].item.itemName);
        Assert.AreEqual(100,    shop.equipmentItems[0].price);

        UnityEngine.Object.DestroyImmediate(equipment);
        UnityEngine.Object.DestroyImmediate(shop);
    }

    [Test]
    public void ShopSO_ConsumableEntry_HasItemAndPrice()
    {
        var shop      = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        var consumable = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        consumable.itemName = "Potion de Soin";

        shop.consumableItems = new[] { new ShopSO.ConsumableEntry { item = consumable, price = 50 } };

        Assert.AreEqual(1, shop.consumableItems.Length);
        Assert.AreEqual("Potion de Soin", shop.consumableItems[0].item.itemName);
        Assert.AreEqual(50,               shop.consumableItems[0].price);

        UnityEngine.Object.DestroyImmediate(consumable);
        UnityEngine.Object.DestroyImmediate(shop);
    }

    // ── Inventory.Bag (used by buy logic) ─────────────────────────────────

    [Test]
    public void Inventory_Bag_CanAddEquipment()
    {
        var character = new CharacterData();
        character.Initialize("Test", 100, 50, 10, 5, 5, 5, 8, 3);

        var item = UnityEngine.ScriptableObject.CreateInstance<EquipmentSO>();
        item.itemName = "Épée";
        item.slot     = EquipmentSlot.MainWeapon;

        character.Inventory.Bag.Add(item);

        Assert.AreEqual(1, character.Inventory.Bag.Count);
        Assert.AreEqual("Épée", character.Inventory.Bag[0].itemName);

        UnityEngine.Object.DestroyImmediate(item);
    }

    [Test]
    public void Inventory_Bag_DefaultEmpty()
    {
        var character = new CharacterData();
        character.Initialize("Test", 100, 50, 10, 5, 5, 5, 8, 3);
        Assert.AreEqual(0, character.Inventory.Bag.Count);
    }

    // ── ShopOpenedEvent ───────────────────────────────────────────────────

    [Test]
    public void ShopOpenedEvent_HasShopField()
    {
        var shop = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        shop.shopName = "Marchand";

        var evt = new ShopOpenedEvent { Shop = shop };

        Assert.AreEqual("Marchand", evt.Shop.shopName);
        UnityEngine.Object.DestroyImmediate(shop);
    }
}
