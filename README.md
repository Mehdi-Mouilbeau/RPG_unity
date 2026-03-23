# RPG Unity

Un RPG tactique au tour par tour développé avec Unity 6 et C#.

## Aperçu

Le jeu propose un système de combat stratégique, une campagne avec exploration de donjons, un mode arène PvP, et une progression profonde via arbres de compétences, équipements et compagnons.

## Fonctionnalités implémentées

### Combat
- Système de tours avec gestion des priorités et des statuts
- Calculateur de dégâts avec système d'éléments (affinités et résistances)
- Effets de statut (poison, stun, etc.) via `StatusManager`
- Boss avec phases de comportement dynamiques
- Résolution d'actions et log de combat en temps réel

### Arène
- Mode draft 3v3 (sélection de personnages avant le combat)
- Matchmaking hot-seat (deux joueurs sur le même écran)
- IA avec deux niveaux de difficulté (Easy / Normal) via `BotBrain` et `ActionEvaluator`

### Équipements
- ScriptableObjects pour armes, armures et accessoires
- Système de raretés (Common → Legendary)
- Forge pour améliorer les équipements
- Enchantements
- Inventaire par personnage avec stats calculées dynamiquement

### Progression
- Système d'XP sur 30 niveaux
- Arbre de compétences avec branches (ex: Guerrier → Chevalier / Berserker)
- Points de compétences dépensables et réinitialisables

### Compagnons
- Compagnons recrutables avec compétences propres (`CompanionSO`, `CompanionSkillSO`)
- Système d'actions compagnon intégré au combat

### Campagne & Exploration
- Menu principal, sélection de personnage, carte du monde
- Exploration de villages et donjons avec déclencheurs d'événements
- Système de sauvegarde / chargement
- Gestion des flags de progression narrative

## Stack technique

| Domaine | Technologie |
|---|---|
| Moteur | Unity 6 |
| Langage | C# |
| Architecture | ScriptableObjects + EventBus |
| Tests | Unity Test Framework (Edit Mode) |
| Rendu | Universal Render Pipeline (URP) |

## Structure du projet

```
Assets/
├── Scripts/
│   ├── Combat/        # BattleManager, TurnSystem, DamageCalculator, ElementSystem...
│   ├── Arena/         # DraftSystem, MatchManager, ArenaRoster
│   ├── AI/            # BotBrain, ActionEvaluator
│   ├── Equipment/     # EquipmentSO, ForgeSystem, EnchantSystem, Inventory
│   ├── Skills/        # SkillTreeSO, SkillTreeState, XPSystem
│   ├── Companions/    # CompanionSO, CompanionSystem
│   ├── Campaign/      # SaveSystem, GameSession, SceneLoader
│   ├── Exploration/   # PlayerController, EncounterTrigger, BossTrigger
│   ├── Characters/    # CharacterData
│   ├── Core/          # GameManager, EventBus, GameEvents
│   ├── Data/          # ScriptableObjects partagés (Classes, Races, Ennemis, Zones...)
│   └── UI/            # BattleHUD, BattleLog, ActionMenuUI, SkillMenuUI...
├── Tests/
│   └── EditMode/      # Tests unitaires (DamageCalculator, TurnSystem, ElementSystem...)
└── _Data/             # Assets de données (ScriptableObjects instanciés)
```

## Lancer le projet

1. Cloner le repo
2. Ouvrir le dossier `My project/` avec **Unity 6**
3. Lancer la scène `Assets/Scenes/MainMenu.unity`

## Auteur

Mehdi Mouilbeau
