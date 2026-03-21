using UnityEngine;

public static class DamageCalculator
{
    public static int CalculatePhysical(CharacterData attacker, CharacterData defender,
                                        bool critOverride = false, float powerMultiplier = 1f)
    {
        float raw = attacker.ATK * 2f - defender.DEF;
        raw = Mathf.Max(1f, raw);
        bool isCrit = critOverride || Random.value < (attacker.LCK / 400f);
        float critMod = isCrit ? 1.5f : 1f;
        return Mathf.RoundToInt(raw * powerMultiplier * critMod);
    }

    public static int CalculateMagical(CharacterData attacker, CharacterData defender,
                                       ElementType skillElement, bool critOverride = false,
                                       float powerMultiplier = 1f)
    {
        float raw = attacker.MAG * 2f - defender.RES;
        raw = Mathf.Max(1f, raw);
        float elemMod = ElementSystem.GetModifier(skillElement, defender.ElementalAffinity);
        bool isCrit = critOverride || Random.value < (attacker.LCK / 400f);
        float critMod = isCrit ? 1.5f : 1f;
        return Mathf.RoundToInt(raw * powerMultiplier * elemMod * critMod);
    }

    public static int CalculateHealing(CharacterData caster, float multiplier = 1f)
    {
        return Mathf.RoundToInt(caster.MAG * multiplier);
    }
}
