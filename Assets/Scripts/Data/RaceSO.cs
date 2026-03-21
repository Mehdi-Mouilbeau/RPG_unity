using UnityEngine;

[CreateAssetMenu(fileName = "NewRace", menuName = "RPG/Race")]
public class RaceSO : ScriptableObject
{
    [Header("Identité")]
    public string raceName;
    [TextArea] public string description;

    [Header("Modificateurs de stats (%)")]
    [Range(-0.5f, 0.5f)] public float hpModifier;
    [Range(-0.5f, 0.5f)] public float mpModifier;
    [Range(-0.5f, 0.5f)] public float atkModifier;
    [Range(-0.5f, 0.5f)] public float defModifier;
    [Range(-0.5f, 0.5f)] public float magModifier;
    [Range(-0.5f, 0.5f)] public float resModifier;
    [Range(-0.5f, 0.5f)] public float agiModifier;
    [Range(-0.5f, 0.5f)] public float lckModifier;

    [Header("Affinité élémentaire")]
    public ElementType elementalAffinity;

    [Header("Immunités aux statuts")]
    public StatusEffectType[] statusImmunities;

    [Header("Bonus passif")]
    public string passiveDescription;
}
