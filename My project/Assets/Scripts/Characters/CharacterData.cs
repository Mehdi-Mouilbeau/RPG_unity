using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string CharacterName { get; private set; }

    public int MaxHP { get; private set; }
    public int MaxMP { get; private set; }
    public int ATK { get; private set; }
    public int DEF { get; private set; }
    public int MAG { get; private set; }
    public int RES { get; private set; }
    public int AGI { get; private set; }
    public int LCK { get; private set; }

    public int CurrentHP { get; private set; }
    public int CurrentMP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    public ClassSO Class { get; private set; }
    public RaceSO Race { get; private set; }
    public int Level { get; private set; } = 1;
    public ElementType ElementalAffinity { get; private set; }

    public List<StatusEffect> ActiveStatuses { get; } = new();
    public List<SkillSO> Skills { get; } = new();
    public Dictionary<string, int> Cooldowns { get; } = new();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Test/debug helpers — stripped from release builds
    private HashSet<StatusEffectType> _testImmunities = new();
    public void SetImmunity_TestOnly(StatusEffectType type) => _testImmunities.Add(type);
    public ElementType ElementalAffinity_TestOnly { set => ElementalAffinity = value; }
#else
    private HashSet<StatusEffectType> _testImmunities = new();
#endif

    public void Initialize(string name, int hp, int mp, int atk, int def,
                           int mag, int res, int agi, int lck)
    {
        CharacterName = name;
        MaxHP = hp; MaxMP = mp;
        ATK = atk; DEF = def;
        MAG = mag; RES = res;
        AGI = agi; LCK = lck;
        CurrentHP = hp;
        CurrentMP = mp;
    }

    public void InitializeFromSO(string name, ClassSO classSO, RaceSO raceSO, int level = 1)
    {
        CharacterName = name;
        Class = classSO;
        Race = raceSO;
        Level = level;

        int hp  = Mathf.RoundToInt((classSO.baseHP  + classSO.hpGrowth  * (level - 1)) * (1 + raceSO.hpModifier));
        int mp  = Mathf.RoundToInt((classSO.baseMP  + classSO.mpGrowth  * (level - 1)) * (1 + raceSO.mpModifier));
        int atk = Mathf.RoundToInt((classSO.baseATK + classSO.atkGrowth * (level - 1)) * (1 + raceSO.atkModifier));
        int def = Mathf.RoundToInt((classSO.baseDEF + classSO.defGrowth * (level - 1)) * (1 + raceSO.defModifier));
        int mag = Mathf.RoundToInt((classSO.baseMAG + classSO.magGrowth * (level - 1)) * (1 + raceSO.magModifier));
        int res = Mathf.RoundToInt((classSO.baseRES + classSO.resGrowth * (level - 1)) * (1 + raceSO.resModifier));
        int agi = Mathf.RoundToInt((classSO.baseAGI + classSO.agiGrowth * (level - 1)) * (1 + raceSO.agiModifier));
        int lck = Mathf.RoundToInt((classSO.baseLCK + classSO.lckGrowth * (level - 1)) * (1 + raceSO.lckModifier));

        Initialize(name, hp, mp, atk, def, mag, res, agi, lck);

        ElementalAffinity = raceSO.elementalAffinity != ElementType.None
            ? raceSO.elementalAffinity
            : classSO.elementalAffinity;

        if (classSO.startingSkills != null)
            Skills.AddRange(classSO.startingSkills);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = System.Math.Max(0, CurrentHP - amount);
        if (IsDead) EventBus.Publish(new CharacterDiedEvent { Character = this });
    }

    public void Heal(int amount)
    {
        CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
    }

    public void SpendMP(int amount)
    {
        CurrentMP = System.Math.Max(0, CurrentMP - amount);
    }

    public void RestoreMP(int amount)
    {
        CurrentMP = System.Math.Min(MaxMP, CurrentMP + amount);
    }

    public int GetCooldown(SkillSO skill) =>
        Cooldowns.TryGetValue(skill.skillName, out int cd) ? cd : 0;

    public bool HasStatus(StatusEffectType type) =>
        ActiveStatuses.Exists(s => s.type == type);

    public bool IsImmuneToStatus(StatusEffectType type)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testImmunities.Contains(type)) return true;
#endif
        return Race != null && System.Array.Exists(Race.statusImmunities, s => s == type);
    }
}
