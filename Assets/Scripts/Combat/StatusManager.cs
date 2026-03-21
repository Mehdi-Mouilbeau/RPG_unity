using UnityEngine;
using System.Collections.Generic;

public static class StatusManager
{
    public static void Apply(CharacterData target, StatusEffect effect)
    {
        if (target.IsImmuneToStatus(effect.type)) return;
        if (effect.type == StatusEffectType.Shield && target.HasStatus(StatusEffectType.Shield))
            return;
        target.ActiveStatuses.Add(effect);
        EventBus.Publish(new StatusAppliedEvent { Target = target, Status = effect });
    }

    public static void Tick(CharacterData character)
    {
        var toRemove = new List<StatusEffect>();
        foreach (var status in character.ActiveStatuses)
        {
            switch (status.type)
            {
                case StatusEffectType.Burn:
                case StatusEffectType.Poison:
                    int dmg = Mathf.Max(1, Mathf.RoundToInt(character.MaxHP * 0.05f));
                    character.TakeDamage(dmg);
                    break;
            }
            status.remainingTurns--;
            if (status.remainingTurns <= 0)
                toRemove.Add(status);
        }
        foreach (var s in toRemove)
            character.ActiveStatuses.Remove(s);
    }

    public static bool CanAct(CharacterData character)
    {
        if (character.HasStatus(StatusEffectType.Freeze)) return false;
        if (character.HasStatus(StatusEffectType.Paralysis))
            return Random.value > 0.5f;
        return true;
    }

    public static int AbsorbWithShield(CharacterData target, int incomingDamage)
    {
        var shield = target.ActiveStatuses.Find(s => s.type == StatusEffectType.Shield);
        if (shield == null) return incomingDamage;
        if (shield.value >= incomingDamage)
        {
            shield.value -= incomingDamage;
            if (shield.value <= 0) target.ActiveStatuses.Remove(shield);
            return 0;
        }
        else
        {
            int remaining = incomingDamage - (int)shield.value;
            target.ActiveStatuses.Remove(shield);
            return remaining;
        }
    }
}
