using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "RPG/Consumable")]
public class ConsumableSO : ScriptableObject
{
    [Header("Identité")]
    public string itemName;
    [TextArea] public string description;

    [Header("Effet")]
    public ConsumableEffectType effectType;
    [Tooltip("Pour HealHP : fraction de MaxHP (0.3 = 30%). Pour RestoreMP : valeur fixe.")]
    public float value;
}
