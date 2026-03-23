# Companion Select — Design Spec
Date: 2026-03-23

## Contexte

`CharacterSelectUI` a déjà `CompanionSO companionSO` dans `PredefinedCharacterConfig`, mais le compagnon est pré-défini par config Inspector, pas choisi par le joueur.

La persistence du compagnon est **déjà complète** (companionKey sauvegardé/chargé dans GameSession).

## Ce qui existe déjà

- `CharacterSelectUI.PredefinedCharacterConfig` — contient `CompanionSO companionSO`
- `GameSession.CreatePredefinedCharacter(name, class, race, level, companionSO)` — assigne déjà le compagnon
- `GameDataRegistry.companions[]` + `GetCompanion(key)` — registre de tous les compagnons
- `CompanionSO.companionName` + `CompanionSO.description` — données affichables

## Objectif

Ajouter un **deuxième panneau** dans l'écran Character Select : après que le joueur a cliqué sur un personnage, un panneau de sélection de compagnon s'affiche. Le joueur choisit un compagnon parmi ceux disponibles dans `GameDataRegistry.companions[]`, puis le jeu démarre.

## Architecture

Deux panneaux dans la scène CharacterSelect :
1. **CharacterPanel** (existant) — boutons de sélection de personnage
2. **CompanionPanel** (nouveau) — affiché après sélection du personnage

### Flux

```
CharacterPanel visible
    → joueur clique un personnage
    → config sauvegardée en mémoire (_pendingConfig)
    → CharacterPanel masqué
    → CompanionPanel visible, boutons générés depuis GameDataRegistry.companions[]
        → joueur clique un compagnon
        → CreatePredefinedCharacter() avec le companionSO sélectionné
        → SetActiveCharacter(), Gold = 200
        → LoadScene("WorldMap")
```

### Gestion du cas "aucun compagnon dans le registre"

Si `GameDataRegistry.Instance` est null ou `companions` est vide/null, un bouton "Sans compagnon" est affiché — `SelectCompanion(null)` est appelé, ce qui crée le personnage sans compagnon (comportement existant conservé). `SelectCompanion` ne déréférence jamais `companionSO` avant de le passer à `CreatePredefinedCharacter` — le null est passé directement.

### `PredefinedCharacterConfig.companionSO`

Le champ `companionSO` dans `PredefinedCharacterConfig` est **supprimé** — le compagnon est désormais toujours choisi par le joueur, jamais pré-défini par config.

## Modifications

### CharacterSelectUI.cs — refonte partielle

Suppression du champ `companionSO` dans `PredefinedCharacterConfig`. Le champ `description` et la méthode `ShowDescription()` sont **conservés** — ils servent toujours à afficher la description du personnage (pas du compagnon).

Nouveaux champs sérialisés :
```csharp
[SerializeField] private GameObject characterPanel;        // panel existant à encapsuler
[SerializeField] private GameObject companionPanel;        // nouveau panel
[SerializeField] private Transform  companionListContainer; // parent des boutons générés
[SerializeField] private Button     btnBack;               // retour au CharacterPanel
[SerializeField] private TMP_Text   companionDescText;     // description du compagnon sélectionné
[SerializeField] private GameObject companionButtonPrefab; // même prefab que SkillButtonPrefab
```

Nouveaux champs privés :
```csharp
private PredefinedCharacterConfig _pendingConfig;
private readonly List<GameObject> _spawnedCompanionButtons = new();
```

Dans `Start()` : `btnBack.onClick.AddListener(ShowCharacterPanel)` ajouté.

Changement dans `SelectCharacter()` : au lieu de créer le personnage directement, stocker la config dans `_pendingConfig`, afficher CompanionPanel, générer les boutons compagnon.

Génération des boutons : à chaque appel à `ShowCompanionPanel()` — les boutons précédents sont détruits (`Destroy()`), la liste `_spawnedCompanionButtons` est vidée, puis les nouveaux boutons sont générés. Source : `GameDataRegistry.Instance?.companions` — null-guardé. Si null ou vide, un seul bouton "Sans compagnon" est affiché. `companionDescText.text` est réinitialisé à `""` en début de `ShowCompanionPanel()`.

Nouveau `SelectCompanion(CompanionSO companionSO)` : reçoit `CompanionSO` ou `null`. Appelle `GameSession.CreatePredefinedCharacter()` en décompressant `_pendingConfig` (signature existante inchangée) :
```csharp
var character = GameSession.CreatePredefinedCharacter(
    _pendingConfig.characterName,
    _pendingConfig.classSO,
    _pendingConfig.raceSO,
    _pendingConfig.level,
    companionSO); // null accepté — CreatePredefinedCharacter le gère
```
Puis `GameSession.Instance.SetActiveCharacter(character)`, `GameSession.Instance.Gold = 200`, `SceneLoader.Instance.LoadScene("WorldMap")`.

`companionDescText` : mis à jour sur chaque bouton généré via son `onClick` — affiche `companionSO.description` si non null, ou `"Aucun compagnon"` pour le bouton "Sans compagnon". Initialisé à vide dans `ShowCompanionPanel()`.

`ShowCharacterPanel()` : appelle `Destroy()` sur chaque élément de `_spawnedCompanionButtons`, vide la liste, `companionPanel.SetActive(false)`, `characterPanel.SetActive(true)`, `companionDescText.text = ""` si non null.

### Aucun nouveau fichier

Tout dans `CharacterSelectUI.cs`.

## Ce qui n'est PAS dans ce scope

- Modification du compagnon après la création (prévu mais non daté)
- Description détaillée des compétences du compagnon sur l'écran de sélection
- Animations de transition entre les panneaux
