# Plan 6 — Demo Campagne : WorldMap, Exploration, 2 Zones, Boss

**Date :** 2026-03-22
**Étape roadmap :** 9 / 10
**Livrable :** Demo campagne jouable — personnage sélectionnable, WorldMap, Village, Donjon, Boss, sauvegarde

---

## 1. Objectif

Rendre le jeu **testable de bout en bout** : sélectionner un personnage, explorer le monde, déclencher des combats, dialoguer avec des PNJs, ouvrir des coffres et vaincre un boss. Tout en placeholders visuels (rectangles colorés Unity) — les vrais sprites sont intégrés en Plan 7.

---

## 2. Scènes Unity

| Scène | Rôle |
|-------|------|
| `MainMenu` | Start / Charger partie / Arène / Quitter |
| `CharacterSelect` | Sélection parmi 3 personnages prédéfinis |
| `WorldMap` | Carte cliquable — zones avec état (verrouillé / disponible / complété) |
| `Village` | Tilemap top-down — Village de Départ |
| `Donjon` | Tilemap top-down — Donjon du Roi Squelette |
| `Battle` | Existante — réutilisée sans modification |

---

## 3. Architecture technique

### 3.1 GameSession (singleton persistant)

`GameSession` est un MonoBehaviour `DontDestroyOnLoad`. Il est le point central de toutes les données de session :

```
GameSession
├── CharacterData ActiveCharacter
├── ProgressionFlags Flags
├── int Gold
└── SaveSystem SaveSystem
```

Toutes les scènes accèdent aux données via `GameSession.Instance`.

### 3.2 Nouveaux scripts

**Scripts/Campaign/**
- `GameSession.cs` — singleton DontDestroyOnLoad, porte CharacterData actif + flags + or
- `SceneLoader.cs` — transitions avec fondu noir (coroutine async), méthode statique `LoadScene(string)`
- `ProgressionFlags.cs` — dictionnaire `Dictionary<string, bool>` de flags nommés ; méthodes `Set(key)`, `IsSet(key)`, `Reset()`
- `SaveSystem.cs` — sérialise/désérialise `SaveData` en JSON vers `Application.persistentDataPath/save.json` ; auto-save déclenché par événement `BossDefeatedEvent`

**Scripts/Exploration/**
- `PlayerController.cs` — déplacement WASD/flèches en top-down, vitesse configurable, Rigidbody2D + collisions
- `NpcInteractor.cs` — détecte appui sur E dans un trigger 2D → lance un dialogue Yarn Spinner par `DialogueRunner`
- `ChestInteractor.cs` — coffre avec état (ouvert/fermé) persisté dans `ProgressionFlags` → loot depuis `LootTableSO` → ajout à l'inventaire du personnage actif
- `EncounterTrigger.cs` — zone de collision 2D → probabilité configurable de déclencher un combat (seuil par pas, cooldown entre rencontres) → charge scène `Battle` avec ennemis aléatoires tirés de `CampaignZoneSO`
- `SceneEntrance.cs` — trigger 2D portail → appelle `SceneLoader.LoadScene()` avec position d'arrivée

**Scripts/Data/**
- `EnemySO.cs` — ScriptableObject : `enemyName`, stats (hp/mp/atk/def/mag/res/agi/lck), `aiLevel (Easy/Normal)`, `elementalAffinity`, `lootTable (LootTableSO)`, `xpReward`
- `LootTableSO.cs` — ScriptableObject : liste de `LootEntry { EquipmentSO equipment; float weight; }` ; méthode `Roll()` retourne un `EquipmentSO` (peut être null si roll vide)
- `CampaignZoneSO.cs` — ScriptableObject : `zoneName`, `enemies[]` (pool de rencontres aléatoires), `boss (EnemySO)`, `sceneKey`

**Scripts/Campaign/UI/**
- `MainMenuUI.cs` — boutons : Start (→ CharacterSelect si pas de save, sinon propose Continuer/Nouvelle Partie), Charger, Arène, Quitter
- `CharacterSelectUI.cs` — affiche 3 cartes de perso prédéfinis ; clic → `GameSession.Instance.ActiveCharacter` → charge `WorldMap`
- `WorldMapUI.cs` — affiche les zones ; zone cliquable si `!IsLocked` ; indicateur d'état (disponible / complété)
- `SaveMenuUI.cs` — bouton accessible depuis pause (Échap) ou PNJ Sauvegarde → appelle `SaveSystem.Save()`

**Dialogues Yarn Spinner/**
- `Assets/Dialogue/Village.yarn` — dialogues pour 4 PNJs : Villageois, Forgeron (accès ForgeSystem), Enchanteur (accès EnchantSystem), PNJ Sauvegarde (déclenche save)
- `Assets/Dialogue/Boss.yarn` — cinématique d'intro du Roi Squelette avant le combat

### 3.3 Assets ScriptableObjects (générés par RPGAssetCreator)

**Ennemis (EnemySO) :**
| Nom | Zone | HP | ATK | DEF | AGI | Affinité | XP |
|-----|------|----|-----|-----|-----|----------|----|
| Squelette | Donjon | 60 | 18 | 8 | 10 | Ténèbres | 30 |
| Archer Squelette | Donjon | 45 | 22 | 5 | 15 | Ténèbres | 35 |
| Golem d'Os | Donjon | 90 | 15 | 18 | 6 | Ténèbres | 50 |
| Roi Squelette | Boss | 300 | 30 | 15 | 12 | Ténèbres | 500 |

**Zones (CampaignZoneSO) :**
- `Zone_Village` — pas d'ennemis aléatoires, pas de boss
- `Zone_Donjon` — pool = [Squelette, Archer Squelette, Golem d'Os], boss = Roi Squelette

**Personnages prédéfinis (instanciés dans GameSession au démarrage) :**
| Nom | Classe | Race | Niveau | Compagnon |
|-----|--------|------|--------|-----------|
| Kael | Guerrier | Humain | 5 | Loup des Ombres |
| Lyra | Mage | Elfe | 5 | Corbeau Analyste |
| Theron | Soigneur | Lycanthrope | 5 | Fée Sylvestre |

---

## 4. Boss — Roi Squelette

**Comportement en deux phases :**

- **Phase 1 (100% → 50% HP)** : attaques normales, sort Ténèbres, IA Normal
- **Transition à 50% HP** : active "Armure de Crâne" (bouclier = 30% du HP max du boss via `StatusEffectType.Shield`) + publie `BossPhaseEvent { Phase = 2 }`
- **Phase 2 (50% → 0%)** : ATK +25%, peut utiliser "Appel des Morts" (invoque un Squelette comme allié, 1 fois)

**Implémentation :** `BossController.cs` (MonoBehaviour attaché au GameObject boss dans la scène Battle) qui s'abonne à `CharacterDiedEvent` et `ActionResolvedEvent` pour surveiller le seuil de HP.

> Note : "Appel des Morts" (invocation d'allié en combat) est une mécanique nouvelle. Pour Plan 6, l'allié invoqué est un `CharacterData` créé à la volée depuis un `EnemySO` et injecté dans le `TurnSystem` existant.

---

## 5. Système de sauvegarde

Format : JSON unique à `Application.persistentDataPath/save.json`.

```json
{
  "characterName": "Kael",
  "classKey": "Guerrier",
  "raceKey": "Humain",
  "level": 5,
  "experience": 500,
  "currentHP": 120,
  "gold": 200,
  "flags": { "village_visited": true, "boss_defeated": false },
  "equippedItems": ["iron_sword", "iron_shield"],
  "companionKey": "loup_des_ombres"
}
```

- **Auto-save** : déclenché par `BossDefeatedEvent` et `SceneEntrance` (changement de zone)
- **Sauvegarde manuelle** : via PNJ Sauvegarde dans le village ou menu pause (Échap)
- **Chargement** : depuis `MainMenu` → bouton "Charger" ; si pas de fichier, bouton grisé

---

## 6. Progression de la session

```
MainMenu
  ↓ Start
CharacterSelect → sélection perso prédéfini
  ↓
WorldMap → Village disponible (Donjon verrouillé)
  ↓ clic Village
Village (exploration)
  ├── Dialogues PNJs (Yarn Spinner)
  ├── Forgeron / Enchanteur (systèmes existants)
  ├── Coffres (loot)
  └── SceneEntrance → Donjon (déverrouille Donjon sur WorldMap)
  ↓
Donjon (exploration)
  ├── EncounterTriggers → Battle (ennemis aléatoires)
  ├── Coffres (loot garanti Rare+)
  └── Trigger Boss → dialogue Boss.yarn → Battle (Roi Squelette)
  ↓ boss vaincu
Auto-save + WorldMap (Donjon marqué complété)
```

---

## 7. Tests

Tous les tests sont des **EditMode tests** (Unity Test Runner). Les scènes Unity et les MonoBehaviours ne sont pas testables en EditMode — seule la logique pure l'est.

Couverture cible :

| Fichier test | Scénarios |
|-------------|-----------|
| `EnemySOTests.cs` | Stats valides, XP reward > 0 |
| `LootTableTests.cs` | Roll retourne item du pool, Roll sur table vide retourne null, poids respectés |
| `ProgressionFlagsTests.cs` | Set/IsSet/Reset, flags indépendants |
| `SaveSystemTests.cs` | Save puis Load restitue données identiques, Load sans fichier retourne null |
| `BossControllerTests.cs` | Phase 2 déclenchée à ≤50% HP, bouclier activé, phase 2 non re-déclenchée |

---

## 8. Assets visuels (placeholder Plan 6 → finaux Plan 7)

En Plan 6 : rectangles colorés Unity (pas d'import d'assets requis).

Assets à préparer pour Plan 7 :

**Tilesets (pixel art 16×16 ou 32×32)**
- Village extérieur : herbe, chemin, eau, arbres, façades bâtiments
- Village intérieur : sol bois, murs, meubles
- Donjon : pierre, sol, torches, portes, cages
- WorldMap : fond carte + icônes zones

**Sprites personnages (idle + marche 4 directions)**
- Joueur (3 variantes : Guerrier, Mage, Soigneur)
- PNJs × 4 : Villageois, Forgeron, Enchanteur, PNJ Sauvegarde
- Ennemis × 3 : Squelette, Archer Squelette, Golem d'Os
- Boss × 1 : Roi Squelette (64×64+)

**UI**
- Frame boîte de dialogue Yarn Spinner
- Portraits × 3 (CharacterSelect)
- Fonds : MainMenu, CharacterSelect, WorldMap
- Icônes zones WorldMap × 2

**Sources gratuites recommandées :** itch.io (tag: rpg, free), OpenGameArt.org

---

## 9. Hors scope Plan 6

- Chiffrement AES de la sauvegarde (Plan 7)
- Multi-slots de sauvegarde (Plan 7)
- Steam Cloud Save (Plan 7)
- Vraie création de personnage libre (Plan 7)
- Audio (musiques, SFX) (Plan 7)
- Classes/races au-delà des 3 persos prédéfinis (Plan 7)
- DOTween animations (Plan 7)
