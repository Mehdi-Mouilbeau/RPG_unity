using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton persistant entre les scènes.
/// Toutes les scènes accèdent aux données de session via GameSession.Instance.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public CharacterData        ActiveCharacter  { get; private set; }
    public List<CharacterData>  Party            { get; private set; } = new();
    public ProgressionFlags     Flags            { get; } = new ProgressionFlags();
    public int                  Gold             { get; set; }
    public CampaignEncounter    PendingEncounter { get; set; } // rencontre en attente (combat)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActiveCharacter(CharacterData character)
    {
        ActiveCharacter = character;
    }

    public void SetParty(List<CharacterData> party)
    {
        Party = party ?? new List<CharacterData>();
    }

    // ── Sauvegarde ─────────────────────────────────────────────────────────

    public void Save()
    {
        if (ActiveCharacter == null) return;

        var data = new SaveData
        {
            characterName = ActiveCharacter.CharacterName,
            classKey      = ActiveCharacter.Class?.className ?? "",
            raceKey       = ActiveCharacter.Race?.raceName ?? "",
            level         = ActiveCharacter.Level,
            experience    = ActiveCharacter.Experience,
            currentHP     = ActiveCharacter.CurrentHP,
            currentMP     = ActiveCharacter.CurrentMP,
            gold          = Gold,
            companionKey  = ActiveCharacter.Companion?.Definition?.companionName ?? "",
        };
        data.flags = Flags.GetAllAsList();

        foreach (var kvp in ActiveCharacter.Inventory.Equipped)
        {
            if (kvp.Value != null) data.equippedItemKeys.Add(kvp.Value.itemName);
        }

        foreach (var c in ActiveCharacter.Inventory.Consumables)
            if (c != null) data.consumableKeys.Add(c.itemName);

        foreach (var member in Party)
        {
            data.party.Add(new PartyMemberSaveData
            {
                characterName = member.CharacterName,
                classKey      = member.Class?.className ?? "",
                raceKey       = member.Race?.raceName  ?? "",
                level         = member.Level,
                experience    = member.Experience,
                currentHP     = member.CurrentHP,
                currentMP     = member.CurrentMP,
            });
        }

        SaveSystem.Save(data);
    }

    public void Load()
    {
        var data = SaveSystem.Load();
        if (data == null) return;

        var registry = GameDataRegistry.Instance;
        if (registry == null) { Debug.LogError("GameDataRegistry introuvable dans Resources/"); return; }

        var classSO     = registry.GetClass(data.classKey);
        var raceSO      = registry.GetRace(data.raceKey);
        var companionSO = registry.GetCompanion(data.companionKey);

        if (classSO == null || raceSO == null)
        {
            Debug.LogError($"Save load failed: class '{data.classKey}' or race '{data.raceKey}' not found in registry.");
            return;
        }

        var character = new CharacterData();
        character.InitializeFromSO(data.characterName, classSO, raceSO, data.level);

        character.ApplyLoadedStats(data.currentHP, data.currentMP, data.experience);

        if (companionSO != null) character.AssignCompanion(companionSO);

        Gold = data.gold;
        Flags.LoadFrom(data.flags);

        // Équipements
        foreach (var itemKey in data.equippedItemKeys)
        {
            var eq = registry.GetEquipment(itemKey);
            if (eq != null) character.Inventory.Equip(eq);
        }

        foreach (var key in data.consumableKeys)
        {
            var consumable = registry.GetConsumable(key);
            if (consumable != null) character.Inventory.AddConsumable(consumable);
        }

        SetActiveCharacter(character);

        // Charger l'équipe de combat
        var loadedParty = new List<CharacterData>();
        foreach (var memberData in data.party)
        {
            var mClass = registry.GetClass(memberData.classKey);
            var mRace  = registry.GetRace(memberData.raceKey);
            if (mClass == null || mRace == null) continue;

            var member = new CharacterData();
            member.InitializeFromSO(memberData.characterName, mClass, mRace, memberData.level);
            member.ApplyLoadedStats(memberData.currentHP, memberData.currentMP, memberData.experience);
            loadedParty.Add(member);
        }
        SetParty(loadedParty);
    }

    // ── Personnages prédéfinis ──────────────────────────────────────────────

    public static CharacterData CreatePredefinedCharacter(string name, ClassSO classSO, RaceSO raceSO, int level, CompanionSO companionSO)
    {
        var character = new CharacterData();
        character.InitializeFromSO(name, classSO, raceSO, level);
        if (companionSO != null) character.AssignCompanion(companionSO);
        return character;
    }
}
