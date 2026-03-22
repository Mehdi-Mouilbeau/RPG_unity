/// <summary>
/// A single special effect on an equipment piece.
/// Used both for rarity-granted effects and the enchantment slot.
/// Data-only — combat integration reads these in ActionResolver (future plan).
/// </summary>
[System.Serializable]
public class EquipmentEffect
{
    public string          effectId;    // unique identifier, e.g. "crit_boost_5"
    public string          displayName; // shown in UI, e.g. "Coup critique +5%"
    public EquipmentEffectType effectType;
    public ElementType     element;     // used when effectType == ElementalResist
    public float           value;       // meaning depends on effectType
}
