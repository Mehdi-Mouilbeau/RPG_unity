using UnityEngine;

public static class DamageCalculator
{
    public static int CalculatePhysical(CharacterData attacker, CharacterData defender,
                                        bool critOverride = false, float powerMultiplier = 1f,
                                        out bool wasCritical)
    {
        float critChance = System.Math.Min(attacker.LCK / 400f, 0.5f);
        bool isCrit = critOverride || Random.value < critChance;
        wasCritical = isCrit;
        float raw = attacker.ATK * 2f - defender.DEF;
        raw = UnityEngine.Mathf.Max(1f, raw);
        float critMod = isCrit ? 1.5f : 1f;
        return UnityEngine.Mathf.RoundToInt(raw * powerMultiplier * critMod);
    }

    public static int CalculateMagical(CharacterData attacker, CharacterData defender,
                                       ElementType skillElement, bool critOverride = false,
                                       float powerMultiplier = 1f, out bool wasCritical)
    {
        float critChance = System.Math.Min(attacker.LCK / 400f, 0.5f);
        bool isCrit = critOverride || Random.value < critChance;
        wasCritical = isCrit;
        float raw = attacker.MAG * 2f - defender.RES;
        raw = UnityEngine.Mathf.Max(1f, raw);
        float elemMod = ElementSystem.GetModifier(skillElement, defender.ElementalAffinity);
        float critMod = isCrit ? 1.5f : 1f;
        return UnityEngine.Mathf.RoundToInt(raw * powerMultiplier * elemMod * critMod);
    }

    public static int CalculateHealing(CharacterData caster, float multiplier = 1f)
    {
        return UnityEngine.Mathf.RoundToInt(caster.MAG * multiplier);
    }
}
