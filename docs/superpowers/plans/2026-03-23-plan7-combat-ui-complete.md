# Combat UI Complet Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Compléter l'interface de combat avec 5 boutons d'action, consommables, compagnon en combat, couleurs HP/MP, affichage des statuts, et écran Victoire/Défaite.

**Architecture:** Nouveaux composants MonoBehaviour séparés par responsabilité (ItemMenuUI, CompanionMenuUI, VictoryScreenUI, StatusDisplayUI), cohérents avec le pattern EventBus + SkillMenuUI existant. Les changements de données (BattleEndedEvent, Inventory, CharacterData) sont isolés et rétrocompatibles.

**Tech Stack:** Unity 6, C#, TextMeshPro, Unity UI, EventBus (pattern existant), NUnit (EditMode tests via Unity Test Runner)

**Spec:** `docs/superpowers/specs/2026-03-23-combat-ui-complete-design.md`

---

## File Map

| Fichier | Action |
|---------|--------|
| `Assets/Scripts/Data/Enums.cs` | Ajouter `ConsumableEffectType` |
| `Assets/Scripts/Core/GameEvents.cs` | `BattleEndedEvent` struct→class + XP/Loot ; `CompanionActivatedEvent` + Message |
| `Assets/Scripts/Characters/CharacterData.cs` | Ajouter `XPReward`, `SourceLootTable` |
| `Assets/Scripts/Equipment/Inventory.cs` | Ajouter `Consumables` + Add/Remove |
| `Assets/Scripts/Combat/BattleManager.cs` | `CheckBattleEnd()` calcule XP + loot |
| `Assets/Scripts/Combat/BattleCampaignBridge.cs` | Supprimer LoadScene ; ajouter XPReward/SourceLootTable/GainXP |
| `Assets/Scripts/Companions/CompanionSystem.cs` | Passer `Message` dans `CompanionActivatedEvent` |
| `Assets/Scripts/UI/BattleHUD.cs` | Couleurs HP/MP ; `OnBattleEnded` masque indicateur |
| `Assets/Scripts/UI/BattleLog.cs` | S'abonner à `CompanionActivatedEvent` |
| `Assets/Scripts/UI/ActionMenuUI.cs` | Ajouter `companionButton` + `CompanionMenuUI` ; câbler `itemsButton` |
| **Créer** `Assets/Scripts/Data/ConsumableSO.cs` | Nouveau ScriptableObject consommable |
| **Créer** `Assets/Scripts/UI/ItemMenuUI.cs` | Sous-menu objets |
| **Créer** `Assets/Scripts/UI/CompanionMenuUI.cs` | Sous-menu compagnon |
| **Créer** `Assets/Scripts/UI/StatusDisplayUI.cs` | Affichage statuts par combattant |
| **Créer** `Assets/Scripts/UI/VictoryScreenUI.cs` | Écran fin de combat |
| **Créer** `Assets/Tests/EditMode/CombatUIFoundationTests.cs` | Tests EditMode |

---

## Task 1 : Foundation — Enums, GameEvents, ConsumableSO

**Files:**
- Modify: `Assets/Scripts/Data/Enums.cs`
- Modify: `Assets/Scripts/Core/GameEvents.cs`
- Create: `Assets/Scripts/Data/ConsumableSO.cs`

- [ ] **Ajouter `ConsumableEffectType` dans `Enums.cs`** — après la dernière enum du fichier :

```csharp
public enum ConsumableEffectType { HealHP, RestoreMP, CureStatus }
```

- [ ] **Remplacer `BattleEndedEvent` dans `GameEvents.cs`** — changer struct en class et ajouter les champs :

```csharp
// Remplacer :
// public struct BattleEndedEvent    { public bool PlayerWon; }
// Par :
public class BattleEndedEvent
{
    public bool PlayerWon;
    public int XPGained;
    public List<EquipmentSO> Loot;
}
```
Ajouter `using System.Collections.Generic;` en haut si absent.

- [ ] **Ajouter `Message` à `CompanionActivatedEvent`** dans `GameEvents.cs` :

```csharp
// Remplacer :
// public struct CompanionActivatedEvent { public CharacterData Owner; public CompanionSkillSO Skill; public CharacterData Target; }
// Par :
public struct CompanionActivatedEvent
{
    public CharacterData Owner;
    public CompanionSkillSO Skill;
    public CharacterData Target;
    public string Message;
}
```

- [ ] **Créer `ConsumableSO.cs`** :

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "RPG/Consumable")]
public class ConsumableSO : ScriptableObject
{
    [Header("Identité")]
    public string itemName;
    [TextArea] public string description;

    [Header("Effet")]
    public ConsumableEffectType effectType;
    [Tooltip("Pour HealHP : fraction de MaxHP (0.3 = 30%). Pour RestoreMP : valeur fixe.")]
    public float value;
}
```

- [ ] **Vérifier compilation** — ouvrir Unity, s'assurer qu'il n'y a aucune erreur dans la Console avant de continuer.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Data/Enums.cs" \
        "My project/Assets/Scripts/Core/GameEvents.cs" \
        "My project/Assets/Scripts/Data/ConsumableSO.cs" \
        "My project/Assets/Scripts/Data/ConsumableSO.cs.meta"
git commit -m "feat: add ConsumableEffectType, ConsumableSO, enrich BattleEndedEvent and CompanionActivatedEvent"
```

---

## Task 2 : CharacterData + Inventory extensions

**Files:**
- Modify: `Assets/Scripts/Characters/CharacterData.cs`
- Modify: `Assets/Scripts/Equipment/Inventory.cs`
- Create: `Assets/Tests/EditMode/CombatUIFoundationTests.cs`

- [ ] **Ajouter `XPReward` et `SourceLootTable` dans `CharacterData.cs`** — après la ligne `public int Experience`:

```csharp
/// <summary>XP accordé quand cet ennemi est vaincu. 0 pour les personnages joueurs.</summary>
public int XPReward;

/// <summary>Table de loot de cet ennemi. Null pour les personnages joueurs.</summary>
public LootTableSO SourceLootTable;
```

- [ ] **Ajouter `Consumables` dans `Inventory.cs`** — après la propriété `Bag` :

```csharp
/// <summary>Objets consommables (potions, antidotes, etc.).</summary>
public List<ConsumableSO> Consumables { get; } = new List<ConsumableSO>();

public void AddConsumable(ConsumableSO c)
{
    if (c != null) Consumables.Add(c);
}

public void RemoveConsumable(ConsumableSO c) => Consumables.Remove(c);
```

- [ ] **Écrire les tests** dans `Assets/Tests/EditMode/CombatUIFoundationTests.cs` :

```csharp
using NUnit.Framework;
using System.Collections.Generic;

public class CombatUIFoundationTests
{
    // ── CharacterData ──────────────────────────────────────────────────────

    [Test]
    public void CharacterData_XPReward_DefaultsToZero()
    {
        var c = new CharacterData();
        Assert.AreEqual(0, c.XPReward);
    }

    [Test]
    public void CharacterData_SourceLootTable_DefaultsToNull()
    {
        var c = new CharacterData();
        Assert.IsNull(c.SourceLootTable);
    }

    // ── Inventory.Consumables ──────────────────────────────────────────────

    [Test]
    public void Inventory_Consumables_EmptyByDefault()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        Assert.AreEqual(0, c.Inventory.Consumables.Count);
    }

    [Test]
    public void Inventory_AddConsumable_IncreasesCount()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        var potion = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion";
        potion.effectType = ConsumableEffectType.HealHP;
        potion.value = 0.3f;

        c.Inventory.AddConsumable(potion);

        Assert.AreEqual(1, c.Inventory.Consumables.Count);
        UnityEngine.Object.DestroyImmediate(potion);
    }

    [Test]
    public void Inventory_RemoveConsumable_DecreasesCount()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        var potion = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        potion.itemName = "Potion";

        c.Inventory.AddConsumable(potion);
        c.Inventory.RemoveConsumable(potion);

        Assert.AreEqual(0, c.Inventory.Consumables.Count);
        UnityEngine.Object.DestroyImmediate(potion);
    }

    [Test]
    public void Inventory_AddNull_DoesNothing()
    {
        var c = new CharacterData();
        c.Initialize("T", 100, 50, 10, 5, 5, 5, 8, 3);
        c.Inventory.AddConsumable(null);
        Assert.AreEqual(0, c.Inventory.Consumables.Count);
    }

    // ── BattleEndedEvent ──────────────────────────────────────────────────

    [Test]
    public void BattleEndedEvent_IsClass_SupportsNullLoot()
    {
        var evt = new BattleEndedEvent { PlayerWon = true, XPGained = 50, Loot = null };
        Assert.IsTrue(evt.PlayerWon);
        Assert.AreEqual(50, evt.XPGained);
        Assert.IsNull(evt.Loot);
    }

    [Test]
    public void BattleEndedEvent_LootList_CanBePopulated()
    {
        var evt = new BattleEndedEvent
        {
            PlayerWon = true,
            XPGained = 100,
            Loot = new List<EquipmentSO>()
        };
        Assert.AreEqual(0, evt.Loot.Count);
    }
}
```

- [ ] **Lancer les tests** — Window > General > Test Runner > EditMode > Run All
  - Résultat attendu : tous les nouveaux tests passent (verts)

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Characters/CharacterData.cs" \
        "My project/Assets/Scripts/Equipment/Inventory.cs" \
        "My project/Assets/Tests/EditMode/CombatUIFoundationTests.cs" \
        "My project/Assets/Tests/EditMode/CombatUIFoundationTests.cs.meta"
git commit -m "feat: add XPReward/SourceLootTable to CharacterData, Consumables to Inventory"
```

---

## Task 3 : BattleManager — calcul XP + Loot

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`

- [ ] **Ajouter `using System.Linq;`** en haut si absent (déjà présent — vérifier).

- [ ] **Remplacer `CheckBattleEnd()`** dans `BattleManager.cs` :

```csharp
private bool CheckBattleEnd()
{
    bool playersAllDead = _playerTeam.All(c => c.IsDead);
    bool enemiesAllDead = _enemyTeam.All(c => c.IsDead);
    if (!playersAllDead && !enemiesAllDead) return false;

    int xp = 0;
    var loot = new List<EquipmentSO>();

    if (enemiesAllDead)
    {
        foreach (var enemy in _enemyTeam)
        {
            xp += enemy.XPReward;
            if (enemy.SourceLootTable != null)
            {
                var item = enemy.SourceLootTable.Roll();
                if (item != null) loot.Add(item);
            }
        }
    }

    EventBus.Publish(new BattleEndedEvent
    {
        PlayerWon = enemiesAllDead,
        XPGained  = xp,
        Loot      = loot
    });
    return true;
}
```

- [ ] **Vérifier compilation** dans Unity — aucune erreur.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Combat/BattleManager.cs"
git commit -m "feat: BattleManager computes XP and loot on battle end"
```

---

## Task 4 : BattleCampaignBridge — supprimer LoadScene, ajouter XP

**Files:**
- Modify: `Assets/Scripts/Combat/BattleCampaignBridge.cs`

- [ ] **Renseigner `XPReward` et `SourceLootTable`** dans la boucle de construction des ennemis (après `enemy.Initialize(...)`) :

```csharp
enemy.XPReward       = enemySO.xpReward;
enemy.SourceLootTable = enemySO.lootTable;
```

- [ ] **Remplacer le handler `onEnd`** — supprimer les `LoadScene`, ajouter `GainXP` :

```csharp
onEnd = evt =>
{
    EventBus.Unsubscribe<BattleEndedEvent>(onEnd);

    if (onTurn != null)
        EventBus.Unsubscribe<TurnStartedEvent>(onTurn);

    if (evt.PlayerWon)
    {
        if (encounter.IsBoss)
            EventBus.Publish(new BossDefeatedEvent { Player = player });

        if (encounter.IsBoss && encounter.Zone != null
            && !string.IsNullOrEmpty(encounter.Zone.bossDefeatedFlagKey))
            session.Flags.Set(encounter.Zone.bossDefeatedFlagKey);

        player.GainXP(evt.XPGained);
        session.Save();
        // La transition de scène est maintenant gérée par VictoryScreenUI
    }
    // Pas de LoadScene en cas de défaite non plus — VictoryScreenUI s'en charge
};
```

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Combat/BattleCampaignBridge.cs"
git commit -m "feat: BattleCampaignBridge assigns XPReward/SourceLootTable, delegates scene transition to VictoryScreenUI"
```

---

## Task 5 : BattleHUD couleurs + StatusDisplayUI

**Files:**
- Modify: `Assets/Scripts/UI/BattleHUD.cs`
- Create: `Assets/Scripts/UI/StatusDisplayUI.cs`

- [ ] **Créer `StatusDisplayUI.cs`** :

```csharp
using TMPro;
using UnityEngine;

/// <summary>
/// Affiche les effets de statut actifs d'un combattant.
/// Un composant par combattant, placé sur un GameObject enfant du HUD.
/// </summary>
public class StatusDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private CharacterData _character;

    public void Initialize(CharacterData character)
    {
        _character = character;
        Refresh();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TurnStartedEvent>(OnRefresh);
        EventBus.Subscribe<ActionResolvedEvent>(OnRefresh);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnRefresh);
        EventBus.Unsubscribe<ActionResolvedEvent>(OnRefresh);
    }

    private void OnRefresh<T>(T _) => Refresh();

    private void Refresh()
    {
        if (_character == null || statusText == null) return;

        var sb = new System.Text.StringBuilder();
        foreach (var s in _character.ActiveStatuses)
        {
            string entry = s.type switch
            {
                StatusEffectType.Poison    => $"<color=#9B59B6>● Poison ({s.remainingTurns})</color>",
                StatusEffectType.Burn      => $"<color=#E67E22>● Brûlure ({s.remainingTurns})</color>",
                StatusEffectType.Shield    => "<color=#3498DB>● Bouclier</color>",
                StatusEffectType.Paralysis => $"<color=#F1C40F>● Paralysie ({s.remainingTurns})</color>",
                _                          => string.Empty
            };
            if (!string.IsNullOrEmpty(entry))
            {
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(entry);
            }
        }
        statusText.text = sb.ToString();
    }
}
```

- [ ] **Vérifier `StatusEffect`** — confirmer que `StatusEffect` a un champ `remainingTurns` (ou `turns`). Si le champ s'appelle `turns`, adapter le code : `s.turns` au lieu de `s.remainingTurns`.

- [ ] **Mettre à jour `BattleHUD.cs`** — ajouter les refs StatusDisplayUI, la méthode de couleur, et modifier `OnBattleEnded` :

```csharp
using TMPro;
using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text[] playerNameTexts;
    [SerializeField] private TMP_Text[] playerHPTexts;
    [SerializeField] private TMP_Text[] playerMPTexts;
    [SerializeField] private TMP_Text[] enemyNameTexts;
    [SerializeField] private TMP_Text[] enemyHPTexts;
    [SerializeField] private TMP_Text activeTurnIndicator;

    [Header("Status Display")]
    [SerializeField] private StatusDisplayUI[] playerStatusDisplays;
    [SerializeField] private StatusDisplayUI[] enemyStatusDisplays;

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
            SetHPText(playerHPTexts[i], players[i].CurrentHP, players[i].MaxHP);
            SetMPText(playerMPTexts[i], players[i].CurrentMP, players[i].MaxMP);
        }

        for (int i = 0; i < enemies.Count && i < enemyNameTexts.Length; i++)
        {
            enemyNameTexts[i].text = enemies[i].CharacterName;
            SetHPText(enemyHPTexts[i], enemies[i].CurrentHP, enemies[i].MaxHP);
        }
    }

    /// <summary>Appelé une fois au démarrage par BattleCampaignBridge ou BattleBootstrap après StartBattle.</summary>
    public void InitializeStatusDisplays(BattleManager battle)
    {
        var players = battle.GetPlayerTeam();
        var enemies = battle.GetEnemyTeam();

        for (int i = 0; i < players.Count && i < playerStatusDisplays.Length; i++)
            if (playerStatusDisplays[i] != null)
                playerStatusDisplays[i].Initialize(players[i]);

        for (int i = 0; i < enemies.Count && i < enemyStatusDisplays.Length; i++)
            if (enemyStatusDisplays[i] != null)
                enemyStatusDisplays[i].Initialize(enemies[i]);
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (activeTurnIndicator != null)
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
        if (activeTurnIndicator != null)
            activeTurnIndicator.gameObject.SetActive(false);
    }

    private static void SetHPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text = $"HP: {current}/{max}";
        label.color = GetHealthColor(max > 0 ? (float)current / max : 0f);
    }

    private static void SetMPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text = $"MP: {current}/{max}";
        label.color = GetMPColor(max > 0 ? (float)current / max : 0f);
    }

    private static Color GetHealthColor(float ratio)
    {
        if (ratio > 0.5f) return Color.white;
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }

    private static Color GetMPColor(float ratio)
    {
        if (ratio > 0.5f) return new Color(0.2f, 0.6f, 1f);
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }
}
```

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/BattleHUD.cs" \
        "My project/Assets/Scripts/UI/StatusDisplayUI.cs" \
        "My project/Assets/Scripts/UI/StatusDisplayUI.cs.meta"
git commit -m "feat: BattleHUD colored HP/MP text, StatusDisplayUI for active status effects"
```

---

## Task 6 : ItemMenuUI + assets consommables

**Files:**
- Create: `Assets/Scripts/UI/ItemMenuUI.cs`

- [ ] **Créer `ItemMenuUI.cs`** :

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sous-menu des consommables en combat. Même pattern que SkillMenuUI.
/// </summary>
public class ItemMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  itemListContainer;
    [SerializeField] private Button     btnBack;
    [SerializeField] private GameObject itemButtonPrefab;

    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
    private const float ButtonHeight  = 45f;
    private const float ButtonSpacing = 5f;

    private void Start()
    {
        if (btnBack != null) btnBack.onClick.AddListener(Hide);
        Hide();
    }

    public void Show()
    {
        var panelRT = panel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin       = new Vector2(0f, 0.5f);
            panelRT.anchorMax       = new Vector2(0f, 0.5f);
            panelRT.pivot           = new Vector2(0f, 0.5f);
            panelRT.anchoredPosition = new Vector2(10f, 0f);
            panelRT.sizeDelta       = new Vector2(280f, 300f);
        }

        var containerRT = itemListContainer.GetComponent<RectTransform>();
        if (containerRT != null)
        {
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;
        }

        panel.SetActive(true);
        BuildItemList();
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearList();
    }

    private void BuildItemList()
    {
        ClearList();

        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        var consumables = character.Inventory.Consumables;

        if (consumables.Count == 0)
        {
            SpawnButton("Aucun objet", usable: false, onClick: null);
        }
        else
        {
            foreach (var item in consumables)
            {
                bool usable = IsUsable(item, character);
                var captured = item;
                SpawnButton(item.itemName, usable, onClick: () => UseItem(captured, character));
            }
        }

        LayoutButtons();
    }

    private static bool IsUsable(ConsumableSO item, CharacterData c)
    {
        return item.effectType switch
        {
            ConsumableEffectType.HealHP     => c.CurrentHP < c.MaxHP,
            ConsumableEffectType.RestoreMP  => c.CurrentMP < c.MaxMP,
            ConsumableEffectType.CureStatus => c.HasStatus(StatusEffectType.Poison)
                                            || c.HasStatus(StatusEffectType.Burn),
            _ => true
        };
    }

    private void UseItem(ConsumableSO item, CharacterData character)
    {
        switch (item.effectType)
        {
            case ConsumableEffectType.HealHP:
                int heal = Mathf.RoundToInt(character.MaxHP * item.value);
                character.Heal(heal);
                break;
            case ConsumableEffectType.RestoreMP:
                character.RestoreMP(Mathf.RoundToInt(item.value));
                break;
            case ConsumableEffectType.CureStatus:
                character.ActiveStatuses.RemoveAll(
                    s => s.type == StatusEffectType.Poison || s.type == StatusEffectType.Burn);
                break;
        }

        character.Inventory.RemoveConsumable(item);
        Hide();
        BattleManager.Instance?.Pass();
    }

    private void SpawnButton(string label, bool usable, System.Action onClick)
    {
        var go = Instantiate(itemButtonPrefab, itemListContainer);
        _spawnedButtons.Add(go);

        var btn = go.GetComponent<Button>();
        var lbl = go.GetComponentInChildren<TMP_Text>();

        if (lbl != null) lbl.text = label;
        btn.interactable = usable;
        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private void LayoutButtons()
    {
        float yOffset = 10f;
        foreach (var go in _spawnedButtons)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -yOffset);
            rt.sizeDelta        = new Vector2(260f, ButtonHeight);
            yOffset += ButtonHeight + ButtonSpacing;
        }
    }

    private void ClearList()
    {
        foreach (var go in _spawnedButtons)
            if (go != null) Destroy(go);
        _spawnedButtons.Clear();
    }
}
```

- [ ] **Créer les assets consommables dans Unity** (RPG > Create Starter Assets ne les crée pas — faire manuellement) :
  - Dans Project, clic droit `Assets/_Data` → Create folder `Consumables`
  - Clic droit → Create > RPG > Consumable → nommer `PotionDeSoin`
    - itemName : "Potion de Soin", effectType : HealHP, value : 0.3
  - Créer `Antidote` : effectType CureStatus, value 0
  - Créer `Ether` : effectType RestoreMP, value 20

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/ItemMenuUI.cs" \
        "My project/Assets/Scripts/UI/ItemMenuUI.cs.meta"
git commit -m "feat: add ItemMenuUI and ConsumableSO assets"
```

---

## Task 7 : CompanionMenuUI + BattleLog + CompanionSystem

**Files:**
- Create: `Assets/Scripts/UI/CompanionMenuUI.cs`
- Modify: `Assets/Scripts/UI/BattleLog.cs`
- Modify: `Assets/Scripts/Companions/CompanionSystem.cs`

- [ ] **Mettre à jour `CompanionSystem.Execute()`** — passer `result.Message` dans l'event :

```csharp
// Remplacer la ligne existante :
// EventBus.Publish(new CompanionActivatedEvent { Owner = user, Skill = skill, Target = primaryTarget });
// Par :
EventBus.Publish(new CompanionActivatedEvent
{
    Owner   = user,
    Skill   = skill,
    Target  = primaryTarget,
    Message = result.Message
});
```

- [ ] **Mettre à jour `BattleLog.cs`** — ajouter `CompanionActivatedEvent` aux abonnements et ajouter le handler. Conserver `OnBattleEnded` existant tel quel :

```csharp
private void OnEnable()
{
    EventBus.Subscribe<ActionResolvedEvent>(OnAction);
    EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
    EventBus.Subscribe<StatusAppliedEvent>(OnStatus);
    EventBus.Subscribe<CompanionActivatedEvent>(OnCompanion);
}

private void OnDisable()
{
    EventBus.Unsubscribe<ActionResolvedEvent>(OnAction);
    EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
    EventBus.Unsubscribe<StatusAppliedEvent>(OnStatus);
    EventBus.Unsubscribe<CompanionActivatedEvent>(OnCompanion);
}

// Conserver les méthodes existantes telles quelles :
private void OnAction(ActionResolvedEvent e)     => Log(e.Result.Description);
private void OnStatus(StatusAppliedEvent e)      => Log($"{e.Target.CharacterName} : {e.Status.type} !");
private void OnBattleEnded(BattleEndedEvent e)   => Log(e.PlayerWon ? "=== VICTOIRE ===" : "=== DÉFAITE ===");

// Nouveau :
private void OnCompanion(CompanionActivatedEvent e) =>
    Log($"[Compagnon] {e.Skill.skillName} : {e.Message}");
```

- [ ] **Créer `CompanionMenuUI.cs`** :

```csharp
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sous-menu des compétences du compagnon en combat. Même pattern que SkillMenuUI.
/// </summary>
public class CompanionMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  skillListContainer;
    [SerializeField] private Button     btnBack;
    [SerializeField] private GameObject skillButtonPrefab;

    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
    private const float ButtonHeight  = 45f;
    private const float ButtonSpacing = 5f;

    private void Start()
    {
        if (btnBack != null) btnBack.onClick.AddListener(Hide);
        Hide();
    }

    public void Show()
    {
        var panelRT = panel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin       = new Vector2(0f, 0.5f);
            panelRT.anchorMax       = new Vector2(0f, 0.5f);
            panelRT.pivot           = new Vector2(0f, 0.5f);
            panelRT.anchoredPosition = new Vector2(10f, 0f);
            panelRT.sizeDelta       = new Vector2(280f, 300f);
        }

        var containerRT = skillListContainer.GetComponent<RectTransform>();
        if (containerRT != null)
        {
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;
        }

        panel.SetActive(true);
        BuildSkillList();
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearList();
    }

    private void BuildSkillList()
    {
        ClearList();

        var bm        = BattleManager.Instance;
        var character = bm?.ActiveCharacter;
        if (character?.Companion?.Definition == null) return;

        var companion = character.Companion;
        var skills    = companion.Definition.skills;

        if (skills == null || skills.Length == 0)
        {
            SpawnButton("Aucune compétence", usable: false, onClick: null);
        }
        else
        {
            foreach (var skill in skills)
            {
                int cd     = companion.GetCooldown(skill);
                bool usable = cd == 0;
                string label = cd > 0 ? $"{skill.skillName} [CD:{cd}]" : skill.skillName;
                var captured = skill;
                SpawnButton(label, usable, onClick: () => UseSkill(captured, character));
            }
        }

        LayoutButtons();
    }

    private void UseSkill(CompanionSkillSO skill, CharacterData character)
    {
        var bm = BattleManager.Instance;
        if (bm == null) { Hide(); return; }

        var target = ResolveTarget(skill, bm);
        character.UseCompanionSkill(
            skill, target,
            bm.GetAliveAllies().ToArray(),
            bm.GetAliveEnemies().ToArray());

        Hide();
        bm.Pass();
    }

    private static CharacterData ResolveTarget(CompanionSkillSO skill, BattleManager bm)
    {
        return skill.targetType switch
        {
            CompanionTargetType.EnemySingle or CompanionTargetType.AllEnemies =>
                bm.GetAliveEnemies().FirstOrDefault(),
            CompanionTargetType.AllySingle or CompanionTargetType.AllAllies =>
                bm.GetAliveAllies().FirstOrDefault(),
            _ => bm.GetAliveEnemies().FirstOrDefault()
        };
    }

    private void SpawnButton(string label, bool usable, System.Action onClick)
    {
        var go = Instantiate(skillButtonPrefab, skillListContainer);
        _spawnedButtons.Add(go);

        var btn = go.GetComponent<Button>();
        var lbl = go.GetComponentInChildren<TMP_Text>();

        if (lbl != null) lbl.text = label;
        btn.interactable = usable;
        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private void LayoutButtons()
    {
        float yOffset = 10f;
        foreach (var go in _spawnedButtons)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -yOffset);
            rt.sizeDelta        = new Vector2(260f, ButtonHeight);
            yOffset += ButtonHeight + ButtonSpacing;
        }
    }

    private void ClearList()
    {
        foreach (var go in _spawnedButtons)
            if (go != null) Destroy(go);
        _spawnedButtons.Clear();
    }
}
```

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/CompanionMenuUI.cs" \
        "My project/Assets/Scripts/UI/CompanionMenuUI.cs.meta" \
        "My project/Assets/Scripts/UI/BattleLog.cs" \
        "My project/Assets/Scripts/Companions/CompanionSystem.cs"
git commit -m "feat: add CompanionMenuUI, update BattleLog and CompanionSystem for Message field"
```

---

## Task 8 : ActionMenuUI — bouton Compagnon + câblage Objets

**Files:**
- Modify: `Assets/Scripts/UI/ActionMenuUI.cs`

- [ ] **Remplacer `ActionMenuUI.cs`** entier :

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    [SerializeField] private Button     attackButton;
    [SerializeField] private Button     passButton;
    [SerializeField] private Button     skillsButton;
    [SerializeField] private Button     itemsButton;
    [SerializeField] private Button     companionButton;
    [SerializeField] private SkillMenuUI    skillMenu;
    [SerializeField] private ItemMenuUI     itemMenu;
    [SerializeField] private CompanionMenuUI companionMenu;
    [SerializeField] private TMP_Text   playerTurnLabel;

    private System.Action<BattleEndedEvent> _onBattleEnded;

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackPressed);
        passButton.onClick.AddListener(OnPassPressed);
        if (skillsButton   != null) skillsButton.onClick.AddListener(OnSkillsPressed);
        if (itemsButton    != null) itemsButton.onClick.AddListener(OnItemsPressed);
        if (companionButton != null) companionButton.onClick.AddListener(OnCompanionPressed);

        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
        _onBattleEnded = _ => SetMenuActive(false);
        EventBus.Subscribe(_onBattleEnded);

        bool isPlayerTurn = BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn;
        SetMenuActive(isPlayerTurn);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEvent>(OnPlayerTurn);
        EventBus.Unsubscribe(_onBattleEnded);
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        bool isPlayerTurn = BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn;
        SetMenuActive(isPlayerTurn);

        if (isPlayerTurn) RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        if (itemsButton != null)
            itemsButton.interactable = character.Inventory.Consumables.Count > 0;

        if (companionButton != null)
            companionButton.interactable = character.Companion != null;
    }

    private void OnAttackPressed()
    {
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

    private void OnSkillsPressed()
    {
        if (skillMenu != null) skillMenu.Show();
        SetMenuActive(false);
    }

    private void OnItemsPressed()
    {
        if (itemMenu != null) itemMenu.Show();
        SetMenuActive(false);
    }

    private void OnCompanionPressed()
    {
        if (companionMenu != null) companionMenu.Show();
        SetMenuActive(false);
    }

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

    private void SetMenuActive(bool active) => gameObject.SetActive(active);
}
```

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/ActionMenuUI.cs"
git commit -m "feat: ActionMenuUI adds Companion button and wires Items button"
```

---

## Task 9 : VictoryScreenUI

**Files:**
- Create: `Assets/Scripts/UI/VictoryScreenUI.cs`

- [ ] **Créer `VictoryScreenUI.cs`** :

```csharp
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche l'écran de fin de combat (Victoire ou Défaite).
/// Gère la transition de scène via le bouton Retour.
/// </summary>
public class VictoryScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   titleText;
    [SerializeField] private TMP_Text   xpText;
    [SerializeField] private TMP_Text   lootText;
    [SerializeField] private Button     btnReturn;

    [Header("Panels à masquer")]
    [SerializeField] private GameObject actionMenuPanel;
    [SerializeField] private GameObject skillMenuPanel;
    [SerializeField] private GameObject itemMenuPanel;
    [SerializeField] private GameObject companionMenuPanel;

    private bool _playerWon;

    private System.Action<BattleEndedEvent> _onBattleEnded;

    private void Start()
    {
        panel.SetActive(false);
        if (btnReturn != null) btnReturn.onClick.AddListener(OnReturnPressed);

        _onBattleEnded = OnBattleEnded;
        EventBus.Subscribe(_onBattleEnded);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe(_onBattleEnded);
    }

    private void OnBattleEnded(BattleEndedEvent e)
    {
        _playerWon = e.PlayerWon;

        // Masquer les autres panels
        if (actionMenuPanel  != null) actionMenuPanel.SetActive(false);
        if (skillMenuPanel   != null) skillMenuPanel.SetActive(false);
        if (itemMenuPanel    != null) itemMenuPanel.SetActive(false);
        if (companionMenuPanel != null) companionMenuPanel.SetActive(false);

        // Titre
        if (titleText != null)
        {
            titleText.text  = e.PlayerWon ? "VICTOIRE !" : "DÉFAITE...";
            titleText.color = e.PlayerWon
                ? new Color(0.18f, 0.80f, 0.44f) // #2ECC71
                : new Color(0.91f, 0.30f, 0.24f); // #E74C3C
        }

        // XP
        if (xpText != null)
            xpText.text = e.PlayerWon ? $"XP gagné : {e.XPGained}" : string.Empty;

        // Loot
        if (lootText != null)
        {
            if (e.PlayerWon && e.Loot != null && e.Loot.Count > 0)
            {
                var sb = new StringBuilder("Loot :\n");
                foreach (var item in e.Loot)
                    sb.AppendLine($"  - {item.itemName}");
                lootText.text = sb.ToString();
            }
            else
            {
                lootText.text = e.PlayerWon ? "Aucun loot" : string.Empty;
            }
        }

        panel.SetActive(true);
    }

    private void OnReturnPressed()
    {
        string scene = _playerWon ? "WorldMap" : "MainMenu";
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(scene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
```

- [ ] **Vérifier compilation** dans Unity.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/VictoryScreenUI.cs" \
        "My project/Assets/Scripts/UI/VictoryScreenUI.cs.meta"
git commit -m "feat: add VictoryScreenUI with XP, loot display and scene transition"
```

---

## Task 10 : Setup scène Battle (Unity Editor — manuel)

Cette tâche est effectuée directement dans l'éditeur Unity.

### Structure attendue dans Canvas (scène Battle)

```
Canvas
├── HUD                          (BattleHUD component)
│   ├── PlayerStatusDisplay      (StatusDisplayUI component + TMP_Text)
│   └── EnemyStatusDisplay       (StatusDisplayUI component + TMP_Text)
├── ActionMenu                   (ActionMenuUI component)
│   ├── BtnAttaque
│   ├── BtnCompetences
│   ├── BtnObjets
│   ├── BtnCompagnon             ← nouveau bouton à créer
│   └── BtnPasser
├── SkillMenu                    (SkillMenuUI component) — existant
├── ItemMenu                     ← nouveau : même structure que SkillMenu
│   └── Panel
│       ├── ItemListContainer
│       └── BtnBack
├── CompanionMenu                ← nouveau : même structure que SkillMenu
│   └── Panel
│       ├── SkillListContainer
│       └── BtnBack
├── VictoryScreen                ← nouveau Panel
│   ├── TitleText
│   ├── XPText
│   ├── LootText
│   └── BtnReturn
├── BattleLog
└── ArenaHUD
```

### Étapes

- [ ] **HUD — StatusDisplayUI** :
  - Sous `HUD`, créer 2 GameObjects : `PlayerStatusDisplay` et `EnemyStatusDisplay`
  - Ajouter un composant `TMP_Text` sur chacun
  - Ajouter composant `StatusDisplayUI` sur chacun ; assigner le champ `Status Text`
  - Sur le composant `BattleHUD` : assigner `Player Status Displays[0]` → `PlayerStatusDisplay`, `Enemy Status Displays[0]` → `EnemyStatusDisplay`

- [ ] **ActionMenu — BtnCompagnon** :
  - Dupliquer BtnObjets → renommer `BtnCompagnon`, changer le texte en "Compagnon"
  - Sur `ActionMenuUI` : assigner `Companion Button` → `BtnCompagnon`
  - Assigner `Item Menu` → l'objet `ItemMenu` (à créer ci-dessous)
  - Assigner `Companion Menu` → l'objet `CompanionMenu` (à créer ci-dessous)

- [ ] **ItemMenu** :
  - Clic droit sur Canvas → UI > Panel → renommer `ItemMenu`
  - Sous ItemMenu : créer `Panel` → sous Panel : créer `ItemListContainer` (empty) + `BtnBack` (Button TMP)
  - Sur `ItemMenu` : Add Component `ItemMenuUI`
  - Assigner les champs : `Panel`, `Item List Container`, `Btn Back`, `Item Button Prefab` → `SkillButtonPrefab`

- [ ] **CompanionMenu** :
  - Même structure qu'ItemMenu
  - Add Component `CompanionMenuUI`
  - Assigner les champs identiquement

- [ ] **VictoryScreen** :
  - Clic droit sur Canvas → UI > Panel → renommer `VictoryScreen`
  - Centrer : ancres = center/middle, sizeDelta = (400, 300)
  - Ajouter enfants : `TitleText` (TMP), `XPText` (TMP), `LootText` (TMP), `BtnReturn` (Button TMP, texte "Retour")
  - Sur `VictoryScreen` : Add Component `VictoryScreenUI`
  - Assigner : `Panel` → VictoryScreen, `Title Text`, `XP Text`, `Loot Text`, `Btn Return`
  - Assigner les panels à masquer : `Action Menu Panel`, `Skill Menu Panel`, `Item Menu Panel`, `Companion Menu Panel`

- [ ] **Lancer Play, jouer un combat et vérifier** :
  - ActionMenu a 5 boutons visibles
  - Compétences ouvre SkillMenuUI ✓
  - Objets grisé si inventaire vide (normal à ce stade)
  - Compagnon grisé si pas de compagnon assigné (normal)
  - À la fin du combat : VictoryScreen s'affiche avec titre coloré + XP

- [ ] **Commit final**

```bash
git add "My project/Assets/Scenes/Battle.unity"
git commit -m "feat: Battle scene UI setup — ItemMenu, CompanionMenu, VictoryScreen, StatusDisplay"
```
