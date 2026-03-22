public enum ElementType { None, Fire, Nature, Lightning, Water, Light, Dark }
public enum StatusEffectType { None, Burn, Poison, Freeze, Paralysis, Confusion, Shield }
public enum EquipmentSlot { MainWeapon, Offhand, Helmet, Armor, Boots, Ring1, Ring2 }
public enum SkillTargetType { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
public enum SkillDamageType { Physical, Magical, Healing, Status }
public enum ClassRole { DPS, Tank, Healer, Support, Summoner }

public enum EquipmentRarity { Common, Uncommon, Rare, Epic, Legendary }

public enum EquipmentEffectType
{
    None,
    StatBoost,       // flat stat increase (stored in value)
    DamageOnHit,     // deal extra damage on physical hit
    HealOnKill,      // restore % HP on kill (value = 0–1 fraction)
    ElementalResist, // reduce elemental damage of given element (value = 0–1 fraction)
    MpRegenPerTurn,  // restore flat MP each turn
    CritBoost,       // increase crit chance (value = 0–1 fraction added to base)
}

public enum SkillBranch { Common, SpecA, SpecB }
