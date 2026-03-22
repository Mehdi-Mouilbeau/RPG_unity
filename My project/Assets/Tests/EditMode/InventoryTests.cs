using NUnit.Framework;
using UnityEngine;

public class InventoryTests
{
    // Helper: create a CharacterData with known stats
    private CharacterData MakeCharacter(string name = "Hero",
        int hp = 100, int mp = 50, int atk = 10, int def = 5,
        int mag = 8, int res = 4, int agi = 6, int lck = 3)
    {
        var c = new CharacterData();
        c.Initialize(name, hp, mp, atk, def, mag, res, agi, lck);
        return c;
    }

    // Helper: create an EquipmentSO in memory (no asset file needed)
    private EquipmentSO MakeItem(EquipmentSlot slot, int atkBonus = 0, int defBonus = 0,
        int hpBonus = 0, EquipmentRarity rarity = EquipmentRarity.Common)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.slot      = slot;
        item.atkBonus  = atkBonus;
        item.defBonus  = defBonus;
        item.hpBonus   = hpBonus;
        item.rarity    = rarity;
        return item;
    }

    [Test]
    public void Equip_PutsItemInEquippedSlot()
    {
        var c    = MakeCharacter();
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);

        c.Inventory.Equip(sword);

        Assert.AreEqual(sword, c.Inventory.Equipped[EquipmentSlot.MainWeapon]);
    }

    [Test]
    public void Equip_SameSlotTwice_MovesFirstToBag()
    {
        var c      = MakeCharacter();
        var sword1 = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        var sword2 = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 8);

        c.Inventory.Equip(sword1);
        c.Inventory.Equip(sword2);

        Assert.AreEqual(sword2, c.Inventory.Equipped[EquipmentSlot.MainWeapon]);
        Assert.Contains(sword1, c.Inventory.Bag);
    }

    [Test]
    public void Unequip_MovesItemToBag()
    {
        var c    = MakeCharacter();
        var helm = MakeItem(EquipmentSlot.Helmet, defBonus: 3);

        c.Inventory.Equip(helm);
        c.Inventory.Unequip(EquipmentSlot.Helmet);

        Assert.IsFalse(c.Inventory.Equipped.ContainsKey(EquipmentSlot.Helmet));
        Assert.Contains(helm, c.Inventory.Bag);
    }

    [Test]
    public void Equip_ItemAlreadyInBag_RemovedFromBag()
    {
        var c    = MakeCharacter();
        var ring = MakeItem(EquipmentSlot.Ring1);
        c.Inventory.Bag.Add(ring);   // manually add to bag first

        c.Inventory.Equip(ring);

        Assert.IsFalse(c.Inventory.Bag.Contains(ring));
        Assert.AreEqual(ring, c.Inventory.Equipped[EquipmentSlot.Ring1]);
    }

    [Test]
    public void GetTotalATK_SumsAllEquippedItems()
    {
        var c     = MakeCharacter();
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        var ring  = MakeItem(EquipmentSlot.Ring1,      atkBonus: 2);

        c.Inventory.Equip(sword);
        c.Inventory.Equip(ring);

        // Common rarity multiplier = 1.0, so effective = base
        Assert.AreEqual(7, c.Inventory.GetTotalATK());
    }

    [Test]
    public void GetTotalATK_UncommonItem_AppliesMultiplier()
    {
        var c    = MakeCharacter();
        var item = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 10,
            rarity: EquipmentRarity.Uncommon); // ×1.1 → 11

        c.Inventory.Equip(item);

        Assert.AreEqual(11, c.Inventory.GetTotalATK());
    }

    [Test]
    public void GetTotalHP_EmptyInventory_ReturnsZero()
    {
        var c = MakeCharacter();
        Assert.AreEqual(0, c.Inventory.GetTotalHP());
    }

    [Test]
    public void Equip_PublishesItemEquippedEvent()
    {
        var c     = MakeCharacter();
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);

        ItemEquippedEvent? received = null;
        System.Action<ItemEquippedEvent> handler = e => received = e;
        EventBus.Subscribe<ItemEquippedEvent>(handler);

        c.Inventory.Equip(sword);

        EventBus.Unsubscribe<ItemEquippedEvent>(handler);

        Assert.IsNotNull(received);
        Assert.AreEqual(sword, received?.Item);
    }
}
