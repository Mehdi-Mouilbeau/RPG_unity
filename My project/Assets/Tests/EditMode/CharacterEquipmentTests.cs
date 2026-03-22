using NUnit.Framework;
using UnityEngine;

public class CharacterEquipmentTests
{
    private CharacterData MakeCharacter(int atk = 10, int def = 5, int hp = 100)
    {
        var c = new CharacterData();
        c.Initialize("Hero", hp, 50, atk, def, 8, 4, 6, 3);
        return c;
    }

    private EquipmentSO MakeItem(EquipmentSlot slot, int atkBonus = 0, int defBonus = 0, int hpBonus = 0)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.slot     = slot;
        item.atkBonus = atkBonus;
        item.defBonus = defBonus;
        item.hpBonus  = hpBonus;
        item.rarity   = EquipmentRarity.Common;
        return item;
    }

    [Test]
    public void ATK_WithNoEquipment_EqualsBaseATK()
    {
        var c = MakeCharacter(atk: 10);
        Assert.AreEqual(10, c.ATK);
    }

    [Test]
    public void ATK_WithWeapon_IncludesBonus()
    {
        var c    = MakeCharacter(atk: 10);
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        c.Inventory.Equip(sword);
        Assert.AreEqual(15, c.ATK);
    }

    [Test]
    public void MaxHP_WithArmor_IncludesBonus()
    {
        var c     = MakeCharacter(hp: 100);
        var armor = MakeItem(EquipmentSlot.Armor, hpBonus: 30);
        c.Inventory.Equip(armor);
        Assert.AreEqual(130, c.MaxHP);
    }

    [Test]
    public void Unequip_RemovesBonus()
    {
        var c    = MakeCharacter(atk: 10);
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        c.Inventory.Equip(sword);
        c.Inventory.Unequip(EquipmentSlot.MainWeapon);
        Assert.AreEqual(10, c.ATK);
    }

    [Test]
    public void CurrentHP_AfterInitialize_EqualsMaxHP()
    {
        var c = MakeCharacter(hp: 100);
        Assert.AreEqual(c.MaxHP, c.CurrentHP);
    }

    [Test]
    public void Heal_CappedByMaxHPWithEquipment()
    {
        var c     = MakeCharacter(hp: 100);
        var armor = MakeItem(EquipmentSlot.Armor, hpBonus: 50);
        c.Inventory.Equip(armor);
        c.TakeDamage(80);
        c.Heal(200);
        Assert.AreEqual(c.MaxHP, c.CurrentHP); // capped at 150
    }
}
