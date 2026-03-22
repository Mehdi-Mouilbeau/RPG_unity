using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enchant system: offer random effects from an item's pool, then apply the chosen one.
/// Only Rare or better items can be enchanted (one enchantment slot per item).
/// Applying a new enchantment replaces the previous one.
///
/// Cost (per spec §5.4): enchantment resources (Pierre runique, Essence élémentaire) + gold.
/// Note: like ForgeSystem, Apply mutates the SO in place — use runtime copies, not asset files.
/// </summary>
public static class EnchantSystem
{
    public class OptionsResult
    {
        public bool Success;
        public string Message;
        public EquipmentEffect[] Options;
    }

    public class ApplyResult
    {
        public bool Success;
        public string Message;
    }

    /// <summary>
    /// Validates eligibility and returns up to 3 randomly chosen effects from the item's pool.
    /// Call this to present the player with enchantment choices before calling Apply.
    /// </summary>
    public static OptionsResult GetOptions(EquipmentSO item, int gold, int goldCost,
        string[] playerResources, string[] requiredResources)
    {
        if (item == null)
            return FailOptions("Aucun objet sélectionné.");

        if (!RaritySystem.CanEnchant(item.rarity))
            return FailOptions("L'enchantement requiert un objet Rare ou supérieur.");

        if (gold < goldCost)
            return FailOptions($"Or insuffisant. Requis : {goldCost} or.");

        if (requiredResources != null)
        {
            foreach (var res in requiredResources)
            {
                bool found = playerResources != null &&
                             System.Array.Exists(playerResources, r => r == res);
                if (!found) return FailOptions($"Ressource manquante : {res}.");
            }
        }

        if (item.effectPool == null || item.effectPool.Length == 0)
            return FailOptions("Aucun effet disponible pour cet objet.");

        // Draw up to 3 distinct effects from the pool (without replacement)
        var pool   = new List<string>(item.effectPool);
        var chosen = new List<EquipmentEffect>();
        int count  = System.Math.Min(3, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            chosen.Add(new EquipmentEffect
            {
                effectId    = pool[idx],
                displayName = pool[idx],
                // TODO Plan 4: resolve effectType and value from an effect registry keyed by effectId.
                effectType  = EquipmentEffectType.None,
            });
            pool.RemoveAt(idx);
        }

        return new OptionsResult { Success = true, Options = chosen.ToArray() };
    }

    /// <summary>
    /// Applies an enchantment effect to the item, replacing any existing enchantment.
    /// </summary>
    public static ApplyResult Apply(EquipmentSO item, EquipmentEffect effect)
    {
        if (item == null || effect == null)
            return new ApplyResult { Success = false, Message = "Données invalides." };

        if (!RaritySystem.CanEnchant(item.rarity))
            return new ApplyResult { Success = false, Message = "L'enchantement requiert un objet Rare ou supérieur." };

        item.enchantmentEffect = effect;
        return new ApplyResult { Success = true, Message = $"Enchantement appliqué : {effect.displayName}" };
    }

    private static OptionsResult FailOptions(string message) =>
        new OptionsResult { Success = false, Message = message };
}
