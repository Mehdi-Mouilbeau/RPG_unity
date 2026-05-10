using UnityEngine;

[CreateAssetMenu(fileName = "NewClass", menuName = "RPG/Class")]
public class ClassSO : ScriptableObject
{
    [Header("Identité")]
    public string className;
    public ClassRole role;
    [TextArea] public string description;

    [Header("Stats de base (niveau 1)")]
    public int baseHP = 100;
    public int baseMP = 50;
    public int baseATK = 10;
    public int baseDEF = 10;
    public int baseMAG = 10;
    public int baseRES = 10;
    public int baseAGI = 10;
    public int baseLCK = 5;

    [Header("Croissance par niveau")]
    public int hpGrowth = 15;
    public int mpGrowth = 8;
    public int atkGrowth = 2;
    public int defGrowth = 2;
    public int magGrowth = 2;
    public int resGrowth = 2;
    public int agiGrowth = 1;
    public int lckGrowth = 1;

    [Header("Affinité élémentaire naturelle")]
    public ElementType elementalAffinity;

    [Header("Skills de départ")]
    public SkillSO[] startingSkills;

    [Header("Arbre de compétences")]
    public SkillTreeSO skillTree;

    [Header("Races compatibles (vide = toutes les races)")]
    public RaceSO[] allowedRaces;
}
