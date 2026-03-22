using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "RPG/Campaign/LootTable")]
public class LootTableSO : ScriptableObject
{
    [Serializable]
    public struct LootEntry
    {
        public EquipmentSO equipment;
        [Range(0f, 1f)] public float weight;
    }

    public LootEntry[] entries;

    public EquipmentSO Roll()
    {
        if (entries == null || entries.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var e in entries) totalWeight += e.weight;
        if (totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var e in entries)
        {
            cumulative += e.weight;
            if (roll <= cumulative) return e.equipment;
        }
        return entries[entries.Length - 1].equipment;
    }
}
