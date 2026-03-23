using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShop", menuName = "RPG/Shop")]
public class ShopSO : ScriptableObject
{
    public string shopName;

    [Serializable]
    public struct EquipmentEntry
    {
        public EquipmentSO item;
        public int         price;
    }

    [Serializable]
    public struct ConsumableEntry
    {
        public ConsumableSO item;
        public int          price;
    }

    public EquipmentEntry[]  equipmentItems;
    public ConsumableEntry[] consumableItems;
}
