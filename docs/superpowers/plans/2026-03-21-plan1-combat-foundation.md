# Plan 1 — Combat Foundation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mettre en place les fondations techniques Unity (EventBus, ScriptableObjects, GameManager) et un système de combat tour par tour complet avec 3 classes jouables, le système élémentaire, les statuts et un UI de combat fonctionnel.

**Architecture:** Architecture data-driven pilotée par ScriptableObjects (ClassSO, RaceSO, SkillSO). Les systèmes communiquent via un EventBus central sans couplage direct. Le calcul de dégâts est isolé dans DamageCalculator (pur C# sans MonoBehaviour) pour être entièrement testable.

**Tech Stack:** Unity 2022 LTS ou 2023 LTS, C#, Unity Test Framework (NUnit EditMode), Unity UI Toolkit / uGUI, TextMeshPro, Unity Input System

**Spec de référence:** `docs/superpowers/specs/2026-03-21-rpg-design.md`

**Périmètre de ce plan (Roadmap étapes 1-3) :**
- Étape 1 : ScriptableObjects, EventBus, GameManager
- Étape 2 : BattleManager, TurnSystem, DamageCalculator, UI combat
- Étape 3 : 3 classes (Guerrier, Mage, Soigneur), système élémentaire, statuts

**Livrable final :** Un combat tour par tour jouable clavier/souris entre deux personnages (Guerrier vs Mage ou Mage vs Soigneur), avec menus d'actions, système élémentaire, statuts, affichage HP/MP et fin de combat détectée.

---

## Structure des fichiers

```
Assets/
├── _Data/
│   ├── Classes/
│   │   ├── Guerrier.asset
│   │   ├── Mage.asset
│   │   └── Soigneur.asset
│   ├── Races/
│   │   └── Humain.asset
│   └── Skills/
│       ├── AttaqueBasique.asset
│       ├── BouleseDeFeu.asset
│       ├── Soin.asset
│       └── ... (skills de base)
├── Scripts/
│   ├── Core/
│   │   ├── EventBus.cs            ← Bus d'événements générique (static)
│   │   ├── GameManager.cs         ← Singleton, état global du jeu
│   │   └── SceneLoader.cs         ← Chargement de scènes async
│   ├── Data/
│   │   ├── ClassSO.cs             ← ScriptableObject définissant une classe
│   │   ├── RaceSO.cs              ← ScriptableObject définissant une race
│   │   └── SkillSO.cs             ← ScriptableObject définissant un skill
│   ├── Characters/
│   │   └── CharacterData.cs       ← État runtime d'un personnage (HP, MP, statuts...)
│   ├── Combat/
│   │   ├── ElementSystem.cs       ← Matrice 6x6 des affinités élémentaires
│   │   ├── DamageCalculator.cs    ← Calcul dégâts physiques et magiques
│   │   ├── StatusManager.cs       ← Application et tick des statuts
│   │   ├── TurnSystem.cs          ← Ordre d'initiative, gestion des tours
│   │   ├── ActionResolver.cs      ← Résolution des actions choisies
│   │   └── BattleManager.cs       ← Orchestrateur principal du combat
│   └── UI/
│       ├── BattleHUD.cs           ← Affichage HP/MP, nom, statuts
│       ├── ActionMenuUI.cs        ← Menu Attaquer/Sorts/Objets/Passer
│       └── BattleLog.cs           ← Log texte des actions
├── Tests/
│   └── EditMode/
│       ├── ElementSystemTests.cs
│       ├── DamageCalculatorTests.cs
│       ├── StatusManagerTests.cs
│       └── TurnSystemTests.cs
└── Scenes/
    └── Battle.unity
```

---

## Task 1 : Setup du projet Unity

**Files:**
- Create : Structure de dossiers Assets/
- Create : `Assets/Tests/EditMode/` avec assembly definition

- [ ] **Step 1 : Créer le projet Unity**

  Dans Unity Hub, créer un nouveau projet :
  - Template : **2D (Core)** ou **2D URP**
  - Nom : `RPG`
  - Dossier : `C:/Users/Misscrazy/Documents/projets/RPG`
  - Version Unity recommandée : **2022.3 LTS** ou **2023.2 LTS**

- [ ] **Step 2 : Installer les packages requis**

  Window → Package Manager → Add package by name :
  ```
  com.unity.inputsystem
  com.unity.textmeshpro
  com.unity.test-framework
  ```

  Via Add package from git URL :
  ```
  https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git
  ```

  Via NuGet (ou Unity Package) — DOTween :
  - Importer DOTween depuis l'Asset Store (gratuit) OU ajouter : `com.demigiant.dotween`

- [ ] **Step 3 : Créer la structure de dossiers**

  Dans l'éditeur Unity (Project window), créer manuellement :
  ```
  Assets/_Data/Classes/
  Assets/_Data/Races/
  Assets/_Data/Skills/
  Assets/Scripts/Core/
  Assets/Scripts/Data/
  Assets/Scripts/Characters/
  Assets/Scripts/Combat/
  Assets/Scripts/UI/
  Assets/Tests/EditMode/
  Assets/Scenes/
  ```

- [ ] **Step 4 : Créer l'assembly definition pour les tests**

  Dans `Assets/Tests/EditMode/`, clic droit → Create → Assembly Definition :
  - Nom : `RPG.Tests.EditMode`
  - Cocher : **Editor** uniquement
  - References : ajouter `UnityEngine.TestRunner`, `UnityEditor.TestRunner`

- [ ] **Step 5 : Créer l'assembly definition principale**

  Dans `Assets/Scripts/`, clic droit → Create → Assembly Definition :
  - Nom : `RPG.Runtime`
  - Dans `RPG.Tests.EditMode`, ajouter `RPG.Runtime` aux references

- [ ] **Step 6 : Commit initial**

  ```bash
  git init
  git add .
  git commit -m "chore: setup Unity project structure with test framework"
  ```

---

## Task 2 : EventBus

**Files:**
- Create : `Assets/Scripts/Core/EventBus.cs`
- Test : `Assets/Tests/EditMode/EventBusTests.cs`

- [ ] **Step 1 : Écrire le test**

  Créer `Assets/Tests/EditMode/EventBusTests.cs` :
  ```csharp
  using NUnit.Framework;

  public class EventBusTests
  {
      [SetUp]
      public void Setup() => EventBus.Clear();

      [Test]
      public void Subscribe_ThenPublish_CallsHandler()
      {
          bool called = false;
          EventBus.Subscribe<string>(msg => called = true);
          EventBus.Publish("hello");
          Assert.IsTrue(called);
      }

      [Test]
      public void Unsubscribe_ThenPublish_DoesNotCallHandler()
      {
          bool called = false;
          System.Action<string> handler = _ => called = true;
          EventBus.Subscribe(handler);
          EventBus.Unsubscribe(handler);
          EventBus.Publish("hello");
          Assert.IsFalse(called);
      }

      [Test]
      public void Publish_WithNoSubscribers_DoesNotThrow()
      {
          Assert.DoesNotThrow(() => EventBus.Publish(42));
      }

      [Test]
      public void MultipleSubscribers_AllReceiveEvent()
      {
          int count = 0;
          EventBus.Subscribe<int>(_ => count++);
          EventBus.Subscribe<int>(_ => count++);
          EventBus.Publish(1);
          Assert.AreEqual(2, count);
      }
  }
  ```

- [ ] **Step 2 : Lancer les tests — vérifier qu'ils échouent**

  Unity Editor → Window → General → Test Runner → EditMode → Run All
  Attendu : 4 tests en erreur (EventBus introuvable)

- [ ] **Step 3 : Implémenter EventBus**

  Créer `Assets/Scripts/Core/EventBus.cs` :
  ```csharp
  using System;
  using System.Collections.Generic;

  public static class EventBus
  {
      private static readonly Dictionary<Type, List<object>> _handlers = new();

      public static void Subscribe<T>(Action<T> handler)
      {
          var type = typeof(T);
          if (!_handlers.ContainsKey(type))
              _handlers[type] = new List<object>();
          _handlers[type].Add(handler);
      }

      public static void Unsubscribe<T>(Action<T> handler)
      {
          var type = typeof(T);
          if (_handlers.TryGetValue(type, out var list))
              list.Remove(handler);
      }

      public static void Publish<T>(T eventData)
      {
          var type = typeof(T);
          if (!_handlers.TryGetValue(type, out var list)) return;
          foreach (var handler in new List<object>(list))
              ((Action<T>)handler)(eventData);
      }

      public static void Clear() => _handlers.Clear();
  }
  ```

- [ ] **Step 4 : Lancer les tests — vérifier qu'ils passent**

  Test Runner → EditMode → Run All
  Attendu : 4 tests PASS (vert)

- [ ] **Step 5 : Définir les événements du jeu**

  Créer `Assets/Scripts/Core/GameEvents.cs` :
  ```csharp
  // Événements publiés dans le bus — un struct par type d'événement
  public struct TurnStartedEvent    { public CharacterData Character; }
  public struct TurnEndedEvent      { public CharacterData Character; }
  public struct ActionResolvedEvent { public ActionResult Result; }
  public struct CharacterDiedEvent  { public CharacterData Character; }
  public struct BattleEndedEvent    { public bool PlayerWon; }
  public struct StatusAppliedEvent  { public CharacterData Target; public StatusEffect Status; }
  ```
  > Note : `CharacterData`, `ActionResult` et `StatusEffect` seront créés dans les tasks suivantes. Unity va afficher des erreurs de compilation temporaires — c'est normal, elles disparaîtront quand les types seront créés.

- [ ] **Step 6 : Commit**

  ```bash
  git add Assets/Scripts/Core/EventBus.cs Assets/Scripts/Core/GameEvents.cs Assets/Tests/EditMode/EventBusTests.cs
  git commit -m "feat: add generic EventBus and game event definitions"
  ```

---

## Task 3 : ScriptableObjects de données

**Files:**
- Create : `Assets/Scripts/Data/ClassSO.cs`
- Create : `Assets/Scripts/Data/RaceSO.cs`
- Create : `Assets/Scripts/Data/SkillSO.cs`

- [ ] **Step 1 : Créer les enums partagés**

  Créer `Assets/Scripts/Data/Enums.cs` :
  ```csharp
  public enum ElementType { None, Fire, Nature, Lightning, Water, Light, Dark }
  public enum StatusEffectType { None, Burn, Poison, Freeze, Paralysis, Confusion, Shield }
  public enum EquipmentSlot { MainWeapon, Offhand, Helmet, Armor, Boots, Ring1, Ring2 }
  public enum SkillTargetType { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
  public enum SkillDamageType { Physical, Magical, Healing, Status }
  public enum ClassRole { DPS, Tank, Healer, Support, Summoner }
  ```

- [ ] **Step 2 : Créer SkillSO**

  Créer `Assets/Scripts/Data/SkillSO.cs` :
  ```csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewSkill", menuName = "RPG/Skill")]
  public class SkillSO : ScriptableObject
  {
      [Header("Identité")]
      public string skillName;
      [TextArea] public string description;

      [Header("Type")]
      public SkillDamageType damageType;
      public SkillTargetType targetType;
      public ElementType element;

      [Header("Coût & Cooldown")]
      public int mpCost;
      public int cooldownTurns;      // 0 = pas de cooldown

      [Header("Puissance")]
      [Tooltip("Multiplicateur appliqué à ATK ou MAG. Ex: 1.5 = 150% de la stat")]
      public float powerMultiplier = 1f;

      [Header("Effets de statut")]
      public StatusEffectType statusEffect;
      [Range(0f, 1f)] public float statusChance;  // 0.3 = 30% de chance
  }
  ```

- [ ] **Step 3 : Créer ClassSO**

  Créer `Assets/Scripts/Data/ClassSO.cs` :
  ```csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewClass", menuName = "RPG/Class")]
  public class ClassSO : ScriptableObject
  {
      [Header("Identité")]
      public string className;
      public ClassRole role;
      [TextArea] public string description;

      [Header("Stats de base (niveau 1)")]
      public int baseHP = 100;
      public int baseMP = 50;
      public int baseATK = 10;
      public int baseDEF = 10;
      public int baseMAG = 10;
      public int baseRES = 10;
      public int baseAGI = 10;
      public int baseLCK = 5;

      [Header("Croissance par niveau (valeur ajoutée par level up)")]
      public int hpGrowth = 15;
      public int mpGrowth = 8;
      public int atkGrowth = 2;
      public int defGrowth = 2;
      public int magGrowth = 2;
      public int resGrowth = 2;
      public int agiGrowth = 1;
      public int lckGrowth = 1;

      [Header("Affinité élémentaire naturelle")]
      public ElementType elementalAffinity;

      [Header("Skills de départ (tronc commun)")]
      public SkillSO[] startingSkills;
  }
  ```

- [ ] **Step 4 : Créer RaceSO**

  Créer `Assets/Scripts/Data/RaceSO.cs` :
  ```csharp
  using UnityEngine;

  [CreateAssetMenu(fileName = "NewRace", menuName = "RPG/Race")]
  public class RaceSO : ScriptableObject
  {
      [Header("Identité")]
      public string raceName;
      [TextArea] public string description;

      [Header("Modificateurs de stats (% ajouté aux stats de base)")]
      [Range(-0.5f, 0.5f)] public float hpModifier;
      [Range(-0.5f, 0.5f)] public float mpModifier;
      [Range(-0.5f, 0.5f)] public float atkModifier;
      [Range(-0.5f, 0.5f)] public float defModifier;
      [Range(-0.5f, 0.5f)] public float magModifier;
      [Range(-0.5f, 0.5f)] public float resModifier;
      [Range(-0.5f, 0.5f)] public float agiModifier;
      [Range(-0.5f, 0.5f)] public float lckModifier;

      [Header("Affinité élémentaire")]
      public ElementType elementalAffinity;

      [Header("Immunités aux statuts")]
      public StatusEffectType[] statusImmunities;

      [Header("Bonus passif spécial (description textuelle pour l'UI)")]
      public string passiveDescription;
  }
  ```

- [ ] **Step 5 : Commit**

  ```bash
  git add Assets/Scripts/Data/
  git commit -m "feat: add ScriptableObject definitions for Class, Race and Skill"
  ```

---

## Task 4 : CharacterData (état runtime)

**Files:**
- Create : `Assets/Scripts/Characters/CharacterData.cs`
- Create : `Assets/Scripts/Combat/StatusEffect.cs`
- Test : `Assets/Tests/EditMode/CharacterDataTests.cs`

- [ ] **Step 1 : Créer StatusEffect**

  Créer `Assets/Scripts/Combat/StatusEffect.cs` :
  ```csharp
  [System.Serializable]
  public class StatusEffect
  {
      public StatusEffectType type;
      public int remainingTurns;
      public float value;      // valeur de l'effet (ex: absorption HP pour Shield)

      public StatusEffect(StatusEffectType type, int turns, float value = 0f)
      {
          this.type = type;
          this.remainingTurns = turns;
          this.value = value;
      }
  }
  ```

- [ ] **Step 2 : Écrire les tests CharacterData**

  Créer `Assets/Tests/EditMode/CharacterDataTests.cs` :
  ```csharp
  using NUnit.Framework;

  public class CharacterDataTests
  {
      private CharacterData CreateTestCharacter()
      {
          var data = new CharacterData();
          data.Initialize("TestHero", 100, 50, 10, 10, 10, 10, 10, 5);
          return data;
      }

      [Test]
      public void TakeDamage_ReducesHP()
      {
          var c = CreateTestCharacter();
          c.TakeDamage(30);
          Assert.AreEqual(70, c.CurrentHP);
      }

      [Test]
      public void TakeDamage_CannotGoBelowZero()
      {
          var c = CreateTestCharacter();
          c.TakeDamage(200);
          Assert.AreEqual(0, c.CurrentHP);
      }

      [Test]
      public void Heal_IncreasesHP()
      {
          var c = CreateTestCharacter();
          c.TakeDamage(50);
          c.Heal(20);
          Assert.AreEqual(70, c.CurrentHP);
      }

      [Test]
      public void Heal_CannotExceedMaxHP()
      {
          var c = CreateTestCharacter();
          c.Heal(999);
          Assert.AreEqual(100, c.CurrentHP);
      }

      [Test]
      public void IsDead_WhenHPIsZero()
      {
          var c = CreateTestCharacter();
          c.TakeDamage(100);
          Assert.IsTrue(c.IsDead);
      }

      [Test]
      public void IsAlive_WhenHPIsPositive()
      {
          var c = CreateTestCharacter();
          Assert.IsFalse(c.IsDead);
      }
  }
  ```

- [ ] **Step 3 : Lancer les tests — vérifier qu'ils échouent**

  Test Runner → EditMode → Run All
  Attendu : échec (CharacterData introuvable)

- [ ] **Step 4 : Implémenter CharacterData**

  Créer `Assets/Scripts/Characters/CharacterData.cs` :
  ```csharp
  using System.Collections.Generic;

  public class CharacterData
  {
      // Identité
      public string CharacterName { get; private set; }

      // Stats max (calculées depuis ClassSO + RaceSO + équipement)
      public int MaxHP { get; private set; }
      public int MaxMP { get; private set; }
      public int ATK { get; private set; }
      public int DEF { get; private set; }
      public int MAG { get; private set; }
      public int RES { get; private set; }
      public int AGI { get; private set; }
      public int LCK { get; private set; }

      // Stats courantes
      public int CurrentHP { get; private set; }
      public int CurrentMP { get; private set; }
      public bool IsDead => CurrentHP <= 0;

      // Données de source
      public ClassSO Class { get; private set; }
      public RaceSO Race { get; private set; }
      public int Level { get; private set; } = 1;
      public ElementType ElementalAffinity { get; private set; }

      // Statuts actifs
      public List<StatusEffect> ActiveStatuses { get; } = new();

      // Skills disponibles
      public List<SkillSO> Skills { get; } = new();

      // Cooldowns (key = skill name, value = tours restants)
      public Dictionary<string, int> Cooldowns { get; } = new();

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

          // Calcul stats = base + (growth × level) + modificateurs de race
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

          // Charger les skills de départ
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

      public bool HasStatus(StatusEffectType type) =>
          ActiveStatuses.Exists(s => s.type == type);

      public bool IsImmuneToStatus(StatusEffectType type) =>
          Race != null && System.Array.Exists(Race.statusImmunities, s => s == type);
  }
  ```
  > Note : `Mathf` est dans `UnityEngine`. Si les tests EditMode ne trouvent pas `Mathf`, remplacer par `(int)System.Math.Round(...)`.

- [ ] **Step 5 : Lancer les tests — vérifier qu'ils passent**

  Test Runner → EditMode → Run All
  Attendu : tous les tests PASS

- [ ] **Step 6 : Commit**

  ```bash
  git add Assets/Scripts/Characters/ Assets/Scripts/Combat/StatusEffect.cs Assets/Tests/EditMode/CharacterDataTests.cs
  git commit -m "feat: add CharacterData runtime state and StatusEffect"
  ```

---

## Task 5 : ElementSystem

**Files:**
- Create : `Assets/Scripts/Combat/ElementSystem.cs`
- Test : `Assets/Tests/EditMode/ElementSystemTests.cs`

- [ ] **Step 1 : Écrire les tests**

  Créer `Assets/Tests/EditMode/ElementSystemTests.cs` :
  ```csharp
  using NUnit.Framework;

  public class ElementSystemTests
  {
      [Test]
      public void Fire_VsNature_ReturnsAdvantage()
          => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Nature));

      [Test]
      public void Fire_VsWater_ReturnsDisadvantage()
          => Assert.AreEqual(0.75f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Water));

      [Test]
      public void Fire_VsFire_ReturnsNeutral()
          => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Fire));

      [Test]
      public void Light_VsDark_ReturnsAdvantage()
          => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Light, ElementType.Dark));

      [Test]
      public void Dark_VsLight_ReturnsAdvantage()
          => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Dark, ElementType.Light));

      [Test]
      public void Fire_VsLight_ReturnsNeutral()
          => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Light));

      [Test]
      public void None_VsAnything_ReturnsNeutral()
          => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.None, ElementType.Fire));

      [Test]
      public void Water_VsFire_ReturnsAdvantage()
          => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Water, ElementType.Fire));

      [Test]
      public void Nature_VsFire_ReturnsDisadvantage()
          => Assert.AreEqual(0.75f, ElementSystem.GetModifier(ElementType.Nature, ElementType.Fire));
  }
  ```

- [ ] **Step 2 : Lancer les tests — vérifier qu'ils échouent**

- [ ] **Step 3 : Implémenter ElementSystem**

  Créer `Assets/Scripts/Combat/ElementSystem.cs` :
  ```csharp
  using System.Collections.Generic;

  public static class ElementSystem
  {
      // Matrice des avantages élémentaires
      // Key : (attacker, defender) → modifier
      private static readonly Dictionary<(ElementType, ElementType), float> _matrix
          = new()
      {
          // Cycle naturel : Fire > Nature > Lightning > Water > Fire
          { (ElementType.Fire,      ElementType.Nature),    1.25f },
          { (ElementType.Fire,      ElementType.Water),     0.75f },
          { (ElementType.Nature,    ElementType.Lightning), 1.25f },
          { (ElementType.Nature,    ElementType.Fire),      0.75f },
          { (ElementType.Lightning, ElementType.Water),     1.25f },
          { (ElementType.Lightning, ElementType.Nature),    0.75f },
          { (ElementType.Water,     ElementType.Fire),      1.25f },
          { (ElementType.Water,     ElementType.Lightning), 0.75f },
          // Paire Lumière/Ténèbres : opposition mutuelle (×1.25 dans les deux sens)
          // Celui qui reçoit subit ×0.75 (géré par la valeur de la colonne défenseur)
          { (ElementType.Light,     ElementType.Dark),      1.25f },
          { (ElementType.Dark,      ElementType.Light),     1.25f },
      };

      /// <summary>
      /// Retourne le modificateur de dégâts pour un skill d'élément attackerElement
      /// utilisé contre une cible ayant l'affinité defenderAffinity.
      /// </summary>
      public static float GetModifier(ElementType attackerElement, ElementType defenderAffinity)
      {
          if (attackerElement == ElementType.None || defenderAffinity == ElementType.None)
              return 1.0f;
          if (attackerElement == defenderAffinity)
              return 1.0f;

          if (_matrix.TryGetValue((attackerElement, defenderAffinity), out float mod))
              return mod;

          return 1.0f;
      }
  }
  ```

- [ ] **Step 4 : Lancer les tests — vérifier qu'ils passent**

- [ ] **Step 5 : Commit**

  ```bash
  git add Assets/Scripts/Combat/ElementSystem.cs Assets/Tests/EditMode/ElementSystemTests.cs
  git commit -m "feat: add elemental affinity system with 6x6 matrix"
  ```

---

## Task 6 : DamageCalculator

**Files:**
- Create : `Assets/Scripts/Combat/DamageCalculator.cs`
- Create : `Assets/Scripts/Combat/ActionResult.cs`
- Test : `Assets/Tests/EditMode/DamageCalculatorTests.cs`

- [ ] **Step 1 : Créer ActionResult**

  Créer `Assets/Scripts/Combat/ActionResult.cs` :
  ```csharp
  public class ActionResult
  {
      public CharacterData Source;
      public CharacterData Target;
      public SkillSO Skill;
      public int DamageDealt;
      public int HealingDone;
      public bool WasCritical;
      public float ElementalModifier;
      public StatusEffect AppliedStatus;
      public bool TargetDied;
      public string Description;   // texte pour le BattleLog
  }
  ```

- [ ] **Step 2 : Écrire les tests**

  Créer `Assets/Tests/EditMode/DamageCalculatorTests.cs` :
  ```csharp
  using NUnit.Framework;

  public class DamageCalculatorTests
  {
      private CharacterData MakeChar(int atk, int def, int mag, int res, int lck,
                                     ElementType affinity = ElementType.None)
      {
          var c = new CharacterData();
          c.Initialize("Test", 200, 50, atk, def, mag, res, 10, lck);
          // Injecter l'affinité directement (via un setter ou un champ public pour les tests)
          c.ElementalAffinity_TestOnly = affinity;
          return c;
      }

      [Test]
      public void Physical_BasicDamage_IsATKx2MinusDEF()
      {
          // ATK=20, DEF=10 → brut = 40-10 = 30
          var attacker = MakeChar(atk: 20, def: 0, mag: 0, res: 0, lck: 0);
          var defender = MakeChar(atk: 0, def: 10, mag: 0, res: 0, lck: 0);
          int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: false);
          Assert.AreEqual(30, dmg);
      }

      [Test]
      public void Physical_HighDEF_DamageMinimumIsOne()
      {
          var attacker = MakeChar(atk: 5, def: 0, mag: 0, res: 0, lck: 0);
          var defender = MakeChar(atk: 0, def: 100, mag: 0, res: 0, lck: 0);
          int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: false);
          Assert.AreEqual(1, dmg);
      }

      [Test]
      public void Magical_BasicDamage_IsMAGx2MinusRES()
      {
          var attacker = MakeChar(atk: 0, def: 0, mag: 15, res: 0, lck: 0);
          var defender = MakeChar(atk: 0, def: 0, mag: 0, res: 5, lck: 0);
          int dmg = DamageCalculator.CalculateMagical(attacker, defender,
              ElementType.Fire, critOverride: false);
          // MAG×2 - RES = 30 - 5 = 25
          Assert.AreEqual(25, dmg);
      }

      [Test]
      public void ElementalAdvantage_Multiplies1_25()
      {
          var attacker = MakeChar(atk: 0, def: 0, mag: 20, res: 0, lck: 0);
          var defender = MakeChar(atk: 0, def: 0, mag: 0, res: 0, lck: 0,
                                  affinity: ElementType.Nature);
          // MAG×2 = 40, ×1.25 = 50
          int dmg = DamageCalculator.CalculateMagical(attacker, defender,
              ElementType.Fire, critOverride: false);
          Assert.AreEqual(50, dmg);
      }

      [Test]
      public void CriticalHit_Multiplies1_5()
      {
          var attacker = MakeChar(atk: 20, def: 0, mag: 0, res: 0, lck: 0);
          var defender = MakeChar(atk: 0, def: 10, mag: 0, res: 0, lck: 0);
          int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: true);
          // brut = 30, ×1.5 = 45
          Assert.AreEqual(45, dmg);
      }
  }
  ```

- [ ] **Step 3 : Lancer les tests — vérifier qu'ils échouent**

- [ ] **Step 4 : Ajouter le setter de test dans CharacterData**

  Dans `CharacterData.cs`, ajouter :
  ```csharp
  // Uniquement pour les tests — ne pas utiliser en runtime
  public ElementType ElementalAffinity_TestOnly
  {
      set => ElementalAffinity = value;
  }
  ```

- [ ] **Step 5 : Implémenter DamageCalculator**

  Créer `Assets/Scripts/Combat/DamageCalculator.cs` :
  ```csharp
  using UnityEngine;

  public static class DamageCalculator
  {
      public static int CalculatePhysical(CharacterData attacker, CharacterData defender,
                                          bool critOverride = false, float powerMultiplier = 1f)
      {
          float raw = attacker.ATK * 2f - defender.DEF;
          raw = Mathf.Max(1f, raw);

          bool isCrit = critOverride || Random.value < (attacker.LCK / 400f);
          float critMod = isCrit ? 1.5f : 1f;

          return Mathf.RoundToInt(raw * powerMultiplier * critMod);
      }

      public static int CalculateMagical(CharacterData attacker, CharacterData defender,
                                         ElementType skillElement, bool critOverride = false,
                                         float powerMultiplier = 1f)
      {
          float raw = attacker.MAG * 2f - defender.RES;
          raw = Mathf.Max(1f, raw);

          float elemMod = ElementSystem.GetModifier(skillElement, defender.ElementalAffinity);
          bool isCrit = critOverride || Random.value < (attacker.LCK / 400f);
          float critMod = isCrit ? 1.5f : 1f;

          return Mathf.RoundToInt(raw * powerMultiplier * elemMod * critMod);
      }

      public static int CalculateHealing(CharacterData caster, float multiplier = 1f)
      {
          return Mathf.RoundToInt(caster.MAG * multiplier);
      }
  }
  ```

- [ ] **Step 6 : Lancer les tests — vérifier qu'ils passent**

- [ ] **Step 7 : Commit**

  ```bash
  git add Assets/Scripts/Combat/DamageCalculator.cs Assets/Scripts/Combat/ActionResult.cs Assets/Tests/EditMode/DamageCalculatorTests.cs
  git commit -m "feat: add DamageCalculator with physical, magical and healing formulas"
  ```

---

## Task 7 : StatusManager

**Files:**
- Create : `Assets/Scripts/Combat/StatusManager.cs`
- Test : `Assets/Tests/EditMode/StatusManagerTests.cs`

- [ ] **Step 1 : Écrire les tests**

  Créer `Assets/Tests/EditMode/StatusManagerTests.cs` :
  ```csharp
  using NUnit.Framework;

  public class StatusManagerTests
  {
      private CharacterData MakeChar()
      {
          var c = new CharacterData();
          c.Initialize("Test", 200, 50, 10, 10, 10, 10, 10, 5);
          return c;
      }

      [Test]
      public void ApplyBurn_AddsStatusToCharacter()
      {
          var c = MakeChar();
          StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
          Assert.IsTrue(c.HasStatus(StatusEffectType.Burn));
      }

      [Test]
      public void TickBurn_DealsDamageEachTurn()
      {
          var c = MakeChar();
          StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
          StatusManager.Tick(c);
          Assert.Less(c.CurrentHP, 200);
      }

      [Test]
      public void TickStatus_DecrementsRemainingTurns()
      {
          var c = MakeChar();
          StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
          StatusManager.Tick(c);
          var status = c.ActiveStatuses.Find(s => s.type == StatusEffectType.Burn);
          Assert.AreEqual(2, status.remainingTurns);
      }

      [Test]
      public void TickStatus_RemovesWhenTurnsReachZero()
      {
          var c = MakeChar();
          StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 1));
          StatusManager.Tick(c);
          Assert.IsFalse(c.HasStatus(StatusEffectType.Burn));
      }

      [Test]
      public void Apply_IgnoresImmune_WhenRaceIsImmune()
      {
          var c = MakeChar();
          // Simuler une immunité via mock
          c.SetImmunity_TestOnly(StatusEffectType.Poison);
          StatusManager.Apply(c, new StatusEffect(StatusEffectType.Poison, 3));
          Assert.IsFalse(c.HasStatus(StatusEffectType.Poison));
      }
  }
  ```

- [ ] **Step 2 : Ajouter le mock d'immunité dans CharacterData**

  Dans `CharacterData.cs`, ajouter :
  ```csharp
  private HashSet<StatusEffectType> _testImmunities = new();
  public void SetImmunity_TestOnly(StatusEffectType type) => _testImmunities.Add(type);

  public bool IsImmuneToStatus(StatusEffectType type)
  {
      if (_testImmunities.Contains(type)) return true;
      return Race != null && System.Array.Exists(Race.statusImmunities, s => s == type);
  }
  ```

- [ ] **Step 3 : Lancer les tests — vérifier qu'ils échouent**

- [ ] **Step 4 : Implémenter StatusManager**

  Créer `Assets/Scripts/Combat/StatusManager.cs` :
  ```csharp
  using UnityEngine;
  using System.Collections.Generic;

  public static class StatusManager
  {
      // Durées par défaut selon la spec
      private static readonly Dictionary<StatusEffectType, int> DefaultDurations = new()
      {
          { StatusEffectType.Burn,      3 },
          { StatusEffectType.Poison,    3 },
          { StatusEffectType.Freeze,    1 },
          { StatusEffectType.Paralysis, 2 },
          { StatusEffectType.Confusion, 2 },
          { StatusEffectType.Shield,    999 }, // persiste jusqu'à épuisement
      };

      public static void Apply(CharacterData target, StatusEffect effect)
      {
          if (target.IsImmuneToStatus(effect.type)) return;
          // Ne pas stacker le Shield
          if (effect.type == StatusEffectType.Shield && target.HasStatus(StatusEffectType.Shield))
              return;
          target.ActiveStatuses.Add(effect);
          EventBus.Publish(new StatusAppliedEvent { Target = target, Status = effect });
      }

      /// <summary>Applique les effets de tous les statuts actifs et décrémente les compteurs.</summary>
      public static void Tick(CharacterData character)
      {
          var toRemove = new List<StatusEffect>();

          foreach (var status in character.ActiveStatuses)
          {
              switch (status.type)
              {
                  case StatusEffectType.Burn:
                  case StatusEffectType.Poison:
                      int dmg = Mathf.Max(1, Mathf.RoundToInt(character.MaxHP * 0.05f));
                      character.TakeDamage(dmg);
                      break;
              }

              status.remainingTurns--;
              if (status.remainingTurns <= 0)
                  toRemove.Add(status);
          }

          foreach (var s in toRemove)
              character.ActiveStatuses.Remove(s);
      }

      /// <summary>Vérifie si le personnage peut agir ce tour (Freeze/Paralysis).</summary>
      public static bool CanAct(CharacterData character)
      {
          if (character.HasStatus(StatusEffectType.Freeze)) return false;
          if (character.HasStatus(StatusEffectType.Paralysis))
              return Random.value > 0.5f;
          return true;
      }

      /// <summary>Absorbe les dégâts sur le Shield avant de les appliquer aux HP.</summary>
      public static int AbsorbWithShield(CharacterData target, int incomingDamage)
      {
          var shield = target.ActiveStatuses.Find(s => s.type == StatusEffectType.Shield);
          if (shield == null) return incomingDamage;

          if (shield.value >= incomingDamage)
          {
              shield.value -= incomingDamage;
              if (shield.value <= 0) target.ActiveStatuses.Remove(shield);
              return 0;
          }
          else
          {
              int remaining = incomingDamage - (int)shield.value;
              target.ActiveStatuses.Remove(shield);
              return remaining;
          }
      }
  }
  ```

- [ ] **Step 5 : Lancer les tests — vérifier qu'ils passent**

- [ ] **Step 6 : Commit**

  ```bash
  git add Assets/Scripts/Combat/StatusManager.cs Assets/Tests/EditMode/StatusManagerTests.cs
  git commit -m "feat: add StatusManager with tick, apply and shield absorption"
  ```

---

## Task 8 : TurnSystem

**Files:**
- Create : `Assets/Scripts/Combat/TurnSystem.cs`
- Test : `Assets/Tests/EditMode/TurnSystemTests.cs`

- [ ] **Step 1 : Écrire les tests**

  Créer `Assets/Tests/EditMode/TurnSystemTests.cs` :
  ```csharp
  using NUnit.Framework;
  using System.Collections.Generic;

  public class TurnSystemTests
  {
      private CharacterData MakeChar(string name, int agi)
      {
          var c = new CharacterData();
          c.Initialize(name, 100, 50, 10, 10, 10, 10, agi, 0);
          return c;
      }

      [Test]
      public void Initiative_OrderedByAGIDescending()
      {
          var slow  = MakeChar("Slow",  5);
          var fast  = MakeChar("Fast",  20);
          var mid   = MakeChar("Mid",   12);
          var chars = new List<CharacterData> { slow, fast, mid };

          var ts = new TurnSystem(chars);
          Assert.AreEqual("Fast", ts.CurrentCharacter.CharacterName);
      }

      [Test]
      public void NextTurn_AdvancesToNextCharacter()
      {
          var a = MakeChar("A", 20);
          var b = MakeChar("B", 10);
          var ts = new TurnSystem(new List<CharacterData> { a, b });

          ts.NextTurn();
          Assert.AreEqual("B", ts.CurrentCharacter.CharacterName);
      }

      [Test]
      public void NextTurn_WrapsAroundToFirst()
      {
          var a = MakeChar("A", 20);
          var b = MakeChar("B", 10);
          var ts = new TurnSystem(new List<CharacterData> { a, b });

          ts.NextTurn(); // → B
          ts.NextTurn(); // → A (cycle)
          Assert.AreEqual("A", ts.CurrentCharacter.CharacterName);
      }

      [Test]
      public void NextTurn_SkipsDeadCharacters()
      {
          var a = MakeChar("A", 20);
          var b = MakeChar("B", 15);
          var c = MakeChar("C", 10);
          b.TakeDamage(200); // tuer B
          var ts = new TurnSystem(new List<CharacterData> { a, b, c });

          ts.NextTurn(); // B est mort → doit sauter à C
          Assert.AreEqual("C", ts.CurrentCharacter.CharacterName);
      }

      [Test]
      public void TurnNumber_IncrementsEachFullRound()
      {
          var a = MakeChar("A", 20);
          var b = MakeChar("B", 10);
          var ts = new TurnSystem(new List<CharacterData> { a, b });

          Assert.AreEqual(1, ts.TurnNumber);
          ts.NextTurn(); ts.NextTurn(); // tour complet
          Assert.AreEqual(2, ts.TurnNumber);
      }
  }
  ```

- [ ] **Step 2 : Lancer les tests — vérifier qu'ils échouent**

- [ ] **Step 3 : Implémenter TurnSystem**

  Créer `Assets/Scripts/Combat/TurnSystem.cs` :
  ```csharp
  using System.Collections.Generic;
  using System.Linq;

  public class TurnSystem
  {
      private List<CharacterData> _order;
      private int _currentIndex;

      public CharacterData CurrentCharacter => _order[_currentIndex];
      public int TurnNumber { get; private set; } = 1;
      private int _roundSize;

      public TurnSystem(List<CharacterData> characters)
      {
          _order = characters.OrderByDescending(c => c.AGI).ToList();
          _currentIndex = 0;
          _roundSize = _order.Count;
          EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
      }

      public void NextTurn()
      {
          EventBus.Publish(new TurnEndedEvent { Character = CurrentCharacter });

          int steps = 0;
          do
          {
              _currentIndex = (_currentIndex + 1) % _order.Count;
              steps++;
              // Détecter si un round complet s'est écoulé
              if (_currentIndex == 0) TurnNumber++;
          }
          while (CurrentCharacter.IsDead && steps < _order.Count);

          EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
      }

      public List<CharacterData> GetAliveCharacters() =>
          _order.Where(c => !c.IsDead).ToList();
  }
  ```

- [ ] **Step 4 : Lancer les tests — vérifier qu'ils passent**

- [ ] **Step 5 : Commit**

  ```bash
  git add Assets/Scripts/Combat/TurnSystem.cs Assets/Tests/EditMode/TurnSystemTests.cs
  git commit -m "feat: add TurnSystem with AGI-based initiative and dead character skip"
  ```

---

## Task 9 : ActionResolver & BattleManager

**Files:**
- Create : `Assets/Scripts/Combat/ActionResolver.cs`
- Create : `Assets/Scripts/Combat/BattleManager.cs`
- Modify : `Assets/Scripts/Core/GameManager.cs`

- [ ] **Step 1 : Implémenter ActionResolver**

  Créer `Assets/Scripts/Combat/ActionResolver.cs` :
  ```csharp
  using UnityEngine;

  public static class ActionResolver
  {
      public static ActionResult ResolveSkill(CharacterData source, CharacterData target,
                                               SkillSO skill)
      {
          var result = new ActionResult { Source = source, Target = target, Skill = skill };

          // Vérifier MP
          if (source.CurrentMP < skill.mpCost)
          {
              result.Description = $"{source.CharacterName} n'a pas assez de MP !";
              return result;
          }
          source.SpendMP(skill.mpCost);

          switch (skill.damageType)
          {
              case SkillDamageType.Physical:
                  int rawDmg = DamageCalculator.CalculatePhysical(source, target, powerMultiplier: skill.powerMultiplier);
                  int finalDmg = StatusManager.AbsorbWithShield(target, rawDmg);
                  target.TakeDamage(finalDmg);
                  result.DamageDealt = finalDmg;
                  result.Description = $"{source.CharacterName} attaque {target.CharacterName} pour {finalDmg} dégâts.";
                  break;

              case SkillDamageType.Magical:
                  int magDmg = DamageCalculator.CalculateMagical(source, target, skill.element, powerMultiplier: skill.powerMultiplier);
                  int finalMagDmg = StatusManager.AbsorbWithShield(target, magDmg);
                  target.TakeDamage(finalMagDmg);
                  result.DamageDealt = finalMagDmg;
                  result.Description = $"{source.CharacterName} lance {skill.skillName} sur {target.CharacterName} pour {finalMagDmg} dégâts.";
                  break;

              case SkillDamageType.Healing:
                  int healAmt = DamageCalculator.CalculateHealing(source, skill.powerMultiplier);
                  target.Heal(healAmt);
                  result.HealingDone = healAmt;
                  result.Description = $"{source.CharacterName} soigne {target.CharacterName} de {healAmt} HP.";
                  break;

              case SkillDamageType.Status:
                  if (Random.value < skill.statusChance)
                  {
                      var effect = new StatusEffect(skill.statusEffect, GetDefaultDuration(skill.statusEffect));
                      StatusManager.Apply(target, effect);
                      result.AppliedStatus = effect;
                      result.Description = $"{target.CharacterName} est affecté par {skill.statusEffect} !";
                  }
                  else
                  {
                      result.Description = $"{skill.skillName} sur {target.CharacterName} : raté !";
                  }
                  break;
          }

          result.TargetDied = target.IsDead;
          EventBus.Publish(new ActionResolvedEvent { Result = result });
          return result;
      }

      public static ActionResult ResolveBasicAttack(CharacterData source, CharacterData target)
      {
          var result = new ActionResult { Source = source, Target = target };
          int rawDmg = DamageCalculator.CalculatePhysical(source, target);
          int finalDmg = StatusManager.AbsorbWithShield(target, rawDmg);
          target.TakeDamage(finalDmg);
          result.DamageDealt = finalDmg;
          result.TargetDied = target.IsDead;
          result.Description = $"{source.CharacterName} attaque {target.CharacterName} pour {finalDmg} dégâts.";
          EventBus.Publish(new ActionResolvedEvent { Result = result });
          return result;
      }

      private static int GetDefaultDuration(StatusEffectType type) => type switch
      {
          StatusEffectType.Burn      => 3,
          StatusEffectType.Poison    => 3,
          StatusEffectType.Freeze    => 1,
          StatusEffectType.Paralysis => 2,
          StatusEffectType.Confusion => 2,
          StatusEffectType.Shield    => 999,
          _                          => 2
      };
  }
  ```

- [ ] **Step 2 : Implémenter BattleManager**

  Créer `Assets/Scripts/Combat/BattleManager.cs` :
  ```csharp
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public class BattleManager : MonoBehaviour
  {
      public static BattleManager Instance { get; private set; }

      private List<CharacterData> _playerTeam  = new();
      private List<CharacterData> _enemyTeam   = new();
      private TurnSystem _turnSystem;

      public bool IsPlayerTurn => _playerTeam.Contains(_turnSystem?.CurrentCharacter);
      public CharacterData ActiveCharacter => _turnSystem?.CurrentCharacter;

      private void Awake()
      {
          if (Instance != null) { Destroy(gameObject); return; }
          Instance = this;
      }

      public void StartBattle(List<CharacterData> players, List<CharacterData> enemies)
      {
          _playerTeam = players;
          _enemyTeam  = enemies;

          var allChars = new List<CharacterData>(players);
          allChars.AddRange(enemies);
          _turnSystem = new TurnSystem(allChars);
      }

      public void ExecuteAction(CharacterData target, SkillSO skill = null)
      {
          var source = _turnSystem.CurrentCharacter;

          if (!StatusManager.CanAct(source))
          {
              Debug.Log($"{source.CharacterName} ne peut pas agir ce tour !");
              EndCurrentTurn();
              return;
          }

          // Tick statuts en début de tour actif
          StatusManager.Tick(source);

          ActionResult result;
          if (skill == null)
              result = ActionResolver.ResolveBasicAttack(source, target);
          else
              result = ActionResolver.ResolveSkill(source, target, skill);

          Debug.Log(result.Description);

          if (CheckBattleEnd()) return;
          EndCurrentTurn();
      }

      public void Pass()
      {
          StatusManager.Tick(_turnSystem.CurrentCharacter);
          EndCurrentTurn();
      }

      private void EndCurrentTurn()
      {
          _turnSystem.NextTurn();
          EventBus.Publish(new TurnStartedEvent { Character = _turnSystem.CurrentCharacter });
      }

      private bool CheckBattleEnd()
      {
          bool playersAllDead = _playerTeam.All(c => c.IsDead);
          bool enemiesAllDead = _enemyTeam.All(c => c.IsDead);

          if (playersAllDead || enemiesAllDead)
          {
              EventBus.Publish(new BattleEndedEvent { PlayerWon = enemiesAllDead });
              return true;
          }
          return false;
      }

      public List<CharacterData> GetEnemyTeam() => _enemyTeam;
      public List<CharacterData> GetPlayerTeam() => _playerTeam;
      public List<CharacterData> GetAliveEnemies() => _enemyTeam.Where(c => !c.IsDead).ToList();
      public List<CharacterData> GetAliveAllies()  => _playerTeam.Where(c => !c.IsDead).ToList();
  }
  ```

- [ ] **Step 3 : Créer GameManager**

  Créer `Assets/Scripts/Core/GameManager.cs` :
  ```csharp
  using UnityEngine;

  public class GameManager : MonoBehaviour
  {
      public static GameManager Instance { get; private set; }

      private void Awake()
      {
          if (Instance != null) { Destroy(gameObject); return; }
          Instance = this;
          DontDestroyOnLoad(gameObject);
      }
  }
  ```

- [ ] **Step 4 : Commit**

  ```bash
  git add Assets/Scripts/Combat/ActionResolver.cs Assets/Scripts/Combat/BattleManager.cs Assets/Scripts/Core/GameManager.cs
  git commit -m "feat: add ActionResolver and BattleManager to orchestrate combat"
  ```

---

## Task 10 : UI de combat (BattleHUD)

**Files:**
- Create : `Assets/Scripts/UI/BattleHUD.cs`
- Create : `Assets/Scripts/UI/ActionMenuUI.cs`
- Create : `Assets/Scripts/UI/BattleLog.cs`
- Create : `Assets/Scenes/Battle.unity` (manual)

- [ ] **Step 1 : Créer la scène Battle**

  File → New Scene → choisir "Basic (Built-in)" → sauvegarder sous `Assets/Scenes/Battle.unity`

- [ ] **Step 2 : Créer le Canvas et la hiérarchie UI de base**

  Dans la scène Battle, créer la hiérarchie suivante (via GameObject → UI) :
  ```
  Canvas (Screen Space - Overlay)
  ├── PlayerTeamPanel
  │   ├── CharacterCard_1 (Text: Nom + HP + MP)
  │   ├── CharacterCard_2
  │   └── CharacterCard_3
  ├── EnemyTeamPanel
  │   ├── EnemyCard_1
  │   └── EnemyCard_2
  ├── ActionMenuPanel
  │   ├── AttackButton
  │   ├── SkillsButton
  │   ├── ItemsButton
  │   └── PassButton
  └── BattleLogPanel
      └── LogText (TMP_Text)
  ```

- [ ] **Step 3 : Implémenter BattleHUD**

  Créer `Assets/Scripts/UI/BattleHUD.cs` :
  ```csharp
  using UnityEngine;
  using TMPro;

  public class BattleHUD : MonoBehaviour
  {
      [SerializeField] private TMP_Text[] playerNameTexts;
      [SerializeField] private TMP_Text[] playerHPTexts;
      [SerializeField] private TMP_Text[] playerMPTexts;
      [SerializeField] private TMP_Text[] enemyNameTexts;
      [SerializeField] private TMP_Text[] enemyHPTexts;
      [SerializeField] private TMP_Text activeTurnIndicator;

      private void OnEnable()
      {
          EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
          EventBus.Subscribe<ActionResolvedEvent>(OnActionResolved);
          EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
      }

      private void OnDisable()
      {
          EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
          EventBus.Unsubscribe<ActionResolvedEvent>(OnActionResolved);
          EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
      }

      public void RefreshAll(BattleManager battle)
      {
          var players = battle.GetPlayerTeam();
          var enemies = battle.GetEnemyTeam();

          for (int i = 0; i < players.Count && i < playerNameTexts.Length; i++)
          {
              playerNameTexts[i].text = players[i].CharacterName;
              playerHPTexts[i].text   = $"HP: {players[i].CurrentHP}/{players[i].MaxHP}";
              playerMPTexts[i].text   = $"MP: {players[i].CurrentMP}/{players[i].MaxMP}";
          }

          for (int i = 0; i < enemies.Count && i < enemyNameTexts.Length; i++)
          {
              enemyNameTexts[i].text = enemies[i].CharacterName;
              enemyHPTexts[i].text   = $"HP: {enemies[i].CurrentHP}/{enemies[i].MaxHP}";
          }
      }

      private void OnTurnStarted(TurnStartedEvent e)
      {
          activeTurnIndicator.text = $"Tour de : {e.Character.CharacterName}";
          if (BattleManager.Instance != null)
              RefreshAll(BattleManager.Instance);
      }

      private void OnActionResolved(ActionResolvedEvent e)
      {
          if (BattleManager.Instance != null)
              RefreshAll(BattleManager.Instance);
      }

      private void OnBattleEnded(BattleEndedEvent e)
      {
          activeTurnIndicator.text = e.PlayerWon ? "Victoire !" : "Défaite...";
      }
  }
  ```

- [ ] **Step 4 : Implémenter ActionMenuUI**

  Créer `Assets/Scripts/UI/ActionMenuUI.cs` :
  ```csharp
  using UnityEngine;
  using UnityEngine.UI;

  public class ActionMenuUI : MonoBehaviour
  {
      [SerializeField] private Button attackButton;
      [SerializeField] private Button passButton;
      // Les boutons Skills et Items seront développés dans les plans suivants

      private void Start()
      {
          attackButton.onClick.AddListener(OnAttackPressed);
          passButton.onClick.AddListener(OnPassPressed);
          EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
          EventBus.Subscribe<BattleEndedEvent>(_ => SetMenuActive(false));
      }

      private void OnDestroy()
      {
          EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
      }

      private void OnTurnStarted(TurnStartedEvent e)
      {
          // Afficher le menu seulement pendant les tours joueur
          bool isPlayerTurn = BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn;
          SetMenuActive(isPlayerTurn);
      }

      private void OnAttackPressed()
      {
          // Cibler le premier ennemi vivant (comportement temporaire — sera remplacé par sélection)
          var enemies = BattleManager.Instance.GetAliveEnemies();
          if (enemies.Count > 0)
              BattleManager.Instance.ExecuteAction(enemies[0]);
          SetMenuActive(false);
      }

      private void OnPassPressed()
      {
          BattleManager.Instance.Pass();
          SetMenuActive(false);
      }

      private void SetMenuActive(bool active) => gameObject.SetActive(active);
  }
  ```

- [ ] **Step 5 : Implémenter BattleLog**

  Créer `Assets/Scripts/UI/BattleLog.cs` :
  ```csharp
  using System.Collections.Generic;
  using TMPro;
  using UnityEngine;

  public class BattleLog : MonoBehaviour
  {
      [SerializeField] private TMP_Text logText;
      [SerializeField] private int maxLines = 8;
      private readonly Queue<string> _lines = new();

      private void OnEnable()
      {
          EventBus.Subscribe<ActionResolvedEvent>(OnAction);
          EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
          EventBus.Subscribe<StatusAppliedEvent>(OnStatus);
      }

      private void OnDisable()
      {
          EventBus.Unsubscribe<ActionResolvedEvent>(OnAction);
          EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
          EventBus.Unsubscribe<StatusAppliedEvent>(OnStatus);
      }

      private void Log(string message)
      {
          _lines.Enqueue(message);
          if (_lines.Count > maxLines) _lines.Dequeue();
          logText.text = string.Join("\n", _lines);
      }

      private void OnAction(ActionResolvedEvent e) => Log(e.Result.Description);
      private void OnStatus(StatusAppliedEvent e)  => Log($"{e.Target.CharacterName} : {e.Status.type} !");
      private void OnBattleEnded(BattleEndedEvent e) => Log(e.PlayerWon ? "=== VICTOIRE ===" : "=== DÉFAITE ===");
  }
  ```

- [ ] **Step 6 : Câbler la scène Battle**

  Dans la scène Battle :
  1. Créer un GameObject vide nommé `Managers`
  2. Ajouter les composants : `GameManager`, `BattleManager`
  3. Attacher `BattleHUD`, `ActionMenuUI`, `BattleLog` aux GameObjects UI correspondants
  4. Remplir les références serialisées dans l'Inspecteur

- [ ] **Step 7 : Commit**

  ```bash
  git add Assets/Scripts/UI/ Assets/Scenes/
  git commit -m "feat: add BattleHUD, ActionMenuUI and BattleLog for combat UI"
  ```

---

## Task 11 : ScriptableObjects des 3 classes de test

**Files:**
- Create : `Assets/_Data/Skills/*.asset` (skills de base)
- Create : `Assets/_Data/Races/Humain.asset`
- Create : `Assets/_Data/Classes/Guerrier.asset`
- Create : `Assets/_Data/Classes/Mage.asset`
- Create : `Assets/_Data/Classes/Soigneur.asset`

- [ ] **Step 1 : Créer les skills de base**

  Dans `Assets/_Data/Skills/`, clic droit → Create → RPG → Skill :

  **AttaqueBasique.asset**
  - skillName: "Attaque Basique"
  - damageType: Physical
  - targetType: SingleEnemy
  - element: None
  - mpCost: 0
  - powerMultiplier: 1.0

  **BouleDeFeu.asset**
  - skillName: "Boule de Feu"
  - damageType: Magical
  - element: Fire
  - mpCost: 10
  - powerMultiplier: 1.2
  - statusEffect: Burn, statusChance: 0.2

  **GlaceCristal.asset**
  - skillName: "Glace Cristal"
  - damageType: Magical
  - element: Water
  - mpCost: 12
  - powerMultiplier: 1.3
  - statusEffect: Freeze, statusChance: 0.25

  **SoinBasique.asset**
  - skillName: "Soin"
  - damageType: Healing
  - targetType: SingleAlly
  - mpCost: 8
  - powerMultiplier: 1.5

  **SoinZone.asset**
  - skillName: "Soin de Zone"
  - damageType: Healing
  - targetType: AllAllies
  - mpCost: 20
  - powerMultiplier: 0.8

  **FrappeVigoureuse.asset**
  - skillName: "Frappe Vigoureuse"
  - damageType: Physical
  - mpCost: 6
  - powerMultiplier: 1.8

- [ ] **Step 2 : Créer la race Humain**

  `Assets/_Data/Races/Humain.asset` (Create → RPG → Race) :
  - raceName: "Humain"
  - Tous les modificateurs à 0.0
  - passiveDescription: "+5% XP (géré en Plan 3)"

- [ ] **Step 3 : Créer les 3 classes**

  **Guerrier.asset** (Create → RPG → Class) :
  - className: "Guerrier"
  - role: DPS
  - baseHP: 120, baseMP: 30, baseATK: 14, baseDEF: 12, baseMAG: 5, baseRES: 8, baseAGI: 9, baseLCK: 6
  - hpGrowth: 18, atkGrowth: 3, defGrowth: 2
  - elementalAffinity: None
  - startingSkills: [AttaqueBasique, FrappeVigoureuse]

  **Mage.asset** :
  - className: "Mage"
  - role: DPS
  - baseHP: 70, baseMP: 80, baseATK: 5, baseDEF: 6, baseMAG: 16, baseRES: 12, baseAGI: 11, baseLCK: 8
  - mpGrowth: 12, magGrowth: 4
  - elementalAffinity: Fire
  - startingSkills: [AttaqueBasique, BouleDeFeu, GlaceCristal]

  **Soigneur.asset** :
  - className: "Soigneur"
  - role: Healer
  - baseHP: 90, baseMP: 100, baseATK: 7, baseDEF: 8, baseMAG: 14, baseRES: 14, baseAGI: 8, baseLCK: 7
  - mpGrowth: 15, magGrowth: 3
  - elementalAffinity: Light
  - startingSkills: [AttaqueBasique, SoinBasique, SoinZone]

- [ ] **Step 4 : Créer un BattleBootstrap pour tester en Play Mode**

  Créer `Assets/Scripts/Combat/BattleBootstrap.cs` et l'attacher au GameObject `Managers` dans la scène Battle :
  ```csharp
  using System.Collections.Generic;
  using UnityEngine;

  public class BattleBootstrap : MonoBehaviour
  {
      [SerializeField] private ClassSO playerClass;
      [SerializeField] private RaceSO  playerRace;
      [SerializeField] private ClassSO enemyClass;
      [SerializeField] private RaceSO  enemyRace;
      [SerializeField] private BattleHUD battleHUD;

      private void Start()
      {
          var player = new CharacterData();
          player.InitializeFromSO("Héros", playerClass, playerRace, level: 1);

          var enemy = new CharacterData();
          enemy.InitializeFromSO("Ennemi", enemyClass, enemyRace, level: 1);

          BattleManager.Instance.StartBattle(
              new List<CharacterData> { player },
              new List<CharacterData> { enemy }
          );

          battleHUD.RefreshAll(BattleManager.Instance);
      }
  }
  ```

- [ ] **Step 5 : Tester en Play Mode**

  1. Assigner dans l'Inspecteur : playerClass=Guerrier, playerRace=Humain, enemyClass=Mage, enemyRace=Humain
  2. Appuyer sur Play
  3. Vérifier : noms affichés, HP/MP corrects, bouton "Attaquer" visible
  4. Cliquer "Attaquer" → vérifier que les HP de l'ennemi baissent et le log s'affiche
  5. Vérifier que le tour passe à l'ennemi (bot à implémenter dans Plan 2)

- [ ] **Step 6 : Commit final**

  ```bash
  git add Assets/_Data/ Assets/Scripts/Combat/BattleBootstrap.cs
  git commit -m "feat: add 3 starter classes (Guerrier/Mage/Soigneur) and battle test bootstrap"
  ```

---

## Résultat attendu à la fin du Plan 1

- Un projet Unity compilant sans erreurs
- 20+ tests EditMode passant (vert)
- Une scène Battle jouable en clavier/souris
- Un combat 1v1 fonctionnel : Guerrier vs Mage (ou autre combinaison)
- Système élémentaire actif (Feu, Nature, Eau, Foudre, Lumière, Ténèbres)
- Statuts appliqués et tickés (Brûlure, Poison, Gel, Paralysie, Bouclier)
- Log de combat affichant toutes les actions
- Base solide pour Plan 2 (Arène 1v1 locale + Bots)

---

## Prochain plan : Plan 2 — Arène Locale & Bots

Périmètre du Plan 2 (Roadmap étapes 4-5) :
- InputRouter (manettes + clavier, 2 joueurs locaux)
- Sélection de cible (remplace le ciblage automatique)
- BotBrain Facile & Normal
- Mode Arène 1v1 complet
- Préparation du 3v3 (team selection)
