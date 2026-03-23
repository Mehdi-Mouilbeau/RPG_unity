# Combat UI Complet — Design Spec
Date: 2026-03-23

## Contexte

L'interface de combat actuelle est fonctionnelle mais incomplète :
- `BattleHUD` affiche HP/MP en texte brut sans retour visuel
- `ActionMenuUI` n'a que 3 boutons actifs (Attaque, Compétences, Passer) ; `itemsButton` existe déjà mais n'est pas câblé
- Pas d'accès aux objets ni au compagnon en combat
- Pas d'écran Victoire/Défaite
- Pas d'affichage des effets de statut actifs

## Architecture

Nouveaux composants MonoBehaviour séparés par responsabilité, cohérents avec le pattern EventBus existant.

---

## 1. ActionMenuUI — Refonte

**5 boutons :** Attaque · Compétences · Objets · Compagnon · Passer

- `attackButton`, `passButton`, `skillsButton`, `itemsButton` existent déjà — seul `companionButton` + `companionMenu` (ref `CompanionMenuUI`) sont à ajouter
- **Compagnon** : grisé si `ActiveCharacter.Companion == null`
- **Objets** : grisé si `ActiveCharacter.Inventory.Consumables.Count == 0`
- Les sous-menus masquent l'ActionMenu pendant leur affichage, le réaffichent à la fermeture

---

## 2. ConsumableSO + ItemMenuUI

### Nouveau ScriptableObject : ConsumableSO

```csharp
[CreateAssetMenu(menuName = "RPG/Consumable")]
public class ConsumableSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public ConsumableEffectType effectType;
    public float value; // % pour HealHP (0.3 = 30%), valeur fixe pour RestoreMP
}

public enum ConsumableEffectType { HealHP, RestoreMP, CureStatus }
```

`ConsumableEffectType` est ajouté dans `Enums.cs`.

### Inventory — ajout

```csharp
public List<ConsumableSO> Consumables { get; } = new List<ConsumableSO>();
public void AddConsumable(ConsumableSO c) => Consumables.Add(c);
public void RemoveConsumable(ConsumableSO c) => Consumables.Remove(c);
```

### Assets à créer

| Asset | effectType | value |
|-------|-----------|-------|
| Potion de Soin | HealHP | 0.3 (30% MaxHP) |
| Antidote | CureStatus | 0 |
| Éther | RestoreMP | 20 |

### ItemMenuUI — comportement

- Même pattern que `SkillMenuUI` (LayoutButtons, Show/Hide)
- Bouton grisé si non applicable :
  - `HealHP` : grisé si `CurrentHP == MaxHP`
  - `RestoreMP` : grisé si `CurrentMP == MaxMP`
  - `CureStatus` : grisé si aucun statut `Poison` ou `Burn` actif (choix intentionnel — seuls ces deux statuts sont curables par l'Antidote)
- Cliquer → applique l'effet → `Inventory.RemoveConsumable(c)` → cache panel → `BattleManager.Instance.Pass()` pour finir le tour
- `Pass()` appelle `StatusManager.Tick()` — comportement intentionnel, cohérent avec les autres actions

---

## 3. CompanionMenuUI

**Nouveau composant**, même pattern que `SkillMenuUI`.

### Comportement

- Affiche `ActiveCharacter.Companion.Definition.skills` (tableau `CompanionSkillSO[]`)
- Format bouton : `NomSkill` + `[CD:N]` si `ActiveCharacter.Companion.GetCooldown(skill) > 0`
- Bouton grisé si en cooldown
- Bouton **Retour** identique à SkillMenuUI
- Après l'action : `BattleManager.Instance.Pass()` pour consommer le tour (comportement intentionnel — la compétence compagnon = action complète, donc StatusManager.Tick s'applique comme pour n'importe quelle autre action)

### Événement

`CompanionActivatedEvent` existe déjà — ajouter un champ `string Message` pour affichage dans le BattleLog :

```csharp
// GameEvents.cs — extension de l'existant
public struct CompanionActivatedEvent
{
    public CharacterData Owner;
    public CompanionSkillSO Skill;
    public CharacterData Target;
    public string Message; // nouveau champ
}
```

`BattleLog` s'abonne à `CompanionActivatedEvent` et affiche `e.Message`.

### Résolution de cible (`CompanionTargetType`)

| targetType | Cible résolue |
|-----------|--------------|
| `EnemySingle` | `BattleManager.Instance.GetAliveEnemies().FirstOrDefault()` |
| `AllEnemies` | idem |
| `AllySingle` | `BattleManager.Instance.GetAliveAllies().FirstOrDefault()` |
| `AllAllies` | idem |

Appel :
```csharp
var result = character.UseCompanionSkill(
    skill, target,
    BattleManager.Instance.GetAliveAllies().ToArray(),
    BattleManager.Instance.GetAliveEnemies().ToArray()
);
EventBus.Publish(new CompanionActivatedEvent {
    Owner = character, Skill = skill, Target = target, Message = result.Message
});
```

---

## 4. BattleHUD — Améliorations

### Texte HP/MP coloré

`BattleHUD.RefreshAll()` applique des couleurs TMP en fonction du ratio :

**HP :**
- > 50% → `Color.white`
- 25–50% → `Color.yellow`
- < 25% → `Color.red`

**MP :**
- > 50% → `new Color(0.2f, 0.6f, 1f)` (bleu clair)
- 25–50% → `Color.yellow`
- < 25% → `Color.red`

### BattleHUD.OnBattleEnded

Le texte `activeTurnIndicator` dans `OnBattleEnded` est **supprimé** (le texte "Victoire/Défaite" est désormais géré exclusivement par `VictoryScreenUI`). `OnBattleEnded` se contente de masquer l'indicateur de tour : `activeTurnIndicator.gameObject.SetActive(false)`.

### StatusDisplayUI

**Nouveau composant MonoBehaviour**, un par combattant sur un GameObject enfant du HUD. Lifecycle géré par `OnDestroy`.

- Reçoit `CharacterData character` via `Initialize(CharacterData c)` appelé par `BattleHUD`
- S'abonne à `TurnStartedEvent` et `ActionResolvedEvent` dans `OnEnable`, se désabonne dans `OnDisable`
- Rafraîchit inconditionnellement sur `ActionResolvedEvent` (simple, suffisant pour ce stade)
- Affiche rich text TMP :
  - `<color=#9B59B6>● Poison (N)</color>`
  - `<color=#E67E22>● Brûlure (N)</color>`
  - `<color=#3498DB>● Bouclier</color>`
  - `<color=#F1C40F>● Paralysie (N)</color>`
- Vide si aucun statut actif

---

## 5. VictoryScreenUI

**Nouveau composant** sur un panel centré, caché par défaut.

### Résolution du conflit avec BattleCampaignBridge

`BattleCampaignBridge` **supprime** ses appels `SceneLoader.LoadScene()`. Il conserve uniquement : unsubscribe, flags boss, `player.GainXP(evt.XPGained)`, `session.Save()`.

`VictoryScreenUI` prend en charge la transition :
- Victoire → bouton **Retour** → `SceneLoader.Instance.LoadScene("WorldMap")`
- Défaite → bouton **Retour** → `SceneLoader.Instance.LoadScene("MainMenu")`

### Attribution de l'XP

`BattleCampaignBridge` appelle `player.GainXP(evt.XPGained)` dans son handler `BattleEndedEvent` (bloc `PlayerWon`), avant `session.Save()`.

### Contenu du panel

- Titre **VICTOIRE !** (`#2ECC71`) ou **DÉFAITE...** (`#E74C3C`)
- XP gagné : `evt.XPGained`
- Loot reçu : liste ou "Aucun loot"
- Bouton **Retour**

### Masquage des autres panels

À l'affichage, masque : `ActionMenuUI`, `SkillMenuUI`, `ItemMenuUI`, `CompanionMenuUI`

### Calcul XP et Loot dans BattleManager

`CharacterData` reçoit deux nouveaux champs publics (sans événements) :
- `public int XPReward;` (0 pour joueurs)
- `public LootTableSO SourceLootTable;` (null pour joueurs)

Renseignés dans `BattleCampaignBridge` après `enemy.Initialize(...)` :
```csharp
enemy.XPReward = enemySO.xpReward; // xpReward existe déjà sur EnemySO
enemy.SourceLootTable = enemySO.lootTable;
```

`BattleManager.CheckBattleEnd()` :
```csharp
int xp = _enemyTeam.Sum(c => c.XPReward);
var loot = _enemyTeam
    .Where(c => c.SourceLootTable != null)
    .Select(c => c.SourceLootTable.Roll())
    .Where(item => item != null)
    .ToList();
EventBus.Publish(new BattleEndedEvent { PlayerWon = enemiesAllDead, XPGained = xp, Loot = loot });
```

### BattleEndedEvent — struct → class

```csharp
public class BattleEndedEvent
{
    public bool PlayerWon;
    public int XPGained;
    public List<EquipmentSO> Loot;
}
```

Fichiers utilisant `BattleEndedEvent` à mettre à jour : `BattleHUD.cs`, `ActionMenuUI.cs`, `BattleCampaignBridge.cs` — le changement struct→class ne nécessite pas de modification de code dans ces fichiers (compatibilité C# assurée), uniquement vérification.

---

## Fichiers impactés

| Fichier | Action |
|---------|--------|
| `ActionMenuUI.cs` | Ajouter `companionButton` + `CompanionMenuUI` ref ; câbler `itemsButton` |
| `BattleHUD.cs` | Couleur HP/MP ; `OnBattleEnded` masque indicateur au lieu d'afficher texte ; appel `StatusDisplayUI.Initialize()` |
| `BattleManager.cs` | `CheckBattleEnd()` calcule XP + loot |
| `BattleCampaignBridge.cs` | Renseigner `XPReward` + `SourceLootTable` ; supprimer `LoadScene` ; ajouter `GainXP` |
| `GameEvents.cs` | `BattleEndedEvent` struct→class + XP/Loot ; `CompanionActivatedEvent` + champ `Message` |
| `CharacterData.cs` | Ajouter `XPReward` et `SourceLootTable` |
| `Inventory.cs` | Ajouter `Consumables` + Add/Remove |
| `Enums.cs` | Ajouter `ConsumableEffectType` |
| `BattleLog.cs` | S'abonner à `CompanionActivatedEvent` |
| **Nouveaux** | `ConsumableSO.cs`, `ItemMenuUI.cs`, `CompanionMenuUI.cs`, `VictoryScreenUI.cs`, `StatusDisplayUI.cs` |

---

## Ce qui n'est PAS dans ce scope

- Animations (flash de dégâts, transitions)
- Sélection de cible visuelle (toujours premier ennemi/allié vivant)
- Indicateur d'ordre des tours
- Sons
