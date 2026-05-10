using UnityEngine;

/// <summary>
/// Data asset for one equipment piece.
/// Base stats are defined in the Inspector; EffectiveXxx properties apply the rarity multiplier.
/// </summary>
[CreateAssetMenu(menuName = "RPG/Equipment/Item", fileName = "New Equipment")]
public class EquipmentSO : ScriptableObject
{
    [Header("Identity")]
    public string         itemName;
    [TextArea] public string description;
    public EquipmentSlot  slot;
    public EquipmentRarity rarity;

    [Header("Base Stats (before rarity multiplier)")]
    public int hpBonus;
    public int mpBonus;
    public int atkBonus;
    public int defBonus;
    public int magBonus;
    public int resBonus;
    public int agiBonus;
    public int lckBonus;

    [Header("Effects")]
    /// <summary>Effects granted by rarity (1 for Rare, 2 for Epic/Legendary).</summary>
    public EquipmentEffect[] rarityEffects;
    /// <summary>Enchantment slot — null means no enchantment.</summary>
    public EquipmentEffect enchantmentEffect;

    [Header("Crafting / Forge")]
    /// <summary>Material names required to upgrade this item's rarity.</summary>
    public string[] craftingMaterials;
    public int      craftingGoldCost = 100;

    [Header("Effect Pool (for Forge & Enchant)")]
    /// <summary>
    /// IDs of possible effects that can be granted on upgrade or enchantment.
    /// The Forge/EnchantSystem draws randomly from this pool.
    /// </summary>
    public string[] effectPool;

    [Header("Restrictions (vide = tous)")]
    /// <summary>Classes autorisées. Vide = accessible à toutes les classes.</summary>
    public ClassSO[] allowedClasses;
    /// <summary>Races autorisées. Vide = accessible à toutes les races.</summary>
    public RaceSO[]  allowedRaces;

    // ── Effective stat helpers (apply rarity multiplier) ──────────────────

    public int EffectiveHP  => ApplyRarity(hpBonus);
    public int EffectiveMP  => ApplyRarity(mpBonus);
    public int EffectiveATK => ApplyRarity(atkBonus);
    public int EffectiveDEF => ApplyRarity(defBonus);
    public int EffectiveMAG => ApplyRarity(magBonus);
    public int EffectiveRES => ApplyRarity(resBonus);
    public int EffectiveAGI => ApplyRarity(agiBonus);
    public int EffectiveLCK => ApplyRarity(lckBonus);

    private int ApplyRarity(int baseValue) =>
        Mathf.RoundToInt(baseValue * RaritySystem.GetStatMultiplier(rarity));
}
