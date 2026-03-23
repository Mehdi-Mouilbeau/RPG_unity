using System;
using System.Collections.Generic;

/// <summary>
/// Runtime inventory for one character.
/// Tracks equipped items (one per slot) and bag (unequipped items).
/// Publishes ItemEquippedEvent via EventBus when equipment changes.
/// </summary>
public class Inventory
{
    /// <summary>Currently equipped items, keyed by slot.</summary>
    public Dictionary<EquipmentSlot, EquipmentSO> Equipped { get; } = new();

    /// <summary>Unequipped items stored in the bag.</summary>
    public List<EquipmentSO> Bag { get; } = new();

    /// <summary>Objets consommables (potions, antidotes, etc.).</summary>
    public List<ConsumableSO> Consumables { get; } = new List<ConsumableSO>();

    public void AddConsumable(ConsumableSO c)
    {
        if (c != null) Consumables.Add(c);
    }

    public void RemoveConsumable(ConsumableSO c) => Consumables.Remove(c);

    private readonly CharacterData _owner;

    public Inventory(CharacterData owner) => _owner = owner;

    /// <summary>
    /// Equip an item. If the slot is already occupied, the old item moves to the Bag.
    /// If the item was already in the Bag, it is removed from there first.
    /// Publishes ItemEquippedEvent.
    /// </summary>
    public void Equip(EquipmentSO item)
    {
        if (item == null) return;

        // Move previous occupant to Bag
        if (Equipped.TryGetValue(item.slot, out var previous) && previous != item)
            Bag.Add(previous);

        // Remove from Bag in case it was stored there
        Bag.Remove(item);

        Equipped[item.slot] = item;
        EventBus.Publish(new ItemEquippedEvent { Owner = _owner, Item = item });
    }

    /// <summary>
    /// Unequip the item in the given slot and move it to the Bag.
    /// Publishes ItemEquippedEvent with Item = null to signal a slot was cleared.
    /// </summary>
    public void Unequip(EquipmentSlot slot)
    {
        if (!Equipped.TryGetValue(slot, out var item)) return;
        Equipped.Remove(slot);
        Bag.Add(item);
        EventBus.Publish(new ItemEquippedEvent { Owner = _owner, Item = null });
    }

    // ── Stat aggregation ──────────────────────────────────────────────────

    public int GetTotalHP()  => Sum(e => e.EffectiveHP);
    public int GetTotalMP()  => Sum(e => e.EffectiveMP);
    public int GetTotalATK() => Sum(e => e.EffectiveATK);
    public int GetTotalDEF() => Sum(e => e.EffectiveDEF);
    public int GetTotalMAG() => Sum(e => e.EffectiveMAG);
    public int GetTotalRES() => Sum(e => e.EffectiveRES);
    public int GetTotalAGI() => Sum(e => e.EffectiveAGI);
    public int GetTotalLCK() => Sum(e => e.EffectiveLCK);

    private int Sum(Func<EquipmentSO, int> selector)
    {
        int total = 0;
        foreach (var item in Equipped.Values)
            total += selector(item);
        return total;
    }
}
