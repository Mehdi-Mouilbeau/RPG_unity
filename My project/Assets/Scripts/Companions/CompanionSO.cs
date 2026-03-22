using UnityEngine;

[CreateAssetMenu(fileName = "NewCompanion", menuName = "RPG/Companion")]
public class CompanionSO : ScriptableObject
{
    [Header("Identité")]
    public string companionName;
    [TextArea] public string description;
    public CompanionType type;

    [Header("Compétences (3 à 5)")]
    public CompanionSkillSO[] skills = System.Array.Empty<CompanionSkillSO>();
}
