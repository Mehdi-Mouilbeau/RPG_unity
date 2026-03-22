using UnityEngine;

public static class ActionResolver
{
    public static ActionResult ResolveSkill(CharacterData source, CharacterData target, SkillSO skill)
    {
        var result = new ActionResult { Source = source, Target = target, Skill = skill };

        if (source.CurrentMP < skill.mpCost)
        {
            result.Description = $"{source.CharacterName} n'a pas assez de MP !";
            return result;
        }
        source.SpendMP(skill.mpCost);

        switch (skill.damageType)
        {
            case SkillDamageType.Physical:
                int rawDmg = DamageCalculator.CalculatePhysical(source, target,
                    powerMultiplier: skill.powerMultiplier, wasCritical: out bool physCrit);
                int finalDmg = StatusManager.AbsorbWithShield(target, rawDmg);
                target.TakeDamage(finalDmg);
                result.DamageDealt = finalDmg;
                result.WasCritical = physCrit;
                result.Description = $"{source.CharacterName} utilise {skill.skillName} sur {target.CharacterName} pour {finalDmg} dégâts{(physCrit ? " (CRITIQUE !)" : "")}.";
                break;

            case SkillDamageType.Magical:
                int magDmg = DamageCalculator.CalculateMagical(source, target, skill.element,
                    powerMultiplier: skill.powerMultiplier, wasCritical: out bool magCrit);
                int finalMagDmg = StatusManager.AbsorbWithShield(target, magDmg);
                target.TakeDamage(finalMagDmg);
                result.DamageDealt = finalMagDmg;
                result.WasCritical = magCrit;
                result.ElementalModifier = ElementSystem.GetModifier(skill.element, target.ElementalAffinity);
                result.Description = $"{source.CharacterName} lance {skill.skillName} sur {target.CharacterName} pour {finalMagDmg} dégâts{(magCrit ? " (CRITIQUE !)" : "")}.";
                break;

            case SkillDamageType.Healing:
                int healAmt = DamageCalculator.CalculateHealing(source, skill.powerMultiplier);
                target.Heal(healAmt);
                result.HealingDone = healAmt;
                result.Description = $"{source.CharacterName} soigne {target.CharacterName} de {healAmt} HP.";
                break;

            case SkillDamageType.Status:
                if (Random.value < skill.statusChance)
                {
                    float shieldValue = skill.statusEffect == StatusEffectType.Shield
                        ? target.MaxHP * 0.20f
                        : 0f;
                    var effect = new StatusEffect(skill.statusEffect, StatusManager.GetDefaultDuration(skill.statusEffect), shieldValue);
                    StatusManager.Apply(target, effect);
                    result.AppliedStatus = effect;
                    result.Description = $"{target.CharacterName} est affecté par {skill.statusEffect} !";
                }
                else
                {
                    result.Description = $"{skill.skillName} sur {target.CharacterName} : raté !";
                }
                break;
        }

        result.TargetDied = target.IsDead;
        EventBus.Publish(new ActionResolvedEvent { Result = result });
        return result;
    }

    public static ActionResult ResolveBasicAttack(CharacterData source, CharacterData target)
    {
        var result = new ActionResult { Source = source, Target = target };
        int rawDmg = DamageCalculator.CalculatePhysical(source, target, wasCritical: out bool isCrit);
        int finalDmg = StatusManager.AbsorbWithShield(target, rawDmg);
        target.TakeDamage(finalDmg);
        result.DamageDealt = finalDmg;
        result.WasCritical = isCrit;
        result.TargetDied = target.IsDead;
        result.Description = $"{source.CharacterName} attaque {target.CharacterName} pour {finalDmg} dégâts{(isCrit ? " (CRITIQUE !)" : "")}.";
        EventBus.Publish(new ActionResolvedEvent { Result = result });
        return result;
    }

}
