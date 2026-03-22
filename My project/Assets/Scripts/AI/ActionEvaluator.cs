using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActionEvaluator
{
    /// <summary>Easy: cible aléatoire vivante, skill aléatoire 30% du temps.</summary>
    /// <param name="allies">Non utilisé en Easy — présent pour cohérence de signature avec EvaluateNormal.</param>
    public static BotAction EvaluateEasy(CharacterData actor,
        List<CharacterData> allies, List<CharacterData> enemies)
    {
        if (actor == null) return null;

        var alive = enemies.Where(e => !e.IsDead).ToList();
        if (alive.Count == 0) return null;

        var target = alive[Random.Range(0, alive.Count)];

        var usable = actor.Skills
            .Where(s => s != null
                && actor.CurrentMP >= s.mpCost
                && actor.GetCooldown(s) == 0)
            .ToList();

        if (usable.Count > 0 && Random.value < 0.30f)
            return new BotAction(target, usable[Random.Range(0, usable.Count)]);

        return new BotAction(target);
    }

    /// <summary>Normal: soigne allié < 30% HP, exploite élémentaire, cible HP le plus bas.</summary>
    public static BotAction EvaluateNormal(CharacterData actor,
        List<CharacterData> allies, List<CharacterData> enemies)
    {
        if (actor == null) return null;

        var aliveEnemies = enemies.Where(e => !e.IsDead).ToList();
        var aliveAllies  = allies.Where(a => !a.IsDead).ToList();
        if (aliveEnemies.Count == 0) return null;

        var usable = actor.Skills
            .Where(s => s != null
                && actor.CurrentMP >= s.mpCost
                && actor.GetCooldown(s) == 0)
            .ToList();

        // Priority 1: heal ally < 30% HP
        var dyingAlly = aliveAllies
            .FirstOrDefault(a => a != actor && (float)a.CurrentHP / a.MaxHP < 0.30f);
        if (dyingAlly != null)
        {
            var heal = usable.FirstOrDefault(s => s.damageType == SkillDamageType.Healing);
            if (heal != null) return new BotAction(dyingAlly, heal);
        }

        // Priority 2: exploit elemental weakness
        foreach (var skill in usable.Where(s => s.damageType == SkillDamageType.Magical))
        {
            var weakTarget = aliveEnemies.FirstOrDefault(e =>
                ElementSystem.GetModifier(skill.element, e.ElementalAffinity) > 1f);
            if (weakTarget != null) return new BotAction(weakTarget, skill);
        }

        // Priority 3: use strongest offensive skill on lowest HP enemy
        var lowestHP = aliveEnemies.OrderBy(e => e.CurrentHP).First();
        var offensiveSkill = usable
            .Where(s => s.damageType == SkillDamageType.Physical || s.damageType == SkillDamageType.Magical)
            .OrderByDescending(s => s.powerMultiplier)
            .FirstOrDefault(s => s.powerMultiplier > 1f);
        if (offensiveSkill != null) return new BotAction(lowestHP, offensiveSkill);

        // Priority 4: basic attack fallback
        return new BotAction(lowestHP);
    }
}
