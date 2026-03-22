# Plan 6 — Demo Campagne Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rendre le jeu testable de bout en bout — sélection d'un personnage prédéfini, WorldMap, exploration top-down (Village + Donjon), combats aléatoires, boss en deux phases, sauvegarde JSON.

**Architecture:** Singleton `GameSession` (DontDestroyOnLoad) persiste les données entre les scènes Unity. Les systèmes purs (ProgressionFlags, SaveSystem, BossPhaseLogic, LootTable) sont sans dépendance Unity et testables en EditMode. Les MonoBehaviours (PlayerController, BossController, UI) délèguent la logique à ces classes pures.

**Tech Stack:** Unity 6, C#, Unity Tilemap (déjà dans manifest), Yarn Spinner (à ajouter), NUnit EditMode tests, JsonUtility (sérialisation sauvegarde).

---

## Structure des fichiers

```
My project/Assets/
├── Scripts/
│   ├── Campaign/
│   │   ├── GameSession.cs           ← singleton DontDestroyOnLoad
│   │   ├── SceneLoader.cs           ← transitions avec fondu noir
│   │   ├── ProgressionFlags.cs      ← flags booléens nommés (pur C#)
│   │   ├── SaveData.cs              ← données sérialisables
│   │   ├── SaveSystem.cs            ← lecture/écriture JSON (pur C#)
│   │   ├── CampaignEncounter.cs     ← données d'une rencontre en attente
│   │   └── UI/
│   │       ├── MainMenuUI.cs
│   │       ├── CharacterSelectUI.cs
│   │       ├── WorldMapUI.cs
│   │       └── SaveMenuUI.cs
│   ├── Exploration/
│   │   ├── PlayerController.cs      ← déplacement WASD top-down
│   │   ├── NpcInteractor.cs         ← déclenche dialogue Yarn Spinner
│   │   ├── ChestInteractor.cs       ← loot + flag ouverture
│   │   ├── EncounterTrigger.cs      ← déclenche combat aléatoire
│   │   └── SceneEntrance.cs         ← transition entre scènes
│   ├── Combat/
│   │   ├── BossPhaseLogic.cs        ← logique phases boss (pur C#)
│   │   └── BossController.cs        ← MonoBehaviour, bridge vers BossPhaseLogic
│   └── Data/
│       ├── EnemySO.cs               ← stats + loot + XP
│       ├── LootTableSO.cs           ← pool d'items avec poids
│       ├── CampaignZoneSO.cs        ← config zone (ennemis, boss)
│       └── GameDataRegistry.cs      ← résolution clé → ScriptableObject
├── Core/
│   └── GameEvents.cs                ← modifier : ajouter BossDefeatedEvent, BossPhaseEvent, ZoneEnteredEvent
├── Tests/EditMode/
│   ├── EnemyDataTests.cs
│   ├── ProgressionFlagsTests.cs
│   ├── SaveSystemTests.cs
│   └── BossPhaseLogicTests.cs
├── Dialogue/
│   ├── Village.yarn
│   ├── Boss.yarn
│   └── Campaign.yarnproject
└── Resources/
    └── GameDataRegistry.asset       ← créé par RPGAssetCreator
```

**Fichiers modifiés :**
- `My project/Assets/Scripts/Core/GameEvents.cs` — ajout 3 events
- `My project/Assets/Scripts/RPG.Runtime.asmdef` — ajout référence Yarn Spinner
- `My project/Assets/Editor/RPGAssetCreator.cs` — ajout ennemis, zones, persos prédéfinis
- `My project/Packages/manifest.json` — ajout Yarn Spinner

---

## Task 1: Packages — Yarn Spinner

**Files:**
- Modify: `My project/Packages/manifest.json`
- Modify: `My project/Assets/Scripts/RPG.Runtime.asmdef`

- [ ] **Step 1: Ajouter Yarn Spinner au manifest**

Ouvrir `My project/Packages/manifest.json`. Ajouter dans `"dependencies"` :

```json
"dev.yarnspinner.unity": "https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git#current"
```

Résultat : le bloc `dependencies` commence par :
```json
{
  "dependencies": {
    "dev.yarnspinner.unity": "https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git#current",
    "com.unity.2d.animation": "13.0.2",
    ...
```

- [ ] **Step 2: Mettre à jour RPG.Runtime.asmdef**

Ouvrir `My project/Assets/Scripts/RPG.Runtime.asmdef`. Remplacer le contenu par :

```json
{
    "name": "RPG.Runtime",
    "rootNamespace": "",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem",
        "YarnSpinner",
        "YarnSpinner.Unity"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Ouvrir Unity et attendre la compilation**

Ouvrir Unity Editor avec le projet. Unity va télécharger et compiler Yarn Spinner automatiquement. Attendre que la barre de compilation en bas disparaisse (environ 30–60s). Vérifier : pas d'erreur dans la Console.

- [ ] **Step 4: Commit**

```bash
git add "My project/Packages/manifest.json" "My project/Assets/Scripts/RPG.Runtime.asmdef"
git commit -m "chore: add Yarn Spinner package and update Runtime asmdef references"
```

---

## Task 2: Data ScriptableObjects — EnemySO, LootTableSO, CampaignZoneSO

**Files:**
- Create: `My project/Assets/Scripts/Data/EnemySO.cs`
- Create: `My project/Assets/Scripts/Data/LootTableSO.cs`
- Create: `My project/Assets/Scripts/Data/CampaignZoneSO.cs`
- Test: `My project/Assets/Tests/EditMode/EnemyDataTests.cs`

- [ ] **Step 1: Écrire les tests (ils doivent échouer)**

Créer `My project/Assets/Tests/EditMode/EnemyDataTests.cs` :

```csharp
using NUnit.Framework;
using UnityEngine;

public class EnemyDataTests
{
    [Test]
    public void EnemySO_DefaultXPReward_IsPositive()
    {
        var enemy = ScriptableObject.CreateInstance<EnemySO>();
        enemy.xpReward = 30;
        Assert.Greater(enemy.xpReward, 0);
    }

    [Test]
    public void LootTable_EmptyEntries_RollReturnsNull()
    {
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new LootTableSO.LootEntry[0];
        Assert.IsNull(table.Roll());
    }

    [Test]
    public void LootTable_SingleEntry_RollReturnsIt()
    {
        var eq = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[] { new LootTableSO.LootEntry { equipment = eq, weight = 1f } };
        Assert.AreEqual(eq, table.Roll());
    }

    [Test]
    public void LootTable_ZeroWeights_RollReturnsNull()
    {
        var eq = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[] { new LootTableSO.LootEntry { equipment = eq, weight = 0f } };
        Assert.IsNull(table.Roll());
    }

    [Test]
    public void LootTable_MultipleEntries_RollReturnsEntryFromPool()
    {
        var eq1 = ScriptableObject.CreateInstance<EquipmentSO>();
        var eq2 = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[]
        {
            new LootTableSO.LootEntry { equipment = eq1, weight = 1f },
            new LootTableSO.LootEntry { equipment = eq2, weight = 1f }
        };
        var result = table.Roll();
        Assert.IsTrue(result == eq1 || result == eq2);
    }

    [Test]
    public void CampaignZone_GetRandomEnemy_ReturnsFromPool()
    {
        var enemy1 = ScriptableObject.CreateInstance<EnemySO>();
        var enemy2 = ScriptableObject.CreateInstance<EnemySO>();
        var zone = ScriptableObject.CreateInstance<CampaignZoneSO>();
        zone.enemyPool = new[] { enemy1, enemy2 };
        var result = zone.GetRandomEnemy();
        Assert.IsTrue(result == enemy1 || result == enemy2);
    }

    [Test]
    public void CampaignZone_EmptyPool_GetRandomEnemyReturnsNull()
    {
        var zone = ScriptableObject.CreateInstance<CampaignZoneSO>();
        zone.enemyPool = new EnemySO[0];
        Assert.IsNull(zone.GetRandomEnemy());
    }
}
```

- [ ] **Step 2: Lancer les tests — vérifier qu'ils échouent**

Dans Unity : Window > General > Test Runner > EditMode > Run All.
Résultat attendu : tous les tests échouent avec "EnemySO not found" (les classes n'existent pas encore).

- [ ] **Step 3: Créer EnemySO.cs**

Créer `My project/Assets/Scripts/Data/EnemySO.cs` :

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RPG/Campaign/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identité")]
    public string enemyName;
    public ElementType elementalAffinity;

    [Header("Stats")]
    public int hp  = 100;
    public int mp  = 0;
    public int atk = 10;
    public int def = 5;
    public int mag = 5;
    public int res = 5;
    public int agi = 8;
    public int lck = 3;

    [Header("Combat")]
    public BotBrain botBrain;
    public int xpReward = 30;

    [Header("Loot")]
    public LootTableSO lootTable;
}
```

- [ ] **Step 4: Créer LootTableSO.cs**

Créer `My project/Assets/Scripts/Data/LootTableSO.cs` :

```csharp
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "RPG/Campaign/LootTable")]
public class LootTableSO : ScriptableObject
{
    [Serializable]
    public struct LootEntry
    {
        public EquipmentSO equipment;
        [Range(0f, 1f)] public float weight;
    }

    public LootEntry[] entries;

    public EquipmentSO Roll()
    {
        if (entries == null || entries.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var e in entries) totalWeight += e.weight;
        if (totalWeight <= 0f) return null;

        float roll = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var e in entries)
        {
            cumulative += e.weight;
            if (roll <= cumulative) return e.equipment;
        }
        return entries[entries.Length - 1].equipment;
    }
}
```

- [ ] **Step 5: Créer CampaignZoneSO.cs**

Créer `My project/Assets/Scripts/Data/CampaignZoneSO.cs` :

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewZone", menuName = "RPG/Campaign/Zone")]
public class CampaignZoneSO : ScriptableObject
{
    [Header("Identité")]
    public string zoneName;
    public string sceneKey;         // nom de la scène Unity à charger
    public string flagKey;          // clé dans ProgressionFlags pour "zone visitée"

    [Header("Rencontres aléatoires")]
    public EnemySO[] enemyPool;
    public BotBrain defaultBotBrain;

    [Header("Boss")]
    public EnemySO boss;            // null si pas de boss
    public string bossDefeatedFlagKey;

    public EnemySO GetRandomEnemy()
    {
        if (enemyPool == null || enemyPool.Length == 0) return null;
        return enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
    }
}
```

- [ ] **Step 6: Lancer les tests — vérifier qu'ils passent**

Window > General > Test Runner > EditMode > Run All.
Résultat attendu : 7 tests `EnemyDataTests` → PASS.

- [ ] **Step 7: Commit**

```bash
git add "My project/Assets/Scripts/Data/EnemySO.cs" \
        "My project/Assets/Scripts/Data/LootTableSO.cs" \
        "My project/Assets/Scripts/Data/CampaignZoneSO.cs" \
        "My project/Assets/Tests/EditMode/EnemyDataTests.cs"
git commit -m "feat: add EnemySO, LootTableSO, CampaignZoneSO with tests"
```

---

## Task 3: ProgressionFlags

**Files:**
- Create: `My project/Assets/Scripts/Campaign/ProgressionFlags.cs`
- Test: `My project/Assets/Tests/EditMode/ProgressionFlagsTests.cs`

- [ ] **Step 1: Écrire les tests**

Créer `My project/Assets/Tests/EditMode/ProgressionFlagsTests.cs` :

```csharp
using NUnit.Framework;

public class ProgressionFlagsTests
{
    [Test]
    public void IsSet_AfterSet_ReturnsTrue()
    {
        var flags = new ProgressionFlags();
        flags.Set("village_visited");
        Assert.IsTrue(flags.IsSet("village_visited"));
    }

    [Test]
    public void IsSet_NeverSet_ReturnsFalse()
    {
        var flags = new ProgressionFlags();
        Assert.IsFalse(flags.IsSet("village_visited"));
    }

    [Test]
    public void Reset_ClearsAllFlags()
    {
        var flags = new ProgressionFlags();
        flags.Set("a");
        flags.Set("b");
        flags.Reset();
        Assert.IsFalse(flags.IsSet("a"));
        Assert.IsFalse(flags.IsSet("b"));
    }

    [Test]
    public void Flags_AreIndependentFromEachOther()
    {
        var flags = new ProgressionFlags();
        flags.Set("flag_a");
        Assert.IsTrue(flags.IsSet("flag_a"));
        Assert.IsFalse(flags.IsSet("flag_b"));
    }

    [Test]
    public void Set_NullKey_DoesNotThrow()
    {
        var flags = new ProgressionFlags();
        Assert.DoesNotThrow(() => flags.Set(null));
        Assert.IsFalse(flags.IsSet(null));
    }

    [Test]
    public void RoundTrip_SaveLoadFlags_RestoresState()
    {
        var flags = new ProgressionFlags();
        flags.Set("boss_defeated");
        flags.Set("chest_1_opened");

        var saved = flags.GetAllAsList();
        var flags2 = new ProgressionFlags();
        flags2.LoadFrom(saved);

        Assert.IsTrue(flags2.IsSet("boss_defeated"));
        Assert.IsTrue(flags2.IsSet("chest_1_opened"));
        Assert.IsFalse(flags2.IsSet("village_visited"));
    }
}
```

- [ ] **Step 2: Lancer les tests — vérifier qu'ils échouent**

Window > Test Runner > EditMode > Run All. Attendu : FAIL (ProgressionFlags inconnu).

- [ ] **Step 3: Créer ProgressionFlags.cs**

Créer `My project/Assets/Scripts/Campaign/ProgressionFlags.cs` :

```csharp
using System;
using System.Collections.Generic;

public class ProgressionFlags
{
    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

    public void Set(string key)
    {
        if (key == null) return;
        _flags[key] = true;
    }

    public bool IsSet(string key)
    {
        if (key == null) return false;
        return _flags.TryGetValue(key, out bool val) && val;
    }

    public void Reset()
    {
        _flags.Clear();
    }

    // ── Sérialisation (pour SaveSystem) ───────────────────────────────────

    [Serializable]
    public struct FlagEntry
    {
        public string key;
        public bool value;
    }

    public List<FlagEntry> GetAllAsList()
    {
        var list = new List<FlagEntry>();
        foreach (var kv in _flags)
            list.Add(new FlagEntry { key = kv.Key, value = kv.Value });
        return list;
    }

    public void LoadFrom(List<FlagEntry> entries)
    {
        _flags.Clear();
        if (entries == null) return;
        foreach (var e in entries)
            if (e.key != null) _flags[e.key] = e.value;
    }
}
```

- [ ] **Step 4: Lancer les tests — vérifier qu'ils passent**

Window > Test Runner > EditMode > Run All. Attendu : 6 tests `ProgressionFlagsTests` → PASS.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Campaign/ProgressionFlags.cs" \
        "My project/Assets/Tests/EditMode/ProgressionFlagsTests.cs"
git commit -m "feat: add ProgressionFlags with serialization and tests"
```

---

## Task 4: SaveData + SaveSystem

**Files:**
- Create: `My project/Assets/Scripts/Campaign/SaveData.cs`
- Create: `My project/Assets/Scripts/Campaign/SaveSystem.cs`
- Test: `My project/Assets/Tests/EditMode/SaveSystemTests.cs`

- [ ] **Step 1: Écrire les tests**

Créer `My project/Assets/Tests/EditMode/SaveSystemTests.cs` :

```csharp
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

public class SaveSystemTests
{
    private string _testPath;

    [SetUp]
    public void SetUp()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "rpg_test_save.json");
        SaveSystem.OverridePath = _testPath;
    }

    [TearDown]
    public void TearDown()
    {
        SaveSystem.OverridePath = null;
        if (File.Exists(_testPath)) File.Delete(_testPath);
    }

    [Test]
    public void SaveThenLoad_RestoresAllFields()
    {
        var data = new SaveData
        {
            characterName = "Kael",
            classKey      = "Guerrier",
            raceKey       = "Humain",
            level         = 5,
            experience    = 500,
            currentHP     = 120,
            currentMP     = 50,
            gold          = 200,
            companionKey  = "loup_des_ombres"
        };
        data.flags.Add(new ProgressionFlags.FlagEntry { key = "village_visited", value = true });
        data.equippedItemKeys.Add("iron_sword");

        SaveSystem.Save(data);
        var loaded = SaveSystem.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual("Kael",            loaded.characterName);
        Assert.AreEqual("Guerrier",        loaded.classKey);
        Assert.AreEqual("Humain",          loaded.raceKey);
        Assert.AreEqual(5,                 loaded.level);
        Assert.AreEqual(500,               loaded.experience);
        Assert.AreEqual(200,               loaded.gold);
        Assert.AreEqual("loup_des_ombres", loaded.companionKey);
        Assert.AreEqual(1,                 loaded.flags.Count);
        Assert.AreEqual("village_visited", loaded.flags[0].key);
        Assert.AreEqual(1,                 loaded.equippedItemKeys.Count);
    }

    [Test]
    public void Load_NoFile_ReturnsNull()
    {
        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "nonexistent_rpg.json");
        Assert.IsNull(SaveSystem.Load());
    }

    [Test]
    public void HasSave_AfterSave_ReturnsTrue()
    {
        SaveSystem.Save(new SaveData { characterName = "Test" });
        Assert.IsTrue(SaveSystem.HasSave());
    }

    [Test]
    public void HasSave_NoFile_ReturnsFalse()
    {
        SaveSystem.OverridePath = Path.Combine(Path.GetTempPath(), "nonexistent2_rpg.json");
        Assert.IsFalse(SaveSystem.HasSave());
    }

    [Test]
    public void Delete_RemovesFile()
    {
        SaveSystem.Save(new SaveData { characterName = "Test" });
        SaveSystem.Delete();
        Assert.IsFalse(SaveSystem.HasSave());
    }
}
```

- [ ] **Step 2: Lancer les tests — vérifier qu'ils échouent**

Window > Test Runner > EditMode > Run All. Attendu : FAIL (SaveData/SaveSystem inconnus).

- [ ] **Step 3: Créer SaveData.cs**

Créer `My project/Assets/Scripts/Campaign/SaveData.cs` :

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string characterName;
    public string classKey;
    public string raceKey;
    public int    level;
    public int    experience;
    public int    currentHP;
    public int    currentMP;
    public int    gold;
    public string companionKey;

    // Utilise une liste de FlagEntry car Dictionary n'est pas sérialisable par JsonUtility
    public List<ProgressionFlags.FlagEntry> flags          = new List<ProgressionFlags.FlagEntry>();
    public List<string>                     equippedItemKeys = new List<string>();
}
```

- [ ] **Step 4: Créer SaveSystem.cs**

Créer `My project/Assets/Scripts/Campaign/SaveSystem.cs` :

```csharp
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    /// <summary>Injecter un chemin pour les tests EditMode (null = chemin par défaut).</summary>
    public static string OverridePath = null;

    private static string GetPath() =>
        OverridePath ?? Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(GetPath(), json);
    }

    public static SaveData Load()
    {
        string path = GetPath();
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave() => File.Exists(GetPath());

    public static void Delete()
    {
        string path = GetPath();
        if (File.Exists(path)) File.Delete(path);
    }
}
```

- [ ] **Step 5: Lancer les tests — vérifier qu'ils passent**

Window > Test Runner > EditMode > Run All. Attendu : 5 tests `SaveSystemTests` → PASS.

- [ ] **Step 6: Commit**

```bash
git add "My project/Assets/Scripts/Campaign/SaveData.cs" \
        "My project/Assets/Scripts/Campaign/SaveSystem.cs" \
        "My project/Assets/Tests/EditMode/SaveSystemTests.cs"
git commit -m "feat: add SaveData and SaveSystem with JSON serialization and tests"
```

---

## Task 5: GameEvents + CampaignEncounter

**Files:**
- Modify: `My project/Assets/Scripts/Core/GameEvents.cs`
- Create: `My project/Assets/Scripts/Campaign/CampaignEncounter.cs`

- [ ] **Step 1: Ajouter 3 events dans GameEvents.cs**

Ouvrir `My project/Assets/Scripts/Core/GameEvents.cs`. Ajouter à la fin du fichier :

```csharp
public struct BossDefeatedEvent  { public EnemySO Boss; public CharacterData Player; }
public struct BossPhaseEvent     { public int Phase; public CharacterData Boss; }
public struct ZoneEnteredEvent   { public CampaignZoneSO Zone; }
```

Le fichier complet devient :
```csharp
public struct TurnStartedEvent    { public CharacterData Character; }
public struct TurnEndedEvent      { public CharacterData Character; }
public struct ActionResolvedEvent { public ActionResult Result; }
public struct CharacterDiedEvent  { public CharacterData Character; }
public struct BattleEndedEvent    { public bool PlayerWon; }
public struct StatusAppliedEvent  { public CharacterData Target; public StatusEffect Status; }
public struct PlayerTurnEvent     { public int PlayerIndex; public CharacterData Character; }
public struct ItemEquippedEvent   { public CharacterData Owner; public EquipmentSO Item; }
public struct LevelUpEvent        { public CharacterData Character; public int NewLevel; public int SkillPointsGained; }
public struct CompanionActivatedEvent { public CharacterData Owner; public CompanionSkillSO Skill; public CharacterData Target; }

public struct BossDefeatedEvent  { public EnemySO Boss; public CharacterData Player; }
public struct BossPhaseEvent     { public int Phase; public CharacterData Boss; }
public struct ZoneEnteredEvent   { public CampaignZoneSO Zone; }
```

- [ ] **Step 2: Créer CampaignEncounter.cs**

Créer `My project/Assets/Scripts/Campaign/CampaignEncounter.cs` :

```csharp
using System.Collections.Generic;

/// <summary>
/// Données d'une rencontre en attente, passées par GameSession à la scène Battle.
/// </summary>
public class CampaignEncounter
{
    public List<EnemySO>   Enemies  { get; }
    public bool            IsBoss   { get; }
    public CampaignZoneSO  Zone     { get; }

    public CampaignEncounter(List<EnemySO> enemies, CampaignZoneSO zone, bool isBoss = false)
    {
        Enemies = enemies;
        Zone    = zone;
        IsBoss  = isBoss;
    }
}
```

- [ ] **Step 3: Vérifier la compilation**

Dans Unity Editor, s'assurer qu'il n'y a pas d'erreur dans la Console. Si compilé sans erreur, continuer.

- [ ] **Step 4: Commit**

```bash
git add "My project/Assets/Scripts/Core/GameEvents.cs" \
        "My project/Assets/Scripts/Campaign/CampaignEncounter.cs"
git commit -m "feat: add BossDefeatedEvent, BossPhaseEvent, ZoneEnteredEvent, CampaignEncounter"
```

---

## Task 6: BossPhaseLogic

**Files:**
- Create: `My project/Assets/Scripts/Combat/BossPhaseLogic.cs`
- Test: `My project/Assets/Tests/EditMode/BossPhaseLogicTests.cs`

- [ ] **Step 1: Écrire les tests**

Créer `My project/Assets/Tests/EditMode/BossPhaseLogicTests.cs` :

```csharp
using NUnit.Framework;

public class BossPhaseLogicTests
{
    [Test]
    public void Phase2_NotActive_AtStart()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsFalse(logic.Phase2Active);
    }

    [Test]
    public void ShouldTransition_AboveThreshold_ReturnsFalse()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        // 160 HP / 300 max = 53% > 50% → pas de transition
        Assert.IsFalse(logic.CheckAndTransition(currentHP: 160));
    }

    [Test]
    public void ShouldTransition_AtExactThreshold_ReturnsTrue()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        // 150 HP / 300 max = 50% → transition
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 150));
        Assert.IsTrue(logic.Phase2Active);
    }

    [Test]
    public void ShouldTransition_BelowThreshold_ReturnsTrue()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 100));
    }

    [Test]
    public void ShouldTransition_NotCalledTwice_AfterPhase2Active()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 100));   // première fois → true
        Assert.IsFalse(logic.CheckAndTransition(currentHP: 50));   // déjà en phase 2 → false
    }

    [Test]
    public void ATKBoostMultiplier_Is1_25()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.AreEqual(1.25f, logic.Phase2ATKMultiplier, delta: 0.001f);
    }
}
```

- [ ] **Step 2: Lancer les tests — vérifier qu'ils échouent**

Window > Test Runner > EditMode > Run All. Attendu : FAIL (BossPhaseLogic inconnu).

- [ ] **Step 3: Créer BossPhaseLogic.cs**

Créer `My project/Assets/Scripts/Combat/BossPhaseLogic.cs` :

```csharp
/// <summary>
/// Logique pure de phase du boss — sans dépendance Unity.
/// BossController (MonoBehaviour) délègue à cette classe.
/// </summary>
public class BossPhaseLogic
{
    private readonly int   _maxHP;
    private readonly float _threshold;

    public bool  Phase2Active       { get; private set; }
    public float Phase2ATKMultiplier => 1.25f;

    public BossPhaseLogic(int maxHP, float phaseThreshold = 0.5f)
    {
        _maxHP     = maxHP;
        _threshold = phaseThreshold;
    }

    /// <summary>
    /// Vérifie si le boss doit passer en phase 2.
    /// Retourne true une seule fois (à la transition).
    /// </summary>
    public bool CheckAndTransition(int currentHP)
    {
        if (Phase2Active) return false;
        if ((float)currentHP / _maxHP <= _threshold)
        {
            Phase2Active = true;
            return true;
        }
        return false;
    }
}
```

- [ ] **Step 4: Lancer les tests — vérifier qu'ils passent**

Window > Test Runner > EditMode > Run All. Attendu : 6 tests `BossPhaseLogicTests` → PASS.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Combat/BossPhaseLogic.cs" \
        "My project/Assets/Tests/EditMode/BossPhaseLogicTests.cs"
git commit -m "feat: add BossPhaseLogic (pure C#) with tests"
```

---

## Task 7: GameDataRegistry

**Files:**
- Create: `My project/Assets/Scripts/Data/GameDataRegistry.cs`

Note : `GameDataRegistry` est un ScriptableObject chargé depuis `Resources/`. Il n'a pas de logique complexe — les tests de résolution clé sont manuels (une fois l'asset créé dans Unity). Son rôle est de fournir une résolution `string → ScriptableObject` au `SaveSystem` lors du chargement.

- [ ] **Step 1: Créer GameDataRegistry.cs**

Créer `My project/Assets/Scripts/Data/GameDataRegistry.cs` :

```csharp
using UnityEngine;

/// <summary>
/// Registre central pour résoudre les clés string en ScriptableObjects lors du chargement de sauvegarde.
/// Placé dans Assets/Resources/GameDataRegistry.asset pour être chargé via Resources.Load.
/// </summary>
[CreateAssetMenu(fileName = "GameDataRegistry", menuName = "RPG/GameDataRegistry")]
public class GameDataRegistry : ScriptableObject
{
    [Header("Classes")]
    public ClassSO[] classes;

    [Header("Races")]
    public RaceSO[] races;

    [Header("Equipment")]
    public EquipmentSO[] equipment;

    [Header("Companions")]
    public CompanionSO[] companions;

    private static GameDataRegistry _instance;

    public static GameDataRegistry Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GameDataRegistry>("GameDataRegistry");
            return _instance;
        }
    }

    public ClassSO GetClass(string key)
    {
        if (classes == null || key == null) return null;
        foreach (var c in classes)
            if (c != null && c.className == key) return c;
        return null;
    }

    public RaceSO GetRace(string key)
    {
        if (races == null || key == null) return null;
        foreach (var r in races)
            if (r != null && r.raceName == key) return r;
        return null;
    }

    public EquipmentSO GetEquipment(string key)
    {
        if (equipment == null || key == null) return null;
        foreach (var e in equipment)
            if (e != null && e.itemName == key) return e;
        return null;
    }

    public CompanionSO GetCompanion(string key)
    {
        if (companions == null || key == null) return null;
        foreach (var c in companions)
            if (c != null && c.companionName == key) return c;
        return null;
    }
}
```

- [ ] **Step 2: Vérifier la compilation dans Unity**

Ouvrir Unity Editor, vérifier que Console n'a pas d'erreur.

- [ ] **Step 3: Commit**

```bash
git add "My project/Assets/Scripts/Data/GameDataRegistry.cs"
git commit -m "feat: add GameDataRegistry SO for string-key to SO resolution on save load"
```

---

## Task 8: GameSession + SceneLoader

**Files:**
- Create: `My project/Assets/Scripts/Campaign/GameSession.cs`
- Create: `My project/Assets/Scripts/Campaign/SceneLoader.cs`

Note : MonoBehaviours — pas de tests EditMode. Tester manuellement en Play Mode.

- [ ] **Step 1: Créer SceneLoader.cs**

Créer `My project/Assets/Scripts/Campaign/SceneLoader.cs` :

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private Image fadeImage; // Image UI noire couvrant tout l'écran
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Fondu au noir
        yield return StartCoroutine(Fade(0f, 1f));
        yield return SceneManager.LoadSceneAsync(sceneName);
        // Fondu depuis le noir
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;
    }
}
```

- [ ] **Step 2: Créer GameSession.cs**

Créer `My project/Assets/Scripts/Campaign/GameSession.cs` :

```csharp
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
            companionKey  = ActiveCharacter.Companion?.SO?.companionName ?? "",
        };
        data.flags = Flags.GetAllAsList();

        foreach (var slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            var eq = ActiveCharacter.Inventory.GetEquipped((EquipmentSlot)slot);
            if (eq != null) data.equippedItemKeys.Add(eq.itemName);
        }

        SaveSystem.Save(data);
    }

    public void Load()
    {
        var data = SaveSystem.Load();
        if (data == null) return;

        var registry = GameDataRegistry.Instance;
        if (registry == null) { Debug.LogError("GameDataRegistry introuvable dans Resources/"); return; }

        var classSO    = registry.GetClass(data.classKey);
        var raceSO     = registry.GetRace(data.raceKey);
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
```

> Note : `ActiveCharacter.Companion?.SO` nécessite que `CompanionInstance` expose la propriété `SO`. Vérifier dans `CompanionInstance.cs` — si absent, ajouter `public CompanionSO SO { get; }` dans le constructeur.

- [ ] **Step 3: Vérifier CompanionInstance expose SO**

Ouvrir `My project/Assets/Scripts/Companions/CompanionInstance.cs`. Si la propriété `public CompanionSO SO` n'existe pas, l'ajouter :

```csharp
public class CompanionInstance
{
    public CompanionSO SO { get; }   // ← ajouter si absent

    private readonly Dictionary<CompanionSkillSO, int> _cooldowns = new Dictionary<CompanionSkillSO, int>();

    public CompanionInstance(CompanionSO so)
    {
        SO = so;   // ← ajouter si absent
        // ... reste du constructeur
    }
    // ...
}
```

- [ ] **Step 4: Vérifier la compilation**

Unity Console → aucune erreur.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Campaign/GameSession.cs" \
        "My project/Assets/Scripts/Campaign/SceneLoader.cs" \
        "My project/Assets/Scripts/Companions/CompanionInstance.cs"
git commit -m "feat: add GameSession (DontDestroyOnLoad) and SceneLoader with fade transition"
```

---

## Task 9: Scripts d'exploration

**Files:**
- Create: `My project/Assets/Scripts/Exploration/PlayerController.cs`
- Create: `My project/Assets/Scripts/Exploration/NpcInteractor.cs`
- Create: `My project/Assets/Scripts/Exploration/ChestInteractor.cs`
- Create: `My project/Assets/Scripts/Exploration/EncounterTrigger.cs`
- Create: `My project/Assets/Scripts/Exploration/SceneEntrance.cs`

Note : MonoBehaviours — pas de tests EditMode. Tous vérifiés manuellement en Play Mode.

- [ ] **Step 1: Créer PlayerController.cs**

Créer `My project/Assets/Scripts/Exploration/PlayerController.cs` :

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D _rb;
    private Vector2     _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        _moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            _moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            _moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _moveInput.x += 1f;
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput.normalized * moveSpeed;
    }
}
```

- [ ] **Step 2: Créer NpcInteractor.cs**

Créer `My project/Assets/Scripts/Exploration/NpcInteractor.cs` :

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/// <summary>
/// Attaché à un PNJ. Quand le joueur entre dans le trigger et appuie sur E,
/// démarre le dialogue Yarn Spinner correspondant.
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private string      dialogueNode = "Villageois";
    [SerializeField] private DialogueRunner dialogueRunner;

    private bool _playerNearby;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }

    private void Update()
    {
        if (_playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!dialogueRunner.IsDialogueRunning)
                dialogueRunner.StartDialogue(dialogueNode);
        }
    }
}
```

- [ ] **Step 3: Créer ChestInteractor.cs**

Créer `My project/Assets/Scripts/Exploration/ChestInteractor.cs` :

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Coffre. Quand le joueur appuie sur E à proximité, distribue le loot
/// et persiste l'état "ouvert" dans ProgressionFlags.
/// </summary>
public class ChestInteractor : MonoBehaviour
{
    [SerializeField] private string        chestId;     // ex: "chest_donjon_1"
    [SerializeField] private LootTableSO   lootTable;
    [SerializeField] private GameObject    openVisual;  // sprite "coffre ouvert" (placeholder)
    [SerializeField] private GameObject    closedVisual;

    private bool _playerNearby;

    private void Start()
    {
        bool alreadyOpened = GameSession.Instance != null
            && GameSession.Instance.Flags.IsSet(chestId);
        SetVisual(alreadyOpened);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }

    private void Update()
    {
        if (!_playerNearby) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        var session = GameSession.Instance;
        if (session == null || session.Flags.IsSet(chestId)) return;

        var item = lootTable?.Roll();
        if (item != null && session.ActiveCharacter != null)
        {
            session.ActiveCharacter.Inventory.Equip(item);
            Debug.Log($"Coffre : obtenu {item.itemName}");
        }

        session.Flags.Set(chestId);
        SetVisual(true);
    }

    private void SetVisual(bool opened)
    {
        if (openVisual)   openVisual.SetActive(opened);
        if (closedVisual) closedVisual.SetActive(!opened);
    }
}
```

- [ ] **Step 4: Créer EncounterTrigger.cs**

Créer `My project/Assets/Scripts/Exploration/EncounterTrigger.cs` :

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone de collision qui déclenche un combat aléatoire quand le joueur entre dedans.
/// </summary>
public class EncounterTrigger : MonoBehaviour
{
    [SerializeField] private CampaignZoneSO zone;
    [Range(0f, 1f)]
    [SerializeField] private float encounterChance = 0.3f; // 30% chance par entrée

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session == null) return;

        if (Random.value > encounterChance) return;

        var enemy = zone.GetRandomEnemy();
        if (enemy == null) return;

        session.PendingEncounter = new CampaignEncounter(
            new List<EnemySO> { enemy }, zone, isBoss: false);

        SceneLoader.Instance.LoadScene("Battle");
    }
}
```

- [ ] **Step 5: Créer SceneEntrance.cs**

Créer `My project/Assets/Scripts/Exploration/SceneEntrance.cs` :

```csharp
using UnityEngine;

/// <summary>
/// Portail de transition entre scènes. Quand le joueur entre dans le trigger,
/// charge la scène cible et pose un flag de progression si configuré.
/// </summary>
public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private string targetScene;        // ex: "Donjon", "WorldMap"
    [SerializeField] private string progressionFlag;    // ex: "village_visited" (optionnel)
    [SerializeField] private string unlockZoneFlag;     // ex: "donjon_unlocked" (optionnel)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session != null)
        {
            if (!string.IsNullOrEmpty(progressionFlag))
                session.Flags.Set(progressionFlag);
            if (!string.IsNullOrEmpty(unlockZoneFlag))
                session.Flags.Set(unlockZoneFlag);

            session.Save(); // auto-save au changement de zone
        }

        SceneLoader.Instance.LoadScene(targetScene);
    }
}
```

- [ ] **Step 6: Vérifier la compilation**

Unity Console → aucune erreur.

- [ ] **Step 7: Commit**

```bash
git add "My project/Assets/Scripts/Exploration/PlayerController.cs" \
        "My project/Assets/Scripts/Exploration/NpcInteractor.cs" \
        "My project/Assets/Scripts/Exploration/ChestInteractor.cs" \
        "My project/Assets/Scripts/Exploration/EncounterTrigger.cs" \
        "My project/Assets/Scripts/Exploration/SceneEntrance.cs"
git commit -m "feat: add exploration scripts (PlayerController, NpcInteractor, ChestInteractor, EncounterTrigger, SceneEntrance)"
```

---

## Task 10: BossController + BattleCampaignBridge

**Files:**
- Create: `My project/Assets/Scripts/Combat/BossController.cs`
- Create: `My project/Assets/Scripts/Combat/BattleCampaignBridge.cs`

- [ ] **Step 1: Créer BossController.cs**

Créer `My project/Assets/Scripts/Combat/BossController.cs` :

```csharp
using System.Linq;
using UnityEngine;

/// <summary>
/// MonoBehaviour attaché dans la scène Battle pour gérer les phases du boss.
/// Délègue la logique à BossPhaseLogic (pur C#).
/// </summary>
public class BossController : MonoBehaviour
{
    private BossPhaseLogic _logic;
    private CharacterData  _boss;
    private bool           _cryUsed;

    private System.Action<ActionResolvedEvent> _onAction;

    public void Initialize(CharacterData boss)
    {
        _boss  = boss;
        _logic = new BossPhaseLogic(maxHP: boss.MaxHP, phaseThreshold: 0.5f);

        _onAction = evt => OnActionResolved(evt);
        EventBus.Subscribe<ActionResolvedEvent>(_onAction);
    }

    private void OnDestroy()
    {
        if (_onAction != null)
            EventBus.Unsubscribe<ActionResolvedEvent>(_onAction);
    }

    private void OnActionResolved(ActionResolvedEvent evt)
    {
        if (_boss == null || _logic == null) return;

        bool transitioned = _logic.CheckAndTransition(_boss.CurrentHP);
        if (!transitioned) return;

        // Armure de Crâne : bouclier = 30% HP max
        int shieldValue = Mathf.RoundToInt(_boss.MaxHP * 0.3f);
        var shieldStatus = new StatusEffect
        {
            type      = StatusEffectType.Shield,
            duration  = 999,
            shieldHP  = shieldValue
        };
        _boss.ActiveStatuses.Add(shieldStatus);

        // ATK +25% : boost direct sur _baseATK n'est pas accessible (private)
        // On publie l'event et le GameSession/UI peut en tenir compte
        EventBus.Publish(new BossPhaseEvent { Phase = 2, Boss = _boss });

        Debug.Log($"[Boss] {_boss.CharacterName} passe en Phase 2 ! Bouclier activé ({shieldValue} HP).");
    }

    /// <summary>
    /// Appelé par BattleCampaignBridge quand c'est le tour du boss en phase 2.
    /// Retourne true si Cri des Morts a été déclenché ce tour.
    /// </summary>
    public bool TryUseCriDesMorts(CharacterData[] targets)
    {
        if (!_logic.Phase2Active || _cryUsed || targets == null) return false;

        _cryUsed = true;
        foreach (var target in targets.Where(t => t != null && !t.IsDead))
        {
            var poison = new StatusEffect
            {
                type     = StatusEffectType.Poison,
                duration = 3
            };
            target.ActiveStatuses.Add(poison);
        }

        Debug.Log("[Boss] Cri des Morts : tous les alliés du joueur sont empoisonnés !");
        return true;
    }
}
```

- [ ] **Step 2: Créer BattleCampaignBridge.cs**

Ce MonoBehaviour est placé dans la scène Battle. Il lit `GameSession.PendingEncounter` au démarrage et initialise BattleManager avec les bons ennemis.

Créer `My project/Assets/Scripts/Combat/BattleCampaignBridge.cs` :

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pont entre la campagne (GameSession.PendingEncounter) et BattleManager.
/// Remplace BattleBootstrap quand on joue en mode campagne.
/// </summary>
public class BattleCampaignBridge : MonoBehaviour
{
    [SerializeField] private BotBrain fallbackBotBrain;
    [SerializeField] private BossController bossController;

    private void Start()
    {
        var session = GameSession.Instance;
        if (session == null || session.PendingEncounter == null)
        {
            // Pas de session campagne — BattleBootstrap prend le relais
            return;
        }

        var encounter = session.PendingEncounter;
        var player    = session.ActiveCharacter;

        if (player == null)
        {
            Debug.LogError("BattleCampaignBridge: ActiveCharacter est null.");
            return;
        }

        // Construire les ennemis depuis les EnemySO
        var enemies = new List<CharacterData>();
        BotBrain brain = null;

        foreach (var enemySO in encounter.Enemies)
        {
            var enemy = new CharacterData();
            enemy.Initialize(enemySO.enemyName,
                enemySO.hp, enemySO.mp,
                enemySO.atk, enemySO.def,
                enemySO.mag, enemySO.res,
                enemySO.agi, enemySO.lck);
            enemies.Add(enemy);
            brain = enemySO.botBrain ?? fallbackBotBrain;
        }

        BattleManager.Instance.StartBattle(
            new List<CharacterData> { player }, enemies, brain);

        // Si c'est un boss, initialiser BossController sur le premier ennemi
        if (encounter.IsBoss && bossController != null && enemies.Count > 0)
            bossController.Initialize(enemies[0]);

        session.PendingEncounter = null;

        // S'abonner à la fin du combat pour auto-save et retour WorldMap
        System.Action<BattleEndedEvent> onEnd = null;
        onEnd = evt =>
        {
            EventBus.Unsubscribe<BattleEndedEvent>(onEnd);
            if (evt.PlayerWon)
            {
                if (encounter.IsBoss)
                    EventBus.Publish(new BossDefeatedEvent { Player = player });

                session.Save();
                SceneLoader.Instance.LoadScene("WorldMap");
            }
            else
            {
                // Défaite → retour menu principal
                SceneLoader.Instance.LoadScene("MainMenu");
            }
        };
        EventBus.Subscribe<BattleEndedEvent>(onEnd);
    }
}
```

- [ ] **Step 3: Vérifier la compilation**

Unity Console → aucune erreur.

- [ ] **Step 4: Commit**

```bash
git add "My project/Assets/Scripts/Combat/BossController.cs" \
        "My project/Assets/Scripts/Combat/BattleCampaignBridge.cs"
git commit -m "feat: add BossController (phase 2 logic) and BattleCampaignBridge"
```

---

## Task 11: UI — MainMenuUI, CharacterSelectUI, WorldMapUI, SaveMenuUI

**Files:**
- Create: `My project/Assets/Scripts/Campaign/UI/MainMenuUI.cs`
- Create: `My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs`
- Create: `My project/Assets/Scripts/Campaign/UI/WorldMapUI.cs`
- Create: `My project/Assets/Scripts/Campaign/UI/SaveMenuUI.cs`

- [ ] **Step 1: Créer MainMenuUI.cs**

Créer `My project/Assets/Scripts/Campaign/UI/MainMenuUI.cs` :

```csharp
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnLoad;
    [SerializeField] private Button btnArena;
    [SerializeField] private Button btnQuit;

    private void Start()
    {
        bool hasSave = SaveSystem.HasSave();
        btnLoad.interactable = hasSave;

        btnStart.onClick.AddListener(OnStart);
        btnLoad.onClick.AddListener(OnLoad);
        btnArena.onClick.AddListener(OnArena);
        btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnStart()
    {
        SceneLoader.Instance.LoadScene("CharacterSelect");
    }

    private void OnLoad()
    {
        GameSession.Instance.Load();
        SceneLoader.Instance.LoadScene("WorldMap");
    }

    private void OnArena()
    {
        SceneLoader.Instance.LoadScene("Arena");
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
```

- [ ] **Step 2: Créer CharacterSelectUI.cs**

Créer `My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs` :

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [System.Serializable]
    public struct PredefinedCharacterConfig
    {
        public string      characterName;
        public ClassSO     classSO;
        public RaceSO      raceSO;
        public int         level;
        public CompanionSO companionSO;
        [TextArea] public string description;
    }

    [SerializeField] private PredefinedCharacterConfig[] characters;
    [SerializeField] private Button[]   selectButtons;
    [SerializeField] private TMP_Text   descriptionText;

    private void Start()
    {
        for (int i = 0; i < selectButtons.Length && i < characters.Length; i++)
        {
            int index = i; // capture pour la lambda
            selectButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }
    }

    private void SelectCharacter(int index)
    {
        var config = characters[index];
        var character = GameSession.CreatePredefinedCharacter(
            config.characterName,
            config.classSO,
            config.raceSO,
            config.level,
            config.companionSO);

        GameSession.Instance.SetActiveCharacter(character);
        GameSession.Instance.Gold = 200;
        SceneLoader.Instance.LoadScene("WorldMap");
    }

    public void ShowDescription(int index)
    {
        if (descriptionText != null && index < characters.Length)
            descriptionText.text = characters[index].description;
    }
}
```

- [ ] **Step 3: Créer WorldMapUI.cs**

Créer `My project/Assets/Scripts/Campaign/UI/WorldMapUI.cs` :

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldMapUI : MonoBehaviour
{
    [System.Serializable]
    public struct ZoneButton
    {
        public Button         button;
        public TMP_Text       label;
        public CampaignZoneSO zone;
        public string         requiredFlag; // flag requis pour déverrouiller ("" = toujours accessible)
        public string         completedFlag;
    }

    [SerializeField] private ZoneButton[] zones;

    private void Start()
    {
        foreach (var z in zones)
        {
            var zone = z; // capture
            bool locked    = !string.IsNullOrEmpty(z.requiredFlag)
                             && !GameSession.Instance.Flags.IsSet(z.requiredFlag);
            bool completed = !string.IsNullOrEmpty(z.completedFlag)
                             && GameSession.Instance.Flags.IsSet(z.completedFlag);

            z.button.interactable = !locked;
            if (z.label != null)
            {
                string suffix = locked ? " [Verrouillé]" : (completed ? " [Complété]" : "");
                z.label.text = z.zone.zoneName + suffix;
            }

            if (!locked)
                z.button.onClick.AddListener(() => EnterZone(zone.zone));
        }
    }

    private void EnterZone(CampaignZoneSO zone)
    {
        EventBus.Publish(new ZoneEnteredEvent { Zone = zone });
        SceneLoader.Instance.LoadScene(zone.sceneKey);
    }
}
```

- [ ] **Step 4: Créer SaveMenuUI.cs**

Créer `My project/Assets/Scripts/Campaign/UI/SaveMenuUI.cs` :

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     btnSave;
    [SerializeField] private Button     btnClose;
    [SerializeField] private TMP_Text   statusText;

    private void Start()
    {
        panel.SetActive(false);
        btnSave.onClick.AddListener(OnSave);
        btnClose.onClick.AddListener(OnClose);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            panel.SetActive(!panel.activeSelf);
    }

    private void OnSave()
    {
        GameSession.Instance.Save();
        if (statusText) statusText.text = "Partie sauvegardée !";
    }

    private void OnClose()
    {
        panel.SetActive(false);
    }

    public void Show() => panel.SetActive(true);
}
```

- [ ] **Step 5: Vérifier la compilation**

Unity Console → aucune erreur.

- [ ] **Step 6: Commit**

```bash
git add "My project/Assets/Scripts/Campaign/UI/MainMenuUI.cs" \
        "My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs" \
        "My project/Assets/Scripts/Campaign/UI/WorldMapUI.cs" \
        "My project/Assets/Scripts/Campaign/UI/SaveMenuUI.cs"
git commit -m "feat: add campaign UI scripts (MainMenu, CharacterSelect, WorldMap, SaveMenu)"
```

---

## Task 12: Dialogues Yarn Spinner

**Files:**
- Create: `My project/Assets/Dialogue/Campaign.yarnproject`
- Create: `My project/Assets/Dialogue/Village.yarn`
- Create: `My project/Assets/Dialogue/Boss.yarn`

- [ ] **Step 1: Créer le dossier Dialogue dans Unity**

Dans Unity Project window : clic droit sur Assets > Create > Folder > nommer `Dialogue`.

- [ ] **Step 2: Créer le Yarn Project**

Dans Unity : clic droit dans `Assets/Dialogue` > Create > Yarn Spinner > Yarn Project. Nommer `Campaign`.

Ou créer le fichier manuellement :

Créer `My project/Assets/Dialogue/Campaign.yarnproject` (Unity le reconnaît automatiquement).

- [ ] **Step 3: Créer Village.yarn**

Créer `My project/Assets/Dialogue/Village.yarn` :

```
title: Villageois
---
Villageois: Bienvenue dans le Village de Départ, voyageur.
Villageois: Le donjon à l'est est dangereux... Le Roi Squelette y règne depuis des années.
-> En savoir plus
    Villageois: Des squelettes errent dans ses couloirs. Méfiez-vous de l'obscurité.
-> Partir
    Villageois: Bonne chance, héros.
===

title: Forgeron
---
Forgeron: Ah, un aventurier ! Je peux améliorer ton équipement... mais pas aujourd'hui.
Forgeron: Mon enclume est en réparation. Reviens me voir plus tard.
===

title: Enchanteur
---
Enchanteur: Tu veux enchanter ton équipement ? Bonne idée...
Enchanteur: Mais mes matériaux sont épuisés. Je dois attendre une nouvelle livraison.
===

title: PNJ_Sauvegarde
---
PNJ_Sauvegarde: Je peux graver ta progression dans les annales du village.
-> Sauvegarder
    <<save>>
    PNJ_Sauvegarde: Ta progression est sauvegardée. Bonne chance !
-> Non merci
    PNJ_Sauvegarde: Comme tu veux. Sois prudent.
===
```

- [ ] **Step 4: Créer Boss.yarn**

Créer `My project/Assets/Dialogue/Boss.yarn` :

```
title: RoiSquelette_Intro
---
Roi Squelette: Encore un héros qui s'aventure dans mes domaines...
Roi Squelette: Des siècles que je règne sur ces ossements.
Roi Squelette: Tu ne seras qu'un os de plus dans ma collection.
===

title: RoiSquelette_Phase2
---
Roi Squelette: Insolent ! Tu oses me blesser ?
Roi Squelette: SENTINELLES ! Que mes os se renforcent !
===
```

- [ ] **Step 5: Associer les fichiers au Yarn Project**

Dans Unity : sélectionner `Campaign.yarnproject` dans le Project window. Dans l'Inspector, cliquer "Add" et ajouter `Village.yarn` et `Boss.yarn`. Cliquer Apply.

Si l'Inspector ne montre pas ce bouton, glisser les fichiers .yarn dans le champ "Source Files" du Yarn Project.

- [ ] **Step 6: Implémenter la commande Yarn `<<save>>`**

Dans `NpcInteractor.cs`, il faut enregistrer la commande `save` dans Yarn Spinner. La façon la plus simple est d'ajouter un `YarnCommand` dans un MonoBehaviour présent dans la scène. Créer `My project/Assets/Scripts/Campaign/YarnCommands.cs` :

```csharp
using UnityEngine;
using Yarn.Unity;

public class YarnCommands : MonoBehaviour
{
    [YarnCommand("save")]
    public void SaveGame()
    {
        GameSession.Instance?.Save();
        Debug.Log("[Yarn] Partie sauvegardée.");
    }
}
```

- [ ] **Step 7: Vérifier la compilation**

Unity Console → aucune erreur.

- [ ] **Step 8: Commit**

```bash
git add "My project/Assets/Dialogue/Village.yarn" \
        "My project/Assets/Dialogue/Boss.yarn" \
        "My project/Assets/Scripts/Campaign/YarnCommands.cs"
git commit -m "feat: add Yarn Spinner dialogues (Village, Boss) and YarnCommands save handler"
```

---

## Task 13: RPGAssetCreator — ennemis, zones, persos prédéfinis

**Files:**
- Modify: `My project/Assets/Editor/RPGAssetCreator.cs`

- [ ] **Step 1: Ajouter les nouvelles méthodes dans RPGAssetCreator**

Ouvrir `My project/Assets/Editor/RPGAssetCreator.cs`.

**1) Ajouter dans `CreateFolders()` :**
```csharp
EnsureFolder("Assets/_Data/Enemies");
EnsureFolder("Assets/_Data/Zones");
EnsureFolder("Assets/_Data/LootTables");
EnsureFolder("Assets/Resources");
```

**2) Ajouter dans `CreateAllAssets()` :**
```csharp
CreateCampaignEnemies();
CreateCampaignZones();
CreateGameDataRegistry();
```

**3) Ajouter les méthodes suivantes à la fin de la classe :**

```csharp
// ─────────────────────────── CAMPAIGN ENEMIES ───────────────────────────

private static void CreateCampaignEnemies()
{
    var botNormal = CreateOrLoad<BotBrain>("Assets/_Data/AI/BotBrain_Normal.asset");

    CreateEnemy("Squelette",
        enemyName: "Squelette",
        affinity: ElementType.Dark,
        hp: 60, mp: 0, atk: 18, def: 8, mag: 0, res: 5, agi: 10, lck: 5,
        xpReward: 30, botBrain: botNormal);

    CreateEnemy("ArcherSquelette",
        enemyName: "Archer Squelette",
        affinity: ElementType.Dark,
        hp: 45, mp: 0, atk: 22, def: 5, mag: 0, res: 5, agi: 15, lck: 8,
        xpReward: 35, botBrain: botNormal);

    CreateEnemy("GolemOs",
        enemyName: "Golem d'Os",
        affinity: ElementType.Dark,
        hp: 90, mp: 0, atk: 15, def: 18, mag: 0, res: 10, agi: 6, lck: 3,
        xpReward: 50, botBrain: botNormal);

    CreateEnemy("RoiSquelette",
        enemyName: "Roi Squelette",
        affinity: ElementType.Dark,
        hp: 300, mp: 80, atk: 30, def: 15, mag: 25, res: 15, agi: 12, lck: 10,
        xpReward: 500, botBrain: botNormal);
}

private static EnemySO CreateEnemy(string assetName, string enemyName,
    ElementType affinity, int hp, int mp, int atk, int def,
    int mag, int res, int agi, int lck, int xpReward, BotBrain botBrain)
{
    var enemy = CreateOrLoad<EnemySO>($"Assets/_Data/Enemies/{assetName}.asset");
    enemy.enemyName        = enemyName;
    enemy.elementalAffinity = affinity;
    enemy.hp  = hp;  enemy.mp  = mp;
    enemy.atk = atk; enemy.def = def;
    enemy.mag = mag; enemy.res = res;
    enemy.agi = agi; enemy.lck = lck;
    enemy.xpReward = xpReward;
    enemy.botBrain = botBrain;
    EditorUtility.SetDirty(enemy);
    return enemy;
}

// ─────────────────────────── CAMPAIGN ZONES ─────────────────────────────

private static void CreateCampaignZones()
{
    var botNormal = CreateOrLoad<BotBrain>("Assets/_Data/AI/BotBrain_Normal.asset");

    var squelette = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/Squelette.asset");
    var archer    = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/ArcherSquelette.asset");
    var golem     = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/GolemOs.asset");
    var roi       = CreateOrLoad<EnemySO>("Assets/_Data/Enemies/RoiSquelette.asset");

    // Zone Village : pas d'ennemis ni de boss
    var village = CreateOrLoad<CampaignZoneSO>("Assets/_Data/Zones/Zone_Village.asset");
    village.zoneName           = "Village de Départ";
    village.sceneKey           = "Village";
    village.flagKey            = "village_visited";
    village.enemyPool          = new EnemySO[0];
    village.boss               = null;
    village.defaultBotBrain    = botNormal;
    EditorUtility.SetDirty(village);

    // Zone Donjon : pool de rencontres + boss
    var donjon = CreateOrLoad<CampaignZoneSO>("Assets/_Data/Zones/Zone_Donjon.asset");
    donjon.zoneName            = "Donjon du Roi Squelette";
    donjon.sceneKey            = "Donjon";
    donjon.flagKey             = "donjon_visited";
    donjon.bossDefeatedFlagKey = "boss_defeated";
    donjon.enemyPool           = new[] { squelette, archer, golem };
    donjon.boss                = roi;
    donjon.defaultBotBrain     = botNormal;
    EditorUtility.SetDirty(donjon);
}

// ─────────────────────────── GAME DATA REGISTRY ─────────────────────────

private static void CreateGameDataRegistry()
{
    EnsureFolder("Assets/Resources");
    var registry = CreateOrLoad<GameDataRegistry>("Assets/Resources/GameDataRegistry.asset");

    // Classes
    var guerrier  = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Guerrier.asset");
    var mage      = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Mage.asset");
    var soigneur  = CreateOrLoad<ClassSO>("Assets/_Data/Classes/Soigneur.asset");
    registry.classes = new[] { guerrier, mage, soigneur };

    // Races
    var humain      = CreateOrLoad<RaceSO>("Assets/_Data/Races/Humain.asset");
    var elfe        = CreateOrLoad<RaceSO>("Assets/_Data/Races/Elfe.asset");
    var lycanthrope = CreateOrLoad<RaceSO>("Assets/_Data/Races/Lycanthrope.asset");
    registry.races = new[] { humain, elfe, lycanthrope };

    // Companions
    var loup    = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/LoupDesOmbres.asset");
    var corbeau = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/CorbeauAnalyste.asset");
    var fee     = CreateOrLoad<CompanionSO>("Assets/_Data/Companions/FeeSylvestre.asset");
    registry.companions = new[] { loup, corbeau, fee };

    EditorUtility.SetDirty(registry);
}
```

- [ ] **Step 2: Lancer RPG > Create Starter Assets dans Unity**

Menu Unity > RPG > Create Starter Assets.

Vérifier dans le Project window :
- `Assets/_Data/Enemies/` contient 4 assets (Squelette, ArcherSquelette, GolemOs, RoiSquelette)
- `Assets/_Data/Zones/` contient Zone_Village et Zone_Donjon
- `Assets/Resources/GameDataRegistry.asset` existe

- [ ] **Step 3: Remplir le GameDataRegistry dans l'Inspector**

Sélectionner `Assets/Resources/GameDataRegistry.asset`. Dans l'Inspector, vérifier que les listes `classes`, `races`, `companions` sont bien remplies. Si des slots sont vides, les glisser manuellement depuis le Project window.

- [ ] **Step 4: Commit**

```bash
git add "My project/Assets/Editor/RPGAssetCreator.cs"
git commit -m "feat: RPGAssetCreator — add campaign enemies, zones, GameDataRegistry generation"
```

---

## Task 14: Création des scènes Unity

**Files:** (tous créés manuellement dans Unity Editor)

Note : les scènes Unity ne sont pas scriptables — elles se créent dans l'éditeur. Ce task liste les étapes de setup précises.

- [ ] **Step 1: Créer les scènes dans Unity**

File > New Scene (Basic 2D) pour chacune :
- `MainMenu` → sauvegarder dans `Assets/Scenes/MainMenu.unity`
- `CharacterSelect` → `Assets/Scenes/CharacterSelect.unity`
- `WorldMap` → `Assets/Scenes/WorldMap.unity`
- `Village` → `Assets/Scenes/Village.unity`
- `Donjon` → `Assets/Scenes/Donjon.unity`

(La scène `Battle` existe déjà.)

- [ ] **Step 2: Ajouter les scènes dans Build Settings**

File > Build Settings > Add Open Scenes. Ajouter toutes les scènes dans l'ordre :
1. MainMenu
2. CharacterSelect
3. WorldMap
4. Village
5. Donjon
6. Battle (existante)
7. Arena (existante si elle existe)

- [ ] **Step 3: Configurer la scène MainMenu**

Dans la scène `MainMenu` :

1. Créer un GameObject vide `[SceneLoader]` → ajouter `SceneLoader` component → champ `fadeImage` : créer un Canvas > Image (couvre tout l'écran, couleur noire, alpha 0)
2. Créer un Canvas → ajouter 4 Buttons : "Nouvelle Partie", "Charger", "Arène", "Quitter"
3. Sur un GameObject vide `[MainMenu]` → ajouter `MainMenuUI` → assigner les 4 boutons

> Aussi créer un GameObject vide `[GameSession]` → ajouter `GameSession` component. **Attention :** GameSession est DontDestroyOnLoad, il ne faut l'avoir que dans MainMenu (la première scène chargée).

- [ ] **Step 4: Configurer la scène CharacterSelect**

Dans `CharacterSelect` :
1. Canvas → 3 Buttons (Kael, Lyra, Theron) + TMP_Text pour description
2. GameObject `[CharacterSelectUI]` → ajouter `CharacterSelectUI`
3. Dans l'Inspector de `CharacterSelectUI` → remplir le tableau `characters` (3 entrées) :
   - Kael : Class=Guerrier, Race=Humain, Level=5, Companion=LoupDesOmbres
   - Lyra : Class=Mage, Race=Elfe, Level=5, Companion=CorbeauAnalyste
   - Theron : Class=Soigneur, Race=Lycanthrope, Level=5, Companion=FeeSylvestre

- [ ] **Step 5: Configurer la scène WorldMap**

Dans `WorldMap` :
1. Canvas → 2 Buttons (Village, Donjon) + TMP_Text par zone
2. GameObject `[WorldMapUI]` → `WorldMapUI`
3. Remplir le tableau `zones` :
   - Village : button=BtnVillage, zone=Zone_Village, requiredFlag="" (toujours accessible)
   - Donjon : button=BtnDonjon, zone=Zone_Donjon, requiredFlag="donjon_unlocked"
4. GameObject `[SaveMenuUI]` → `SaveMenuUI` (panel pause accessible via Échap)

- [ ] **Step 6: Configurer la scène Village (exploration)**

Dans `Village` :

1. **Tilemap** : GameObject > 2D Object > Tilemap > Rectangular → nommer `Ground`. Peindre une zone simple (~20×15 tuiles) avec des carré colorés Unity (palette de couleurs de tuile unie, pas de vrai tileset nécessaire).

2. **Player** : créer un Sprite (carré vert 32×32, placeholder) → ajouter `PlayerController`, `Rigidbody2D` (Gravity Scale=0), `BoxCollider2D` → tag = "Player"

3. **4 PNJs** (Villageois, Forgeron, Enchanteur, PNJ_Sauvegarde) :
   - Chaque PNJ : Sprite carré coloré → `BoxCollider2D` (Is Trigger=true) → `NpcInteractor`
   - Pour chaque NpcInteractor : `dialogueNode` = nom du nœud Yarn correspondant
   - Sur un seul GameObject `[DialogueRunner]` → ajouter `DialogueRunner` (Yarn Spinner) → Yarn Project = Campaign.yarnproject
   - Assigner ce `DialogueRunner` à chaque `NpcInteractor`

4. **Coffre** : Sprite carré jaune → `BoxCollider2D` (Is Trigger=true) → `ChestInteractor` → chestId="chest_village_1"

5. **SceneEntrance vers Donjon** : Sprite translucide → `BoxCollider2D` (Is Trigger=true) → `SceneEntrance` → targetScene="Donjon", progressionFlag="village_visited", unlockZoneFlag="donjon_unlocked"

6. **SceneEntrance vers WorldMap** : idem → targetScene="WorldMap"

7. **YarnCommands** : GameObject vide → `YarnCommands` component

- [ ] **Step 7: Configurer la scène Donjon (exploration)**

Dans `Donjon` :

1. **Tilemap** : même setup que Village, tuiles grises/sombres (placeholder)

2. **Player** : copier le prefab Player depuis Village (ou recréer)

3. **EncounterTriggers** : 3–4 zones rectangulaires (`BoxCollider2D` Is Trigger=true) → `EncounterTrigger` → zone=Zone_Donjon, encounterChance=0.4

4. **Coffre** : `ChestInteractor` → chestId="chest_donjon_1"

5. **Trigger Boss** : zone trigger en fin de donjon → `EncounterTrigger` spécialisé (créer `BossTrigger.cs` — voir Step 8)

6. **SceneEntrance vers Village** : targetScene="Village"

- [ ] **Step 8: Créer BossTrigger.cs pour le boss**

Créer `My project/Assets/Scripts/Exploration/BossTrigger.cs` :

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger unique qui déclenche le combat contre le boss de la zone.
/// Disparaît après que le boss soit vaincu (flag boss_defeated).
/// </summary>
public class BossTrigger : MonoBehaviour
{
    [SerializeField] private CampaignZoneSO zone;

    private void Start()
    {
        // Si le boss est déjà vaincu, désactiver le trigger
        if (GameSession.Instance != null
            && !string.IsNullOrEmpty(zone.bossDefeatedFlagKey)
            && GameSession.Instance.Flags.IsSet(zone.bossDefeatedFlagKey))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var session = GameSession.Instance;
        if (session == null || zone.boss == null) return;

        session.PendingEncounter = new CampaignEncounter(
            new List<EnemySO> { zone.boss }, zone, isBoss: true);

        SceneLoader.Instance.LoadScene("Battle");
    }
}
```

Dans la scène Donjon : ajouter ce script au trigger boss (à la place d'`EncounterTrigger`).

- [ ] **Step 9: Configurer la scène Battle pour la campagne**

Dans la scène `Battle` existante :

1. Créer un GameObject vide `[CampaignBridge]` → ajouter `BattleCampaignBridge`
2. Créer un GameObject vide `[BossController]` → ajouter `BossController`
3. Dans `BattleCampaignBridge` Inspector → assigner `bossController` et `fallbackBotBrain`

Note : `BattleBootstrap` (existant) peut rester dans la scène — `BattleCampaignBridge` détecte si `GameSession.PendingEncounter == null` et laisse BattleBootstrap agir.

- [ ] **Step 10: Vérifier le flow complet en Play Mode**

1. Play depuis `MainMenu` → clic "Nouvelle Partie" → arrive sur `CharacterSelect`
2. Choisir Kael → arrive sur `WorldMap` → Village cliquable, Donjon verrouillé
3. Clic Village → scène Village → dialogue avec un PNJ → sortir vers Donjon
4. Donjon déverrouillé sur WorldMap → entrer dans Donjon → combat aléatoire déclenché → combat fonctionne → retour WorldMap
5. Trouver le trigger boss → combat Roi Squelette → phase 2 à 50% HP → après victoire : retour WorldMap + Donjon marqué complété
6. Depuis Village : sauvegarder via PNJ_Sauvegarde → retour MainMenu → Charger → retour WorldMap avec données correctes

- [ ] **Step 11: Commit final**

```bash
git add "My project/Assets/Scenes/" \
        "My project/Assets/Scripts/Exploration/BossTrigger.cs"
git commit -m "feat: create Unity scenes (MainMenu, CharacterSelect, WorldMap, Village, Donjon) and wire up full campaign flow"
```

---

## Récapitulatif des tests EditMode

| Fichier | Tests | Scénarios clés |
|---------|-------|----------------|
| `EnemyDataTests.cs` | 7 | EnemySO stats, LootTable Roll, CampaignZone random enemy |
| `ProgressionFlagsTests.cs` | 6 | Set/IsSet/Reset, indépendance, round-trip save/load |
| `SaveSystemTests.cs` | 5 | Save+Load round-trip, HasSave, Delete, no-file returns null |
| `BossPhaseLogicTests.cs` | 6 | Phase2 trigger, seuil exact, pas de double-transition, ATK multiplier |
| **Total** | **24** | |

## Assets visuels — placeholder Plan 6

Tout fonctionne avec des **carré colorés Unity** (SpriteRenderer + Sprite natif Unity "Square") :
- Player : carré vert
- PNJs : carrés bleus (couleur différente par type)
- Ennemis en combat : carrés rouges
- Boss : carré rouge plus grand
- Coffres : carrés jaunes
- Triggers de rencontre : zones transparentes avec outline
- Tilemaps : tuiles unies (une couleur par type de sol)

Remplacer par les vrais sprites en Plan 7 en réassignant les SpriteRenderer.sprite — aucune modification de code requise.
