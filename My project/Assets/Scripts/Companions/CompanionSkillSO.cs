using UnityEngine;

[CreateAssetMenu(fileName = "NewCompanionSkill", menuName = "RPG/Companion/Skill")]
public class CompanionSkillSO : ScriptableObject
{
    [Header("Identité")]
    public string skillName;
    [TextArea] public string description;

    [Header("Effet")]
    public CompanionEffectType effectType;
    public CompanionTargetType targetType;
    public int value;

    [Header("Cooldown")]
    [Range(3, 6)] public int cooldownTurns = 3;
}
