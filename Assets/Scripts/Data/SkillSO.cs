using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Identité")]
    public string skillName;
    [TextArea] public string description;

    [Header("Type")]
    public SkillDamageType damageType;
    public SkillTargetType targetType;
    public ElementType element;

    [Header("Coût & Cooldown")]
    public int mpCost;
    public int cooldownTurns;

    [Header("Puissance")]
    [Tooltip("Multiplicateur appliqué à ATK ou MAG. Ex: 1.5 = 150% de la stat")]
    public float powerMultiplier = 1f;

    [Header("Effets de statut")]
    public StatusEffectType statusEffect;
    [Range(0f, 1f)] public float statusChance;
}
