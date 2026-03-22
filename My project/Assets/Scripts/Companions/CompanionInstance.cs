using System.Collections.Generic;

public class CompanionInstance
{
    public CompanionSO Definition { get; }
    // Keyed by object reference to avoid name-collision bugs between two skills with the same name.
    private readonly Dictionary<CompanionSkillSO, int> _cooldowns = new Dictionary<CompanionSkillSO, int>();

    public CompanionInstance(CompanionSO definition)
    {
        Definition = definition;
    }

    public bool CanUse(CompanionSkillSO skill)
    {
        if (skill == null) return false;
        return GetCooldown(skill) <= 0;
    }

    public void Use(CompanionSkillSO skill)
    {
        if (!CanUse(skill)) return;
        _cooldowns[skill] = skill.cooldownTurns;
    }

    public int GetCooldown(CompanionSkillSO skill)
    {
        if (skill == null) return 0;
        return _cooldowns.TryGetValue(skill, out int cd) ? cd : 0;
    }

    public void TickCooldowns()
    {
        var keys = new List<CompanionSkillSO>(_cooldowns.Keys);
        foreach (var key in keys)
            if (_cooldowns[key] > 0) _cooldowns[key]--;
    }
}
