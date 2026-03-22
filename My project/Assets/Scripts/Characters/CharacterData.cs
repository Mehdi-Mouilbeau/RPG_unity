using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string CharacterName { get; private set; }

    // ── Base stats (set by Initialize; before equipment bonuses) ──────────
    private int _baseMaxHP, _baseMaxMP;
    private int _baseATK, _baseDEF, _baseMAG, _baseRES, _baseAGI, _baseLCK;

    // ── Effective stats = base + equipment bonus ──────────────────────────
    public int MaxHP => _baseMaxHP + Inventory.GetTotalHP();
    public int MaxMP => _baseMaxMP + Inventory.GetTotalMP();
    public int ATK   => _baseATK   + Inventory.GetTotalATK();
    public int DEF   => _baseDEF   + Inventory.GetTotalDEF();
    public int MAG   => _baseMAG   + Inventory.GetTotalMAG();
    public int RES   => _baseRES   + Inventory.GetTotalRES();
    public int AGI   => _baseAGI   + Inventory.GetTotalAGI();
    public int LCK   => _baseLCK   + Inventory.GetTotalLCK();

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

    // ── Inventory — lazy-initialized, always non-null after first access ──
    private Inventory _inventory;
    public Inventory Inventory => _inventory ??= new Inventory(this);

    // ── Progression ───────────────────────────────────────────────────────
    public int Experience { get; private set; }
    public int SkillPoints { get; private set; }

    private SkillTreeState _skillTreeState;
    public SkillTreeState SkillTree => _skillTreeState;

    public void InitSkillTree(SkillTreeSO tree)
    {
        _skillTreeState = new SkillTreeState(tree);
    }

    public void GainXP(int amount)
    {
        if (amount <= 0) return;
        if (XPSystem.IsMaxLevel(Level)) return;

        int oldLevel = Level;
        Experience += amount;
        int newLevel = XPSystem.GetLevel(Experience);
        if (newLevel > oldLevel)
        {
            int gained = newLevel - oldLevel;
            Level = newLevel;
            SkillPoints += gained;
            EventBus.Publish(new LevelUpEvent { Character = this, NewLevel = Level, SkillPointsGained = gained });
        }
    }

    public bool SpendSkillPoint(string nodeId)
    {
        if (SkillPoints <= 0) return false;
        if (_skillTreeState == null) return false;
        if (!_skillTreeState.CanUnlock(nodeId, Level)) return false;

        bool ok = _skillTreeState.Unlock(nodeId, Level);
        if (ok) SkillPoints--;
        return ok;
    }

    /// <summary>Resets skill tree and refunds points. Returns false if gold is insufficient.</summary>
    public bool ResetSkillTree(int gold, out int goldCost)
    {
        if (_skillTreeState == null) { goldCost = 0; return false; }
        goldCost = _skillTreeState.GetResetCost(Level);
        if (gold < goldCost) return false;

        int refund = _skillTreeState.GetUnlockedCount();
        _skillTreeState.Reset();
        SkillPoints += refund;
        return true;
    }

    // ── Compagnon ─────────────────────────────────────────────────────────
    private CompanionInstance _companion;
    public CompanionInstance Companion => _companion;

    public void AssignCompanion(CompanionSO companionSO)
    {
        _companion = companionSO != null ? new CompanionInstance(companionSO) : null;
    }

    public CompanionActionResult UseCompanionSkill(
        CompanionSkillSO skill,
        CharacterData primaryTarget,
        CharacterData[] allies,
        CharacterData[] enemies)
    {
        if (_companion == null)
            return new CompanionActionResult { Success = false, Message = "Aucun compagnon assigné." };
        return CompanionSystem.Execute(_companion, skill, this, primaryTarget, allies, enemies);
    }

    public void TickCompanionCooldowns()
    {
        _companion?.TickCooldowns();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private HashSet<StatusEffectType> _testImmunities = new();
    public void SetImmunity_TestOnly(StatusEffectType type) => _testImmunities.Add(type);
    public ElementType ElementalAffinity_TestOnly { set => ElementalAffinity = value; }
#endif

    public void Initialize(string name, int hp, int mp, int atk, int def,
                           int mag, int res, int agi, int lck)
    {
        CharacterName = name;
        _baseMaxHP = hp; _baseMaxMP = mp;
        _baseATK = atk; _baseDEF = def;
        _baseMAG = mag; _baseRES = res;
        _baseAGI = agi; _baseLCK = lck;
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
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

        if (classSO.skillTree != null)
            InitSkillTree(classSO.skillTree);
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
