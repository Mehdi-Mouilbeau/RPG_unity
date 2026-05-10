# Plan 2 — Arène Locale & Bots IA

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter le mode Arène local (1v1 hot-seat), un système d'IA bot (Facile/Normal) avec ActionEvaluator, un système de draft pour le 3v3, et l'InputRouter pour manettes + clavier.

**Architecture:** MatchManager orchestre les parties d'arène au-dessus du BattleManager existant. BotBrain (ScriptableObject configurable) + ActionEvaluator (pure C# statique, testable sans Unity) gèrent l'IA. DraftSystem (pure C#) gère ban/pick avant chaque match 3v3. InputRouter (Unity Input System) assigne les devices aux joueurs.

**Tech Stack:** Unity 6, C#, Unity Input System, NUnit EditMode tests

**Spec de référence :** `docs/superpowers/specs/2026-03-21-rpg-design.md` §2.2, §6.3, §6.4, §6.5

**Périmètre (Roadmap étapes 4–5) :**
- Étape 4 : Arène 1v1 local hot-seat, InputRouter
- Étape 5 : Bots Facile & Normal, mode 3v3, draft

> **Hors périmètre (Plan 3) :** Le système de Compagnons en arène (spec §3.6) est intentionnellement différé au Plan 3 avec le système d'équipement.

---

## Structure des fichiers

**Nouveaux fichiers :**
```
Assets/Scripts/
├── AI/
│   ├── BotAction.cs              ← Données d'une décision bot (target + skill optionnel)
│   ├── ActionEvaluator.cs        ← Logique Easy/Normal (pure C# statique, sans MonoBehaviour)
│   └── BotBrain.cs               ← ScriptableObject de config bot (difficulty, seuils)
├── Arena/
│   ├── MatchManager.cs           ← Orchestre arène 1v1/3v3, gère humains vs bots
│   ├── ArenaRoster.cs            ← ScriptableObject roster de persos prédéfinis
│   └── DraftSystem.cs            ← Phase ban/pick serpent avant le combat 3v3
├── Input/
│   └── InputRouter.cs            ← Assigne devices (clavier/manette) aux joueurs
└── UI/
    └── ArenaHUD.cs               ← Indicateurs P1/P2, écran de résultat

Assets/_Data/
├── AI/
│   ├── EasyBot.asset
│   └── NormalBot.asset
└── Arena/
    └── ArenaRoster.asset

Assets/Tests/EditMode/
├── ActionEvaluatorTests.cs
└── DraftSystemTests.cs
```

**Fichiers modifiés :**
```
Assets/Scripts/Characters/CharacterData.cs    ← Ajouter GetCooldown(SkillSO)
Assets/Scripts/Combat/BattleManager.cs        ← Accepter BotBrain, remplacer EnemyTurnRoutine
Assets/Scripts/Combat/BattleBootstrap.cs      ← Déléguer à MatchManager
Assets/Scripts/Core/GameEvents.cs             ← Ajouter PlayerTurnEvent, DraftStartedEvent
Assets/Scripts/UI/ActionMenuUI.cs             ← Afficher indicateur P1/P2
Assets/Scripts/RPG.Runtime.asmdef            ← Ajouter Unity.InputSystem
Assets/Editor/RPGAssetCreator.cs             ← Générer BotBrain assets + ArenaRoster
```

---

## Task 1 : CharacterData — helper GetCooldown

**Files:**
- Modify : `Assets/Scripts/Characters/CharacterData.cs`

`Cooldowns` est un `Dictionary<string, int>` (clé = skillName). ActionEvaluator en a besoin pour filtrer les skills utilisables.

- [ ] **Step 1 : Ajouter GetCooldown dans CharacterData.cs**

  Ajouter après `RestoreMP` :

  ```csharp
  public int GetCooldown(SkillSO skill) =>
      Cooldowns.TryGetValue(skill.skillName, out int cd) ? cd : 0;
  ```

- [ ] **Step 2 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Characters/CharacterData.cs"
  git commit -m "feat: add GetCooldown(SkillSO) helper to CharacterData"
  ```

---

## Task 2 : BotAction + ActionEvaluator Easy (TDD)

**Files:**
- Create : `Assets/Scripts/AI/BotAction.cs`
- Create : `Assets/Scripts/AI/ActionEvaluator.cs`
- Create : `Assets/Tests/EditMode/ActionEvaluatorTests.cs`

- [ ] **Step 1 : Créer BotAction.cs**

  ```csharp
  // Assets/Scripts/AI/BotAction.cs
  public class BotAction
  {
      public CharacterData Target { get; }
      public SkillSO Skill { get; } // null = attaque basique

      public BotAction(CharacterData target, SkillSO skill = null)
      {
          Target = target;
          Skill  = skill;
      }
  }
  ```

- [ ] **Step 2 : Écrire les tests Easy dans ActionEvaluatorTests.cs**

  ```csharp
  // Assets/Tests/EditMode/ActionEvaluatorTests.cs
  using NUnit.Framework;
  using System.Collections.Generic;

  public class ActionEvaluatorTests
  {
      private CharacterData MakeChar(string name, int hp = 100, int mp = 50,
          int atk = 20, int mag = 15, int agi = 10)
      {
          var c = new CharacterData();
          c.Initialize(name, hp, mp, atk, 10, mag, 10, agi, 5);
          return c;
      }

      [Test]
      public void Easy_ReturnsAction_WhenEnemiesAlive()
      {
          var actor   = MakeChar("Bot");
          var enemies = new List<CharacterData> { MakeChar("E1"), MakeChar("E2") };

          var action = ActionEvaluator.EvaluateEasy(actor, new List<CharacterData> { actor }, enemies);

          Assert.IsNotNull(action);
          Assert.IsNotNull(action.Target);
          Assert.IsFalse(action.Target.IsDead);
      }

      [Test]
      public void Easy_ReturnsNull_WhenAllEnemiesDead()
      {
          var actor = MakeChar("Bot");
          var dead  = MakeChar("Dead");
          dead.TakeDamage(999);

          var action = ActionEvaluator.EvaluateEasy(actor,
              new List<CharacterData> { actor },
              new List<CharacterData> { dead });

          Assert.IsNull(action);
      }

      [Test]
      public void Easy_NeverTargetsDeadEnemy()
      {
          var actor  = MakeChar("Bot");
          var dead   = MakeChar("Dead");  dead.TakeDamage(999);
          var alive  = MakeChar("Alive");
          var enemies = new List<CharacterData> { dead, alive };

          for (int i = 0; i < 30; i++)
          {
              var action = ActionEvaluator.EvaluateEasy(actor,
                  new List<CharacterData> { actor }, enemies);
              Assert.AreEqual(alive, action.Target);
          }
      }
  }
  ```

- [ ] **Step 3 : Vérifier que les tests échouent**
  - Unity : Window → General → Test Runner → EditMode → Run All
  - Attendu : FAIL (ActionEvaluator n'existe pas)

- [ ] **Step 4 : Créer ActionEvaluator.cs avec EvaluateEasy**

  ```csharp
  // Assets/Scripts/AI/ActionEvaluator.cs
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public static class ActionEvaluator
  {
      /// <summary>Easy: cible aléatoire vivante, skill aléatoire 30% du temps.</summary>
      public static BotAction EvaluateEasy(CharacterData actor,
          List<CharacterData> allies, List<CharacterData> enemies)
      {
          var alive = enemies.Where(e => !e.IsDead).ToList();
          if (alive.Count == 0) return null;

          var target = alive[Random.Range(0, alive.Count)];

          var usable = actor.Skills
              .Where(s => s != null
                  && actor.CurrentMP >= s.mpCost
                  && actor.GetCooldown(s) == 0)
              .ToList();

          if (usable.Count > 0 && Random.value < 0.30f)
              return new BotAction(target, usable[Random.Range(0, usable.Count)]);

          return new BotAction(target);
      }
  }
  ```

- [ ] **Step 5 : Vérifier que les tests passent**
  - Test Runner → Run All → PASS

- [ ] **Step 6 : Commit**

  ```bash
  git add "My project/Assets/Scripts/AI/" "My project/Assets/Tests/EditMode/ActionEvaluatorTests.cs"
  git commit -m "feat: add BotAction and ActionEvaluator Easy difficulty"
  ```

---

## Task 3 : ActionEvaluator Normal (TDD)

**Files:**
- Modify : `Assets/Scripts/AI/ActionEvaluator.cs`
- Modify : `Assets/Tests/EditMode/ActionEvaluatorTests.cs`

Normal : soigne allié < 30% HP en priorité, exploite les faiblesses élémentaires, cible le HP le plus bas.

- [ ] **Step 1 : Ajouter les tests Normal dans ActionEvaluatorTests.cs**

  ```csharp
  [Test]
  public void Normal_TargetsLowestHPEnemy()
  {
      var actor   = MakeChar("Bot");
      var weakE   = MakeChar("Weak",   hp: 20);
      var strongE = MakeChar("Strong", hp: 100);
      weakE.TakeDamage(15); // weakE = 5 HP restants

      var enemies = new List<CharacterData> { strongE, weakE };

      for (int i = 0; i < 10; i++)
      {
          var action = ActionEvaluator.EvaluateNormal(actor,
              new List<CharacterData> { actor }, enemies);
          Assert.AreEqual(weakE, action.Target);
      }
  }

  [Test]
  public void Normal_HealsAlly_WhenBelowThirtyPercent()
  {
      var actor = MakeChar("Healer", mp: 100, mag: 20);
      var healSkill = UnityEngine.ScriptableObject.CreateInstance<SkillSO>();
      healSkill.skillName    = "Soin";
      healSkill.damageType   = SkillDamageType.Healing;
      healSkill.targetType   = SkillTargetType.SingleAlly;
      healSkill.mpCost       = 10;
      healSkill.powerMultiplier = 1f;
      actor.Skills.Add(healSkill);

      var dyingAlly = MakeChar("Dying", hp: 100);
      dyingAlly.TakeDamage(75); // 25 HP = 25% < 30%

      var enemy = MakeChar("Enemy");

      var action = ActionEvaluator.EvaluateNormal(actor,
          new List<CharacterData> { actor, dyingAlly },
          new List<CharacterData> { enemy });

      Assert.AreEqual(healSkill, action.Skill);
      Assert.AreEqual(dyingAlly, action.Target);
  }

  [Test]
  public void Normal_ExploitsElementalWeakness()
  {
      var actor = MakeChar("Mage", mp: 100);
      var fireSkill = UnityEngine.ScriptableObject.CreateInstance<SkillSO>();
      fireSkill.skillName    = "BouleDeFeu";
      fireSkill.damageType   = SkillDamageType.Magical;
      fireSkill.element      = ElementType.Fire;
      fireSkill.mpCost       = 10;
      fireSkill.powerMultiplier = 1f;
      actor.Skills.Add(fireSkill);

      var natureEnemy = MakeChar("NatureEnemy");
      natureEnemy.ElementalAffinity_TestOnly = ElementType.Nature; // Feu > Nature

      var action = ActionEvaluator.EvaluateNormal(actor,
          new List<CharacterData> { actor },
          new List<CharacterData> { natureEnemy });

      Assert.AreEqual(fireSkill, action.Skill);
  }
  ```

- [ ] **Step 2 : Vérifier que les nouveaux tests échouent**

- [ ] **Step 3 : Ajouter EvaluateNormal dans ActionEvaluator.cs**

  ```csharp
  /// <summary>Normal: soigne < 30%, exploite élémentaire, cible le + bas HP.</summary>
  public static BotAction EvaluateNormal(CharacterData actor,
      List<CharacterData> allies, List<CharacterData> enemies)
  {
      var aliveEnemies = enemies.Where(e => !e.IsDead).ToList();
      var aliveAllies  = allies.Where(a => !a.IsDead).ToList();
      if (aliveEnemies.Count == 0) return null;

      var usable = actor.Skills
          .Where(s => s != null
              && actor.CurrentMP >= s.mpCost
              && actor.GetCooldown(s) == 0)
          .ToList();

      // Priorité 1 : soigner un allié < 30% HP
      var dyingAlly = aliveAllies
          .FirstOrDefault(a => a != actor && (float)a.CurrentHP / a.MaxHP < 0.30f);
      if (dyingAlly != null)
      {
          var heal = usable.FirstOrDefault(s => s.damageType == SkillDamageType.Healing);
          if (heal != null) return new BotAction(dyingAlly, heal);
      }

      // Priorité 2 : exploiter une faiblesse élémentaire
      foreach (var skill in usable.Where(s => s.damageType == SkillDamageType.Magical))
      {
          var weakTarget = aliveEnemies.FirstOrDefault(e =>
              ElementSystem.GetModifier(skill.element, e.ElementalAffinity) > 1f);
          if (weakTarget != null) return new BotAction(weakTarget, skill);
      }

      // Priorité 3 : cible le HP le plus bas
      var lowestHP = aliveEnemies.OrderBy(e => e.CurrentHP).First();
      return new BotAction(lowestHP);
  }
  ```

- [ ] **Step 4 : Vérifier que tous les tests passent**

- [ ] **Step 5 : Commit**

  ```bash
  git add "My project/Assets/Scripts/AI/ActionEvaluator.cs" "My project/Assets/Tests/EditMode/ActionEvaluatorTests.cs"
  git commit -m "feat: add ActionEvaluator Normal (heal < 30%, elemental, lowest HP)"
  ```

---

## Task 4 : BotBrain ScriptableObject + Assets

**Files:**
- Create : `Assets/Scripts/AI/BotBrain.cs`
- Modify : `Assets/Editor/RPGAssetCreator.cs`

- [ ] **Step 1 : Créer BotBrain.cs**

  ```csharp
  // Assets/Scripts/AI/BotBrain.cs
  using System.Collections.Generic;
  using UnityEngine;

  public enum BotDifficulty { Easy, Normal }

  [CreateAssetMenu(fileName = "NewBot", menuName = "RPG/Bot Brain")]
  public class BotBrain : ScriptableObject
  {
      public BotDifficulty difficulty = BotDifficulty.Easy;

      public BotAction Decide(CharacterData actor,
          List<CharacterData> allies, List<CharacterData> enemies)
      {
          return difficulty switch
          {
              BotDifficulty.Normal => ActionEvaluator.EvaluateNormal(actor, allies, enemies),
              _                    => ActionEvaluator.EvaluateEasy(actor, allies, enemies),
          };
      }
  }
  ```

- [ ] **Step 2 : Ajouter la création des assets dans RPGAssetCreator.cs**

  Dans `CreateFolders()`, ajouter :
  ```csharp
  EnsureFolder("Assets/_Data/AI");
  EnsureFolder("Assets/_Data/Arena");
  ```

  Dans `CreateAllAssets()`, avant `AssetDatabase.SaveAssets()` :
  ```csharp
  CreateBotBrains();
  ```

  Ajouter la méthode :
  ```csharp
  private static void CreateBotBrains()
  {
      string easyPath   = "Assets/_Data/AI/EasyBot.asset";
      string normalPath = "Assets/_Data/AI/NormalBot.asset";

      if (AssetDatabase.LoadAssetAtPath<BotBrain>(easyPath) == null)
      {
          var easy = ScriptableObject.CreateInstance<BotBrain>();
          easy.difficulty = BotDifficulty.Easy;
          AssetDatabase.CreateAsset(easy, easyPath);
      }

      if (AssetDatabase.LoadAssetAtPath<BotBrain>(normalPath) == null)
      {
          var normal = ScriptableObject.CreateInstance<BotBrain>();
          normal.difficulty = BotDifficulty.Normal;
          AssetDatabase.CreateAsset(normal, normalPath);
      }
  }
  ```

- [ ] **Step 3 : Dans Unity — RPG → Create Starter Assets**
  - Vérifie que `Assets/_Data/AI/EasyBot.asset` et `NormalBot.asset` sont créés

- [ ] **Step 4 : Commit**

  ```bash
  git add "My project/Assets/Scripts/AI/BotBrain.cs" "My project/Assets/Editor/RPGAssetCreator.cs"
  git commit -m "feat: add BotBrain ScriptableObject with Easy/Normal, generate assets via RPGAssetCreator"
  ```

---

## Task 5 : Connecter BotBrain à BattleManager

**Files:**
- Modify : `Assets/Scripts/Combat/BattleManager.cs`

Remplacer `EnemyTurnRoutine` (attaque basique hardcodée) par `BotBrain.Decide`.

- [ ] **Step 1 : Ajouter le champ _enemyBotBrain et modifier StartBattle**

  Ajouter le champ privé :
  ```csharp
  private BotBrain _enemyBotBrain;
  ```

  Modifier la signature de `StartBattle` :
  ```csharp
  public void StartBattle(List<CharacterData> players, List<CharacterData> enemies,
      BotBrain enemyBrain = null)
  {
      _playerTeam    = players;
      _enemyTeam     = enemies;
      _enemyBotBrain = enemyBrain;
      var allChars = new List<CharacterData>(players);
      allChars.AddRange(enemies);
      _turnSystem = new TurnSystem(allChars);
  }
  ```

- [ ] **Step 2 : Remplacer EnemyTurnRoutine**

  ```csharp
  private IEnumerator EnemyTurnRoutine()
  {
      yield return new WaitForSeconds(0.8f);

      BotAction action;
      if (_enemyBotBrain != null)
          action = _enemyBotBrain.Decide(ActiveCharacter, _enemyTeam, _playerTeam);
      else
          action = new BotAction(GetAliveAllies().FirstOrDefault());

      if (action?.Target != null)
      {
          if (action.Skill != null) ExecuteAction(action.Target, action.Skill);
          else                      ExecuteAction(action.Target);
      }
      else Pass();
  }
  ```

- [ ] **Step 3 : Modifier BattleBootstrap pour passer un BotBrain**

  Ajouter dans `BattleBootstrap.cs` :
  ```csharp
  [SerializeField] private BotBrain enemyBotBrain;
  ```

  Modifier l'appel dans `Start()` :
  ```csharp
  BattleManager.Instance.StartBattle(
      new List<CharacterData> { player },
      new List<CharacterData> { enemy },
      enemyBotBrain);
  ```

- [ ] **Step 4 : Dans Unity — assigner NormalBot.asset au champ Enemy Bot Brain de BattleBootstrap**

- [ ] **Step 5 : Tester en Play Mode** — l'ennemi exploite les faiblesses élémentaires et cible le joueur à HP bas

- [ ] **Step 6 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Combat/BattleManager.cs" "My project/Assets/Scripts/Combat/BattleBootstrap.cs"
  git commit -m "feat: replace hardcoded enemy AI with BotBrain/ActionEvaluator"
  ```

---

## Task 6 : DraftSystem (TDD)

**Files:**
- Create : `Assets/Scripts/Arena/DraftSystem.cs`
- Create : `Assets/Tests/EditMode/DraftSystemTests.cs`

Draft 3v3 : chaque joueur banne 1 perso, puis pick en serpent : P1, P2, P2, P1, P1, P2 (6 picks au total).

- [ ] **Step 1 : Écrire les tests DraftSystem**

  ```csharp
  // Assets/Tests/EditMode/DraftSystemTests.cs
  using NUnit.Framework;
  using System.Collections.Generic;

  public class DraftSystemTests
  {
      private List<string> Roster(int n)
      {
          var r = new List<string>();
          for (int i = 0; i < n; i++) r.Add($"Char{i}");
          return r;
      }

      [Test]
      public void Ban_RemovesCharacterFromRoster()
      {
          var draft = new DraftSystem(Roster(6));
          draft.Ban(0, "Char0");
          Assert.IsFalse(draft.AvailableRoster.Contains("Char0"));
      }

      [Test]
      public void Ban_UnknownCharacter_ThrowsArgumentException()
      {
          var draft = new DraftSystem(Roster(6));
          Assert.Throws<System.ArgumentException>(() => draft.Ban(0, "Ghost"));
      }

      [Test]
      public void Pick_WrongPlayer_ThrowsInvalidOperationException()
      {
          var draft = new DraftSystem(Roster(8));
          draft.Ban(0, "Char0");
          draft.Ban(1, "Char1");
          // First pick is P1 (snake[0] == 0)
          Assert.Throws<System.InvalidOperationException>(() => draft.Pick(1, "Char2"));
      }

      [Test]
      public void SnakePick_FollowsOrder_0_1_1_0_0_1()
      {
          var draft = new DraftSystem(Roster(8));
          draft.Ban(0, "Char0");
          draft.Ban(1, "Char1");

          int[] expected = { 0, 1, 1, 0, 0, 1 };
          var chars = new[] { "Char2","Char3","Char4","Char5","Char6","Char7" };

          for (int i = 0; i < 6; i++)
          {
              Assert.AreEqual(expected[i], draft.CurrentPickPlayer);
              draft.Pick(expected[i], chars[i]);
          }
      }

      [Test]
      public void Draft_IsComplete_AfterSixPicks()
      {
          var draft = new DraftSystem(Roster(8));
          draft.Ban(0, "Char0");
          draft.Ban(1, "Char1");
          var order = new[] { 0, 1, 1, 0, 0, 1 };
          var chars = new[] { "Char2","Char3","Char4","Char5","Char6","Char7" };
          for (int i = 0; i < 6; i++) draft.Pick(order[i], chars[i]);

          Assert.IsTrue(draft.IsComplete);
          Assert.AreEqual(3, draft.GetTeam(0).Count);
          Assert.AreEqual(3, draft.GetTeam(1).Count);
      }
  }
  ```

- [ ] **Step 2 : Vérifier que les tests échouent** (DraftSystem n'existe pas)

- [ ] **Step 3 : Créer DraftSystem.cs**

  ```csharp
  // Assets/Scripts/Arena/DraftSystem.cs
  using System;
  using System.Collections.Generic;

  public class DraftSystem
  {
      // Ordre serpent 3v3 : P1, P2, P2, P1, P1, P2
      private static readonly int[] SnakeOrder = { 0, 1, 1, 0, 0, 1 };

      public List<string> AvailableRoster { get; }
      public bool IsComplete => _pickIndex >= SnakeOrder.Length;
      public int  CurrentPickPlayer => IsComplete ? -1 : SnakeOrder[_pickIndex];

      public event Action<int, string> OnPick;

      private readonly Dictionary<int, List<string>> _teams = new()
          { { 0, new List<string>() }, { 1, new List<string>() } };
      private int _pickIndex;

      public DraftSystem(List<string> roster)
      {
          AvailableRoster = new List<string>(roster);
      }

      public void Ban(int playerIndex, string characterName)
      {
          if (!AvailableRoster.Contains(characterName))
              throw new ArgumentException($"'{characterName}' introuvable dans le roster");
          AvailableRoster.Remove(characterName);
      }

      public void Pick(int playerIndex, string characterName)
      {
          if (IsComplete)
              throw new InvalidOperationException("Le draft est terminé");
          if (SnakeOrder[_pickIndex] != playerIndex)
              throw new InvalidOperationException($"C'est au joueur {SnakeOrder[_pickIndex]} de pick");
          if (!AvailableRoster.Contains(characterName))
              throw new ArgumentException($"'{characterName}' non disponible");

          AvailableRoster.Remove(characterName);
          _teams[playerIndex].Add(characterName);
          _pickIndex++;
          OnPick?.Invoke(playerIndex, characterName);
      }

      public List<string> GetTeam(int playerIndex) => _teams[playerIndex];
  }
  ```

- [ ] **Step 4 : Vérifier que les tests passent**

- [ ] **Step 5 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Arena/DraftSystem.cs" "My project/Assets/Tests/EditMode/DraftSystemTests.cs"
  git commit -m "feat: add DraftSystem with ban/snake-pick for 3v3 arena"
  ```

---

## Task 7 : GameEvents — PlayerTurnEvent

**Files:**
- Modify : `Assets/Scripts/Core/GameEvents.cs`

- [ ] **Step 1 : Ajouter PlayerTurnEvent dans GameEvents.cs**

  ```csharp
  public struct PlayerTurnEvent
  {
      public int PlayerIndex;      // 0 = P1, 1 = P2, -1 = bot
      public CharacterData Character;
  }
  ```

- [ ] **Step 2 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Core/GameEvents.cs"
  git commit -m "feat: add PlayerTurnEvent to GameEvents"
  ```

---

## Task 8 : MatchManager (arène 1v1 hot-seat)

**Files:**
- Create : `Assets/Scripts/Arena/MatchManager.cs`
- Modify : `Assets/Scripts/UI/ActionMenuUI.cs`
- Modify : `Assets/Scripts/Combat/BattleBootstrap.cs`

MatchManager sait quelles équipes sont contrôlées par P1, P2, ou un bot. Il publie `PlayerTurnEvent` à chaque changement de tour.

- [ ] **Step 1 : Créer MatchManager.cs**

  ```csharp
  // Assets/Scripts/Arena/MatchManager.cs
  using System.Collections.Generic;
  using UnityEngine;

  public class MatchManager : MonoBehaviour
  {
      public static MatchManager Instance { get; private set; }

      [SerializeField] private BotBrain defaultBotBrain;

      private List<CharacterData> _team0;
      private List<CharacterData> _team1;
      private bool _team1IsBot;

      private void Awake()
      {
          if (Instance != null) { Destroy(gameObject); return; }
          Instance = this;
          _team0 = new List<CharacterData>();
          _team1 = new List<CharacterData>();
          EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
      }

      private void OnDestroy() =>
          EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);

      /// <summary>Lance une arène entre deux équipes. team1IsBot = true si P2 est un bot.</summary>
      public void StartArena(List<CharacterData> team0, List<CharacterData> team1,
          bool team1IsBot = false, BotBrain botBrain = null)
      {
          _team0      = team0;
          _team1      = team1;
          _team1IsBot = team1IsBot;

          BattleManager.Instance.StartBattle(team0, team1,
              team1IsBot ? (botBrain ?? defaultBotBrain) : null);
      }

      /// <returns>0 = P1, 1 = P2, -1 = bot</returns>
      public int GetPlayerIndex(CharacterData character)
      {
          if (_team0 != null && _team0.Contains(character)) return 0;
          if (_team1 != null && _team1.Contains(character)) return _team1IsBot ? -1 : 1;
          return -1;
      }

      private void OnTurnStarted(TurnStartedEvent e)
      {
          int idx = GetPlayerIndex(e.Character);
          EventBus.Publish(new PlayerTurnEvent { PlayerIndex = idx, Character = e.Character });
      }
  }
  ```

- [ ] **Step 2 : Modifier ActionMenuUI.cs pour afficher P1/P2**

  Ajouter le champ sérialisé :
  ```csharp
  [SerializeField] private TMP_Text playerTurnLabel; // ex. "JOUEUR 1" ou "JOUEUR 2"
  ```

  Dans `Start()`, ajouter l'abonnement après les existants :
  ```csharp
  EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
  ```

  Dans `OnDestroy()`, ajouter :
  ```csharp
  EventBus.Unsubscribe<PlayerTurnEvent>(OnPlayerTurn);
  ```

  Ajouter la méthode :
  ```csharp
  private void OnPlayerTurn(PlayerTurnEvent e)
  {
      if (playerTurnLabel == null) return;
      playerTurnLabel.text = e.PlayerIndex switch
      {
          0 => "JOUEUR 1",
          1 => "JOUEUR 2",
          _ => ""
      };
  }
  ```

- [ ] **Step 3 : Modifier BattleBootstrap.cs pour utiliser MatchManager**

  Remplacer le contenu de `Start()` :
  ```csharp
  private void Start()
  {
      var player = new CharacterData();
      player.InitializeFromSO("Héros", playerClass, playerRace, level: 1);

      var enemy = new CharacterData();
      enemy.InitializeFromSO("Ennemi", enemyClass, enemyRace, level: 1);

      if (MatchManager.Instance != null)
          MatchManager.Instance.StartArena(
              new List<CharacterData> { player },
              new List<CharacterData> { enemy },
              team1IsBot: true,
              botBrain: enemyBotBrain);
      else
          BattleManager.Instance.StartBattle(
              new List<CharacterData> { player },
              new List<CharacterData> { enemy },
              enemyBotBrain);
  }
  ```

- [ ] **Step 4 : Dans Unity — configurer la scène**
  - Créer un GameObject `MatchManager` → composant `MatchManager`
  - Assigner `NormalBot.asset` au champ `Default Bot Brain`
  - Créer un `TextMeshPro - Text` nommé `TurnLabel` dans le Canvas
  - Dans l'Inspector de `ActionMenu` → assigner `TurnLabel` au champ `Player Turn Label`

- [ ] **Step 5 : Tester en Play Mode**
  - "JOUEUR 1" apparaît sur le tour du joueur
  - Rien (ou vide) sur le tour de l'ennemi

- [ ] **Step 6 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Arena/MatchManager.cs" "My project/Assets/Scripts/UI/ActionMenuUI.cs" "My project/Assets/Scripts/Combat/BattleBootstrap.cs"
  git commit -m "feat: add MatchManager, hot-seat P1/P2 indicator via PlayerTurnEvent"
  ```

---

## Task 9 : InputRouter

**Files:**
- Modify : `Assets/Scripts/RPG.Runtime.asmdef`
- Create : `Assets/Scripts/Input/InputRouter.cs`

InputRouter détecte les devices connectés et les assigne aux joueurs. P1 = clavier, P2 = première manette détectée (ou clavier si pas de manette).

- [ ] **Step 1 : Vérifier l'asmdef**

  `Assets/Scripts/RPG.Runtime.asmdef` doit contenir dans `references` :
  ```json
  "Unity.TextMeshPro",
  "Unity.InputSystem"
  ```
  Si `Unity.InputSystem` manque, l'ajouter.

- [ ] **Step 2 : Créer InputRouter.cs**

  ```csharp
  // Assets/Scripts/Input/InputRouter.cs
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.InputSystem;

  public class InputRouter : MonoBehaviour
  {
      public static InputRouter Instance { get; private set; }

      private readonly Dictionary<int, InputDevice> _devices = new();

      private void Awake()
      {
          if (Instance != null) { Destroy(gameObject); return; }
          Instance = this;
          AssignDefaults();
      }

      private void AssignDefaults()
      {
          // P1 = clavier (toujours présent)
          if (Keyboard.current != null)
              _devices[0] = Keyboard.current;

          // P2 = première manette connectée ; sinon clavier partagé
          if (Gamepad.all.Count > 0)
              _devices[1] = Gamepad.all[0];
          else if (Keyboard.current != null)
              _devices[1] = Keyboard.current; // hot-seat clavier partagé
      }

      /// <summary>Retourne le device assigné au joueur donné (null si non trouvé).</summary>
      public InputDevice GetDevice(int playerIndex) =>
          _devices.TryGetValue(playerIndex, out var d) ? d : null;

      /// <returns>True si les deux joueurs partagent le même device (hot-seat pur).</returns>
      public bool IsSharedKeyboard =>
          _devices.TryGetValue(0, out var d0) &&
          _devices.TryGetValue(1, out var d1) &&
          d0 == d1;
  }
  ```

- [ ] **Step 3 : Ajouter InputRouter à la scène**
  - Créer un GameObject `InputRouter` → composant `InputRouter`

- [ ] **Step 4 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Input/InputRouter.cs" "My project/Assets/Scripts/RPG.Runtime.asmdef"
  git commit -m "feat: add InputRouter for hot-seat device assignment (keyboard + gamepad)"
  ```

---

## Task 10 : ArenaRoster + 3v3 dans MatchManager

**Files:**
- Create : `Assets/Scripts/Arena/ArenaRoster.cs`
- Modify : `Assets/Scripts/Arena/MatchManager.cs`
- Modify : `Assets/Editor/RPGAssetCreator.cs`

ArenaRoster est un SO qui liste les personnages disponibles pour l'arène. MatchManager l'utilise pour instancier les équipes après le draft.

- [ ] **Step 1 : Créer ArenaRoster.cs**

  ```csharp
  // Assets/Scripts/Arena/ArenaRoster.cs
  using System.Collections.Generic;
  using UnityEngine;

  [CreateAssetMenu(fileName = "ArenaRoster", menuName = "RPG/Arena Roster")]
  public class ArenaRoster : ScriptableObject
  {
      [System.Serializable]
      public class RosterEntry
      {
          public string  displayName;
          public ClassSO characterClass;
          public RaceSO  characterRace;
      }

      public List<RosterEntry> entries = new();

      public List<string> GetNames() => entries.ConvertAll(e => e.displayName);

      public CharacterData CreateCharacter(string displayName, int level = 5)
      {
          var entry = entries.Find(e => e.displayName == displayName);
          if (entry == null)
          {
              Debug.LogWarning($"ArenaRoster: '{displayName}' introuvable");
              return null;
          }
          var c = new CharacterData();
          c.InitializeFromSO(displayName, entry.characterClass, entry.characterRace, level);
          return c;
      }
  }
  ```

- [ ] **Step 2 : Étendre MatchManager pour le 3v3 + DraftSystem**

  Ajouter dans MatchManager :

  ```csharp
  [SerializeField] private ArenaRoster arenaRoster;

  private DraftSystem _draft;

  /// <summary>Initialise un draft 3v3 et publie DraftStartedEvent pour l'UI.</summary>
  public void StartDraft()
  {
      if (arenaRoster == null) { Debug.LogError("ArenaRoster non assigné !"); return; }
      _draft = new DraftSystem(arenaRoster.GetNames());
  }

  public DraftSystem GetDraft() => _draft;

  /// <summary>Lance le combat 3v3 après la fin du draft.</summary>
  public void StartArena3v3(bool team1IsBot = false, BotBrain botBrain = null)
  {
      if (_draft == null || !_draft.IsComplete)
      {
          Debug.LogError("Le draft n'est pas terminé !");
          return;
      }
      var team0 = _draft.GetTeam(0).ConvertAll(n => arenaRoster.CreateCharacter(n));
      var team1 = _draft.GetTeam(1).ConvertAll(n => arenaRoster.CreateCharacter(n));
      team0.RemoveAll(c => c == null);
      team1.RemoveAll(c => c == null);
      if (team0.Count == 0 || team1.Count == 0)
      {
          Debug.LogError("Équipes invalides après le draft — vérifiez l'ArenaRoster");
          return;
      }
      StartArena(team0, team1, team1IsBot, botBrain);
  }
  ```

- [ ] **Step 3 : Ajouter la création de l'ArenaRoster dans RPGAssetCreator.cs**

  Dans `CreateAllAssets()`, ajouter :
  ```csharp
  CreateArenaRoster();
  ```

  Ajouter la méthode :
  ```csharp
  private static void CreateArenaRoster()
  {
      string path = "Assets/_Data/Arena/ArenaRoster.asset";
      if (AssetDatabase.LoadAssetAtPath<ArenaRoster>(path) != null) return;

      var roster = ScriptableObject.CreateInstance<ArenaRoster>();
      roster.entries.Add(new ArenaRoster.RosterEntry {
          displayName    = "Guerrier Humain",
          characterClass = Load<ClassSO>("Assets/_Data/Classes/Guerrier.asset"),
          characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
      });
      roster.entries.Add(new ArenaRoster.RosterEntry {
          displayName    = "Mage Humain",
          characterClass = Load<ClassSO>("Assets/_Data/Classes/Mage.asset"),
          characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
      });
      roster.entries.Add(new ArenaRoster.RosterEntry {
          displayName    = "Soigneur Humain",
          characterClass = Load<ClassSO>("Assets/_Data/Classes/Soigneur.asset"),
          characterRace  = Load<RaceSO>("Assets/_Data/Races/Humain.asset")
      });
      AssetDatabase.CreateAsset(roster, path);
  }
  ```

- [ ] **Step 4 : Dans Unity — RPG → Create Starter Assets**
  - Vérifie que `Assets/_Data/Arena/ArenaRoster.asset` est créé

- [ ] **Step 5 : Dans Unity — assigner ArenaRoster.asset au champ Arena Roster de MatchManager**

- [ ] **Step 6 : Commit**

  ```bash
  git add "My project/Assets/Scripts/Arena/" "My project/Assets/Editor/RPGAssetCreator.cs"
  git commit -m "feat: add ArenaRoster SO, 3v3 via DraftSystem, MatchManager.StartArena3v3"
  ```

---

## Task 11 : ArenaHUD (indicateurs P1/P2, écran de résultat)

**Files:**
- Create : `Assets/Scripts/UI/ArenaHUD.cs`

- [ ] **Step 1 : Créer ArenaHUD.cs**

  ```csharp
  // Assets/Scripts/UI/ArenaHUD.cs
  using UnityEngine;
  using TMPro;

  public class ArenaHUD : MonoBehaviour
  {
      [SerializeField] private TMP_Text p1ActiveLabel;  // "► JOUEUR 1"
      [SerializeField] private TMP_Text p2ActiveLabel;  // "► JOUEUR 2"
      [SerializeField] private GameObject resultPanel;
      [SerializeField] private TMP_Text  resultText;

      private void OnEnable()
      {
          EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
          EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
      }

      private void OnDisable()
      {
          EventBus.Unsubscribe<PlayerTurnEvent>(OnPlayerTurn);
          EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
      }

      private void Start()
      {
          if (resultPanel) resultPanel.SetActive(false);
          SetActivePlayer(-1);
      }

      private void OnPlayerTurn(PlayerTurnEvent e) => SetActivePlayer(e.PlayerIndex);

      private void OnBattleEnded(BattleEndedEvent e)
      {
          SetActivePlayer(-1);
          if (resultPanel) resultPanel.SetActive(true);
          if (resultText)
              resultText.text = e.PlayerWon ? "JOUEUR 1 GAGNE !" : "JOUEUR 2 GAGNE !";
      }

      private void SetActivePlayer(int playerIndex)
      {
          if (p1ActiveLabel) p1ActiveLabel.gameObject.SetActive(playerIndex == 0);
          if (p2ActiveLabel) p2ActiveLabel.gameObject.SetActive(playerIndex == 1);
      }
  }
  ```

- [ ] **Step 2 : Dans Unity — configurer ArenaHUD dans la scène**
  - Créer un GameObject `ArenaHUD` dans le Canvas → composant `ArenaHUD`
  - Créer `TMP_Text` nommé `P1Label` (texte : "► JOUEUR 1") → assigner à `P1 Active Label`
  - Créer `TMP_Text` nommé `P2Label` (texte : "► JOUEUR 2") → assigner à `P2 Active Label`
  - Créer un Panel `ResultPanel` désactivé avec un `TMP_Text` `ResultText` → assigner les deux

- [ ] **Step 3 : Tester en Play Mode**
  - "► JOUEUR 1" s'allume sur le tour du joueur
  - Disparaît sur le tour de l'ennemi
  - ResultPanel s'affiche avec le bon message en fin de combat

- [ ] **Step 4 : Commit**

  ```bash
  git add "My project/Assets/Scripts/UI/ArenaHUD.cs"
  git commit -m "feat: add ArenaHUD with P1/P2 active indicators and result screen"
  ```

---

## Récapitulatif des commits du Plan 2

```
feat: add GetCooldown(SkillSO) helper to CharacterData
feat: add BotAction and ActionEvaluator Easy difficulty
feat: add ActionEvaluator Normal (heal < 30%, elemental, lowest HP)
feat: add BotBrain ScriptableObject with Easy/Normal, generate assets via RPGAssetCreator
feat: replace hardcoded enemy AI with BotBrain/ActionEvaluator
feat: add DraftSystem with ban/snake-pick for 3v3 arena
feat: add PlayerTurnEvent to GameEvents
feat: add MatchManager, hot-seat P1/P2 indicator via PlayerTurnEvent
feat: add InputRouter for hot-seat device assignment (keyboard + gamepad)
feat: add ArenaRoster SO, 3v3 via DraftSystem, MatchManager.StartArena3v3
feat: add ArenaHUD with P1/P2 active indicators and result screen
```

**Livrable final :** Arène 1v1 jouable en hot-seat avec indicateurs P1/P2, IA bot Facile/Normal fonctionnelle, DraftSystem 3v3 testable, ArenaRoster avec 3 personnages prédéfinis.
