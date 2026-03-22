using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton persistant entre les scènes.
/// Toutes les scènes accèdent aux données de session via GameSession.Instance.
/// </summary>
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public CharacterData      ActiveCharacter  { get; private set; }
    public ProgressionFlags   Flags            { get; } = new ProgressionFlags();
    public int                Gold             { get; set; }
    public CampaignEncounter  PendingEncounter { get; set; } // rencontre en attente (combat)

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

        // Restore HP/MP
        int hpDiff = character.CurrentHP - data.currentHP;
        if (hpDiff > 0) character.TakeDamage(hpDiff);

        int mpDiff = character.CurrentMP - data.currentMP;
        if (mpDiff > 0) character.SpendMP(mpDiff);

        if (companionSO != null) character.AssignCompanion(companionSO);

        Gold = data.gold;
        Flags.LoadFrom(data.flags);

        // Équipements
        foreach (var itemKey in data.equippedItemKeys)
        {
            var eq = registry.GetEquipment(itemKey);
            if (eq != null) character.Inventory.Equip(eq);
        }

        SetActiveCharacter(character);
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
