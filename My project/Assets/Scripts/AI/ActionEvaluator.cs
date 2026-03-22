using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActionEvaluator
{
    /// <summary>Easy: cible aléatoire vivante, skill aléatoire 30% du temps.</summary>
    public static BotAction EvaluateEasy(CharacterData actor,
        List<CharacterData> allies, List<CharacterData> enemies)
    {
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
}
