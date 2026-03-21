public struct TurnStartedEvent    { public CharacterData Character; }
public struct TurnEndedEvent      { public CharacterData Character; }
public struct ActionResolvedEvent { public ActionResult Result; }
public struct CharacterDiedEvent  { public CharacterData Character; }
public struct BattleEndedEvent    { public bool PlayerWon; }
public struct StatusAppliedEvent  { public CharacterData Target; public StatusEffect Status; }
