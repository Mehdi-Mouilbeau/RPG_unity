# Save/Load Consumables Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persister les consommables de l'inventaire dans SaveData afin qu'ils soient sauvegardés et rechargés entre les sessions.

**Architecture:** Trois modifications ciblées — ajout d'un champ `consumableKeys` dans `SaveData`, ajout d'un tableau `consumables` + méthode `GetConsumable` dans `GameDataRegistry`, et deux boucles (save/load) dans `GameSession`. Aucun nouveau fichier. Pattern identique à celui des équipements (`equippedItemKeys`).

**Tech Stack:** Unity 6, C#, JsonUtility (sérialisation), NUnit (EditMode tests)

**Spec:** `docs/superpowers/specs/2026-03-23-save-consumables-design.md`

---

## File Map

| Fichier | Action |
|---------|--------|
| `My project/Assets/Scripts/Campaign/SaveData.cs` | Ajouter `List<string> consumableKeys` |
| `My project/Assets/Scripts/Data/GameDataRegistry.cs` | Ajouter `ConsumableSO[] consumables` + `GetConsumable(key)` |
| `My project/Assets/Scripts/Campaign/GameSession.cs` | `Save()` : sérialiser ; `Load()` : désérialiser |
| `My project/Assets/Tests/EditMode/SaveConsumablesTests.cs` | Nouveaux tests EditMode |

---

## Task 1 : SaveData + GameDataRegistry + tests

**Files:**
- Modify: `My project/Assets/Scripts/Campaign/SaveData.cs`
- Modify: `My project/Assets/Scripts/Data/GameDataRegistry.cs`
- Create: `My project/Assets/Tests/EditMode/SaveConsumablesTests.cs`

- [ ] **Écrire les tests en premier** — créer `My project/Assets/Tests/EditMode/SaveConsumablesTests.cs` :

```csharp
using NUnit.Framework;
using System.IO;

public class SaveConsumablesTests
{
    [TearDown]
    public void TearDown()
    {
        // Garantit que OverridePath est réinitialisé même si un test échoue en cours de route
        SaveSystem.OverridePath = null;
    }

    // ── SaveData ──────────────────────────────────────────────────────────

    [Test]
    public void SaveData_ConsumableKeys_DefaultEmpty()
    {
        var data = new SaveData();
        Assert.IsNotNull(data.consumableKeys);
        Assert.AreEqual(0, data.consumableKeys.Count);
    }

    // ── GameDataRegistry ──────────────────────────────────────────────────

    [Test]
    public void GameDataRegistry_GetConsumable_ReturnsMatch()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        var potion   = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion de Soin";
        registry.consumables = new[] { potion };

        var result = registry.GetConsumable("Potion de Soin");

        Assert.AreEqual(potion, result);
        UnityEngine.Object.DestroyImmediate(potion);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    [Test]
    public void GameDataRegistry_GetConsumable_UnknownKey_ReturnsNull()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        registry.consumables = System.Array.Empty<ConsumableSO>();

        var result = registry.GetConsumable("Inexistant");

        Assert.IsNull(result);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    [Test]
    public void GameDataRegistry_GetConsumable_NullKey_ReturnsNull()
    {
        var registry = UnityEngine.ScriptableObject.CreateInstance<GameDataRegistry>();
        registry.consumables = System.Array.Empty<ConsumableSO>();

        var result = registry.GetConsumable(null);

        Assert.IsNull(result);
        UnityEngine.Object.DestroyImmediate(registry);
    }

    // ── SaveSystem round-trip ─────────────────────────────────────────────

    [Test]
    public void SaveData_ConsumableKeys_RoundTrip()
    {
        var data = new SaveData();
        data.consumableKeys.Add("Potion de Soin");
        data.consumableKeys.Add("Antidote");

        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "rpg_test_consumables.json");
        SaveSystem.Save(data);
        var loaded = SaveSystem.Load();
        SaveSystem.Delete();
        SaveSystem.OverridePath = null;

        Assert.AreEqual(2, loaded.consumableKeys.Count);
        Assert.AreEqual("Potion de Soin", loaded.consumableKeys[0]);
        Assert.AreEqual("Antidote",       loaded.consumableKeys[1]);
    }
}
```

- [ ] **Lancer les tests** — Window > General > Test Runner > EditMode > Run All
  - Résultat attendu : **FAIL** — `SaveData` n'a pas encore `consumableKeys`, `GameDataRegistry` n'a pas encore `GetConsumable`

- [ ] **Ajouter `consumableKeys` dans `SaveData.cs`** — après `equippedItemKeys` :

```csharp
public List<string> consumableKeys = new List<string>();
```

- [ ] **Ajouter `consumables` + `GetConsumable` dans `GameDataRegistry.cs`** — après le bloc `[Header("Companions")]` :

```csharp
[Header("Consumables")]
public ConsumableSO[] consumables;

public ConsumableSO GetConsumable(string key)
{
    if (consumables == null || key == null) return null;
    foreach (var c in consumables)
        if (c != null && c.itemName == key) return c;
    return null;
}
```

- [ ] **Lancer les tests** — résultat attendu : tous les nouveaux tests **PASS**

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Campaign/SaveData.cs" \
        "My project/Assets/Scripts/Data/GameDataRegistry.cs" \
        "My project/Assets/Tests/EditMode/SaveConsumablesTests.cs" \
        "My project/Assets/Tests/EditMode/SaveConsumablesTests.cs.meta"
git commit -m "feat: add consumableKeys to SaveData and GetConsumable to GameDataRegistry"
```

---

## Task 2 : GameSession — Save() et Load()

**Files:**
- Modify: `My project/Assets/Scripts/Campaign/GameSession.cs`

- [ ] **Mettre à jour `Save()`** — dans `GameSession.cs`, après la boucle `equippedItemKeys` (avant `SaveSystem.Save(data)`) :

```csharp
foreach (var c in ActiveCharacter.Inventory.Consumables)
    if (c != null) data.consumableKeys.Add(c.itemName);
```

Le bloc `Save()` complet devient :

```csharp
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

    SaveSystem.Save(data);
}
```

- [ ] **Mettre à jour `Load()`** — dans `GameSession.cs`, après la boucle `equippedItemKeys` (avant `SetActiveCharacter(character)`) :

```csharp
foreach (var key in data.consumableKeys)
{
    var consumable = registry.GetConsumable(key);
    if (consumable != null) character.Inventory.AddConsumable(consumable);
}
```

Le bloc `Load()` complet devient :

```csharp
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
}
```

- [ ] **Vérifier compilation** dans Unity — aucune erreur.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Campaign/GameSession.cs"
git commit -m "feat: GameSession saves and loads consumables from inventory"
```

---

## Task 3 : Setup Unity Editor (manuel)

- [ ] **Vérifier que les assets consommables existent** dans `Assets/_Data/Consumables/` :
  - Si les assets `PotionDeSoin`, `Antidote`, `Ether` existent déjà (créés lors du Plan 7) → passer à l'étape suivante
  - Si absents : clic droit dans Project sur `Assets/_Data` → Create Folder `Consumables`, puis clic droit → Create > RPG > Consumable pour chacun :
    - `PotionDeSoin` : itemName = "Potion de Soin", effectType = HealHP, value = 0.3
    - `Antidote` : itemName = "Antidote", effectType = CureStatus, value = 0
    - `Ether` : itemName = "Ether", effectType = RestoreMP, value = 20

- [ ] **Ajouter les ConsumableSO dans `GameDataRegistry`** :
  - Dans la fenêtre Project, sélectionner `Assets/Resources/GameDataRegistry`
  - Dans l'Inspector, dans le tableau **Consumables**, assigner les 3 assets :
    - `PotionDeSoin`
    - `Antidote`
    - `Ether`

- [ ] **Vérifier en Play** :
  - Lancer un combat via la campagne
  - Ajouter un consommable au joueur via code temporaire ou via le débogueur
  - Sauvegarder → recharger → vérifier que le consommable est présent dans l'inventaire
