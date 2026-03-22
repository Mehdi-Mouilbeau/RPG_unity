using UnityEngine;

/// <summary>
/// Pure data service for equipment rarity rules.
/// Stat multipliers, effect slot counts, rarity colors, upgrade ladder.
/// </summary>
public static class RaritySystem
{
    /// <summary>Multiplier applied to all base stats of the equipment piece.</summary>
    public static float GetStatMultiplier(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common    => 1.00f,
        EquipmentRarity.Uncommon  => 1.10f,
        EquipmentRarity.Rare      => 1.20f,
        EquipmentRarity.Epic      => 1.35f,
        EquipmentRarity.Legendary => 1.50f,
        _                         => 1.00f,
    };

    /// <summary>How many rarity-granted effect slots this rarity provides.</summary>
    public static int GetEffectSlots(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Rare      => 1,
        EquipmentRarity.Epic      => 2,
        EquipmentRarity.Legendary => 2,
        _                         => 0,
    };

    /// <summary>UI color for this rarity tier.</summary>
    public static Color GetRarityColor(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common    => new Color(0.70f, 0.70f, 0.70f), // grey
        EquipmentRarity.Uncommon  => new Color(0.20f, 0.80f, 0.20f), // green
        EquipmentRarity.Rare      => new Color(0.20f, 0.40f, 1.00f), // blue
        EquipmentRarity.Epic      => new Color(0.60f, 0.10f, 0.90f), // purple
        EquipmentRarity.Legendary => new Color(1.00f, 0.75f, 0.00f), // gold
        _                         => Color.white,
    };

    /// <summary>Returns the next rarity tier. Returns the same tier if already Legendary.</summary>
    public static EquipmentRarity Upgrade(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common   => EquipmentRarity.Uncommon,
        EquipmentRarity.Uncommon => EquipmentRarity.Rare,
        EquipmentRarity.Rare     => EquipmentRarity.Epic,
        EquipmentRarity.Epic     => EquipmentRarity.Legendary,
        _                        => rarity,
    };

    /// <summary>Whether this item can still be upgraded by the Forge.</summary>
    public static bool CanUpgrade(EquipmentRarity rarity) =>
        rarity != EquipmentRarity.Legendary;

    /// <summary>Whether this item is eligible for enchantment (Rare or better).</summary>
    public static bool CanEnchant(EquipmentRarity rarity) =>
        rarity >= EquipmentRarity.Rare;
}
