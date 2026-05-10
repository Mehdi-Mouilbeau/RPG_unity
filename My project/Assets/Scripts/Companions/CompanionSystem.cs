using System.Collections.Generic;

public static class CompanionSystem
{
    public static CompanionActionResult Execute(
        CompanionInstance companion,
        CompanionSkillSO skill,
        CharacterData user,
        CharacterData primaryTarget,
        CharacterData[] allies,
        CharacterData[] enemies)
    {
        if (companion == null || skill == null)
            return new CompanionActionResult { Success = false, Message = "Compagnon ou compétence invalide." };

        if (!companion.CanUse(skill))
            return new CompanionActionResult { Success = false, Message = $"{skill.skillName} est en cooldown." };

        var result = new CompanionActionResult { Skill = skill };

        switch (skill.effectType)
        {
            case CompanionEffectType.DirectDamage:
                ApplyDirectDamage(skill, primaryTarget, enemies, result);
                break;
            case CompanionEffectType.Heal:
                ApplyHeal(skill, primaryTarget, allies, result);
                break;
            case CompanionEffectType.RemoveStatuses:
                ApplyRemoveStatuses(skill, primaryTarget, allies, result);
                break;
            case CompanionEffectType.BoostAgi:
                if (primaryTarget != null && !primaryTarget.IsDead)
                {
                    primaryTarget.AddAGIBonus(skill.value);
                    result.AffectedTargets = new[] { primaryTarget };
                    result.Message = $"AGI de {primaryTarget.CharacterName} augmentée de {skill.value} !";
                }
                else
                {
                    result.AffectedTargets = new CharacterData[0];
                    result.Message = "Aucune cible valide.";
                }
                break;

            case CompanionEffectType.RevealInfo:
                if (primaryTarget != null)
                {
                    string affinity = primaryTarget.ElementalAffinity == ElementType.None
                        ? "Neutre"
                        : primaryTarget.ElementalAffinity.ToString();
                    result.AffectedTargets = new[] { primaryTarget };
                    result.Message = $"{primaryTarget.CharacterName} — Affinité : {affinity}. HP : {primaryTarget.CurrentHP}/{primaryTarget.MaxHP}.";
                }
                else
                {
                    result.AffectedTargets = new CharacterData[0];
                    result.Message = "Aucune cible à analyser.";
                }
                break;

            default:
                result.AffectedTargets = new CharacterData[0];
                result.Message = $"{skill.skillName} activé.";
                break;
        }

        companion.Use(skill);
        result.Success = true;
        EventBus.Publish(new CompanionActivatedEvent
        {
            Owner   = user,
            Skill   = skill,
            Target  = primaryTarget,
            Message = result.Message
        });
        return result;
    }

    private static void ApplyDirectDamage(CompanionSkillSO skill, CharacterData primary,
        CharacterData[] enemies, CompanionActionResult result)
    {
        var candidates = skill.targetType == CompanionTargetType.AllEnemies ? enemies : new[] { primary };
        var affected = new List<CharacterData>();
        int total = 0;
        foreach (var t in candidates)
        {
            if (t == null || t.IsDead) continue;
            t.TakeDamage(skill.value);
            total += skill.value;
            affected.Add(t);
        }
        result.TotalValue = total;
        result.AffectedTargets = affected.ToArray();
        result.Message = $"Inflige {skill.value} dégâts.";
    }

    private static void ApplyHeal(CompanionSkillSO skill, CharacterData primary,
        CharacterData[] allies, CompanionActionResult result)
    {
        var candidates = skill.targetType == CompanionTargetType.AllAllies ? allies : new[] { primary };
        var affected = new List<CharacterData>();
        int total = 0;
        foreach (var t in candidates)
        {
            if (t == null || t.IsDead) continue;
            t.Heal(skill.value);
            total += skill.value;
            affected.Add(t);
        }
        result.TotalValue = total;
        result.AffectedTargets = affected.ToArray();
        result.Message = $"Soigne {skill.value} HP.";
    }

    private static void ApplyRemoveStatuses(CompanionSkillSO skill, CharacterData primary,
        CharacterData[] allies, CompanionActionResult result)
    {
        var candidates = skill.targetType == CompanionTargetType.AllAllies ? allies : new[] { primary };
        var affected = new List<CharacterData>();
        foreach (var t in candidates)
        {
            if (t == null || t.IsDead) continue;
            t.ActiveStatuses.RemoveAll(s => s.type != StatusEffectType.Shield);
            affected.Add(t);
        }
        result.AffectedTargets = affected.ToArray();
        result.Message = "Supprime les statuts négatifs.";
    }
}
