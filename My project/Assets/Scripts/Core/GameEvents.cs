public struct TurnStartedEvent    { public CharacterData Character; }
public struct TurnEndedEvent      { public CharacterData Character; }
public struct ActionResolvedEvent { public ActionResult Result; }
public struct CharacterDiedEvent  { public CharacterData Character; }
public struct BattleEndedEvent    { public bool PlayerWon; }
public struct StatusAppliedEvent  { public CharacterData Target; public StatusEffect Status; }
public struct PlayerTurnEvent     { public int PlayerIndex; public CharacterData Character; }
public struct ItemEquippedEvent   { public CharacterData Owner; public EquipmentSO Item; }
public struct LevelUpEvent { public CharacterData Character; public int NewLevel; public int SkillPointsGained; }

public struct CompanionActivatedEvent { public CharacterData Owner; public CompanionSkillSO Skill; public CharacterData Target; }

public struct BossDefeatedEvent  { public EnemySO Boss; public CharacterData Player; }
public struct BossPhaseEvent     { public int Phase; public CharacterData Boss; }
public struct ZoneEnteredEvent   { public CampaignZoneSO Zone; }
