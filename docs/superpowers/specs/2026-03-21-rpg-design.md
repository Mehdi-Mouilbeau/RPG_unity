# RPG Tour par Tour — Document de Design
**Date :** 2026-03-21
**Moteur :** Unity + C#
**Cible :** Steam (jeu payant)
**Plateforme :** PC Windows / Mac / Linux

---

## 1. Vision du jeu

RPG tour par tour en side-view 2D, inspiré de Final Fantasy et Dragon Quest. Le joueur crée un personnage (classe + race), l'équipe et le fait progresser au travers d'une campagne solo narrative et d'un mode arène multijoueur local (même PC). Le jeu est conçu pour être commercialisé sur Steam.

---

## 2. Modes de jeu

### 2.1 Campagne Solo
- Monde structuré en zones accessibles via une **World Map** cliquable
- Progression narrative linéaire au lancement : villages → donjons → boss de zone
- Exploration en 2D side-view : PNJ dialoguants, coffres, événements aléatoires
- Rencontres aléatoires et scripted déclenchant des combats
- Chaque zone culmine sur un **boss** avec mécanique unique et loot garanti rare+
- Dialogues via **Yarn Spinner** (package open-source Unity, intégré à l'éditeur)

### 2.2 Arène Multijoueur Local
- **1v1** : duel entre 2 joueurs sur le même PC
- **3v3** : équipes de 3 personnages ; les slots vacants sont remplis par des bots
- **Phase de draft avant chaque combat :**
  - Chaque joueur banne 1 personnage du roster adverse (optionnel si roster < 6)
  - Pick en ordre serpent : J1 pick, J2 pick-pick, J1 pick (pour le 3v3)
  - Pas de limite de temps en local (configurable)
- Personnages utilisables : créés en campagne ou sélectionnés dans un **roster prédéfini de 20 persos** — 2 personnages par classe (une combinaison race/spé différente par perso), conçus par le designer pour assurer l'équilibre
- **Compagnons en arène** : chaque joueur peut utiliser le compagnon associé à son personnage principal ; les bots n'ont pas de compagnon
- Aucun réseau requis — tout en local

---

## 3. Système de Combat

### 3.1 Déroulement d'un tour
1. Calcul de l'ordre d'initiative (stat AGI + modificateurs race/classe)
2. Personnage actif mis en surbrillance
3. Menu d'action : **Attaquer / Sorts / Objets / Compagnon / Passer**
4. Le joueur ou le bot choisit une action
5. Résolution de l'action (calcul dégâts, effets, statuts)
6. Animation side-view + feedback UI (chiffres dégâts flottants, icônes statuts)
7. Vérification fin de combat → tour suivant

### 3.2 Statistiques
| Stat | Rôle |
|------|------|
| HP | Points de vie |
| MP | Mana / Énergie |
| ATK | Attaque physique |
| DEF | Défense physique |
| MAG | Puissance magique |
| RES | Résistance magique |
| AGI | Initiative & taux d'esquive |
| LCK | Taux de coup critique & effets aléatoires |

### 3.3 Formules de dommages

**Attaque physique :**
```
Dégâts bruts = ATK_attaquant × 2 - DEF_défenseur
Dégâts bruts minimum = 1 (jamais 0)
Dégâts finaux = Dégâts bruts × Modificateur élémentaire × Modificateur critique
```

**Attaque magique :**
```
Dégâts bruts = MAG_attaquant × 2 - RES_défenseur
Dégâts bruts minimum = 1
Dégâts finaux = Dégâts bruts × Modificateur élémentaire × Modificateur critique
```

**Modificateur critique :**
```
Taux critique = LCK / 4 (en %) — plafonné à 50%
Si critique : ×1.5 dégâts
```

**Modificateur élémentaire :**
- Avantage élémentaire (skill de l'attaquant > résistance du défenseur) : ×1.25
- Désavantage élémentaire (skill de l'attaquant < résistance du défenseur) : ×0.75
- Neutre : ×1.0

> L'"avantage élémentaire" est déterminé par l'élément du **skill utilisé** comparé à l'**affinité élémentaire de la cible** (définie dans son ScriptableObject de race/classe).

### 3.4 Système élémentaire — Matrice complète

6 éléments. Le cycle des 4 éléments naturels et la paire Lumière/Ténèbres sont indépendants.

**Cycle naturel (circulaire) :**
```
Feu > Nature > Foudre > Eau > Feu
```

**Paire Lumière/Ténèbres (opposition mutuelle) :**

Lumière et Ténèbres sont des éléments **adversariaux mutuels** : chacun inflige ×1.25 à l'autre et subit ×0.75 de l'autre. Ils n'ont pas de relation neutre entre eux — les affrontements Lumière/Ténèbres sont toujours intenses dans les deux sens. C'est une décision de design intentionnelle qui différencie cette paire du cycle naturel.

```
Lumière attaque Ténèbres : ×1.25   |   Ténèbres reçoit Lumière : ×0.75
Ténèbres attaque Lumière : ×1.25   |   Lumière reçoit Ténèbres : ×0.75
```

**Interactions entre les deux groupes :**
Aucune interaction — un sort de Feu contre une cible à affinité Lumière est neutre (×1.0). Les deux systèmes sont orthogonaux.

**Tableau de référence :**

| Attaquant ↓ / Cible → | Feu | Nature | Foudre | Eau | Lumière | Ténèbres |
|----------------------|-----|--------|--------|-----|---------|----------|
| **Feu**              | ×1  | ×1.25  | ×1     | ×0.75| ×1     | ×1       |
| **Nature**           | ×0.75| ×1   | ×1.25  | ×1  | ×1      | ×1       |
| **Foudre**           | ×1  | ×0.75  | ×1     | ×1.25| ×1     | ×1       |
| **Eau**              | ×1.25| ×1   | ×0.75  | ×1  | ×1      | ×1       |
| **Lumière**          | ×1  | ×1     | ×1     | ×1  | ×1      | ×1.25    |
| **Ténèbres**         | ×1  | ×1     | ×1     | ×1  | ×1.25   | ×1       |

> Note d'implémentation : dans `DamageCalculator`, Lumière et Ténèbres s'appliquent symétriquement — le modificateur est ×1.25 si l'élément du skill ≠ l'élément de la cible dans la paire {Lumière, Ténèbres}, et ×0.75 dans le sens inverse.

### 3.5 Statuts & Effets
| Statut | Type | Effet | Durée |
|--------|------|-------|-------|
| Brûlure | Négatif | Dégâts Feu = 5% HP max / tour | 3 tours |
| Poison | Négatif | Dégâts Nature = 5% HP max / tour | 3 tours |
| Gel | Négatif | Skip le prochain tour | 1 tour |
| Paralysie | Négatif | 50% chance skip / tour | 2 tours |
| Confusion | Négatif | Action aléatoire ce tour | 2 tours |
| Bouclier | Positif (buff) | Absorbe dégâts = 20% HP max du porteur (non stackable, 1 couche) | Jusqu'à destruction |

> Le Bouclier est un **buff**, pas un statut négatif — il est représenté dans une barre dédiée au-dessus de la barre HP. Il disparaît quand son absorption est épuisée ou en fin de combat.

### 3.6 Compagnon en combat
- Bonus actif **hors-slot** — n'occupe pas de place dans l'équipe de 3
- Choisi **à la création du personnage**, modifiable hors combat (en ville / au camp)
- Possède **3 à 5 compétences** selon son type, chacune avec cooldown indépendant (3–6 tours)
- Activé via l'option "Compagnon" dans le menu d'action du tour actif
- **En arène** : disponible pour chaque joueur humain ; les bots n'ont pas de compagnon

**4 types de compagnons (taxonomie) :**

| Type | Archétype | Compétences principales |
|------|-----------|------------------------|
| Offensif | Loup des ombres, Drake de feu | Attaques directes, debuff DEF ennemi |
| Défensif | Golem de cristal, Esprit bouclier | Absorbe dégâts, soins d'urgence allié |
| Support | Fée sylvestre, Homonculus | Buffs stats, suppression statuts négatifs |
| Utilitaire | Corbeau analyste, Méca-drone | Révèle infos ennemies, modifie l'initiative |

---

## 4. Personnages

### 4.1 Création de personnage
Ordre des étapes :
1. Choisir la **Race** (bonus passifs)
2. Choisir la **Classe** (rôle + arbre de compétences)
3. Choisir l'**équipement de départ** (propositions adaptées à la classe)
4. Choisir le **Compagnon** (parmi le roster disponible)
5. **Nommer** le personnage

### 4.2 Races (11)
| Race | Bonus passif principal | Affinité élémentaire |
|------|----------------------|----------------------|
| Humain | +5% XP, stats équilibrées | Neutre |
| Elfe | +AGI +10%, +MAG +10% | Lumière / Nature |
| Gnome | Coût crafting -20%, +MAG +5% | Foudre |
| Androïde | Immunité Poison, +DEF +15% | Foudre |
| Peuples des Plantes | Régén HP +3% / tour | Nature |
| Esprits / Élémentaires | +MAG +15%, affinité au choix à la création | Variable |
| Dragons | +ATK +10%, +RES +10%, souffle élémentaire 1×/combat | Feu |
| Êtres d'énergie | Coût sorts -20%, +MP max +20% | Lumière |
| Mort-vivants | Immunité Poison+Gel, drain HP sur attaque +5% | Ténèbres |
| Lycanthropes | Transformation 3 tours (ATK +25%, AGI +25%) 1×/combat | Nature |
| Colosses de Pierre | +DEF max +25%, AGI -10%, provocation passive | Neutre |

### 4.3 Classes (10) & Spécialisations
| Classe | Rôle | Spé A | Spé B |
|--------|------|-------|-------|
| ⚔️ Guerrier | DPS physique | Berserker | Chevalier |
| 🔮 Mage | DPS magique élémentaire | Élémentaliste | Arcaniste |
| 😈 Démoniste | Ténèbres, drain, malédictions | Pacte Démoniaque | Nécromancie |
| 🗡️ Voleur | Critique, poison, esquive | Assassin | Filou |
| 🏹 Archer | Snipe, debuffs à distance | Chasseur | Rôdeur |
| ✨ Soigneur | Soins HP, résurrection, boucliers alliés | Prêtre | Chaman |
| 🐉 Invocateur | Convoque créatures alliées | Maître des Bêtes | Nécro-invocateur |
| 🛡️ Tank | Provocation, absorbe dégâts | Gardien | Paladin Noir |
| ⚙️ Ingénieur | Déploie tourelles, bombes, drones | Mécanicien | Alchimiste |
| 🎭 Éclaireur | Debuffs ennemis, buffs stats, analyse | Buffer/Debuffer | Analyste |

> **Différenciation Soigneur / Éclaireur** : le Soigneur agit sur les **HP et la survie** (soins, résurrection, boucliers) ; l'Éclaireur agit sur les **stats et l'information** (buffs ATK/DEF/AGI, debuffs ennemis, révélation des faiblesses, analyse de mana). Pas de redondance fonctionnelle.

### 4.4 Arbre de Compétences
- **Tronc commun** (niveaux 1–10) : 5 à 8 compétences de base de la classe
- **Choix de spécialisation** au niveau 10 (irréversible sauf reset)
- **Deux branches** (niveaux 10–30) : 8 à 12 compétences par branche
- **Points de compétences** : 1 point par level up (30 points au total)
- **Réinitialisation** : disponible chez un PNJ dédié en ville, coût = niveau actuel × 50 or
- **Niveau maximum** : 30

### 4.5 Progression XP
| Niveau | XP requis (cumulé) | Points compétences acquis |
|--------|-------------------|--------------------------|
| 1→2 | 100 XP | 1 |
| 2→5 | ~500 XP | 3 |
| 5→10 | ~2 000 XP | 5 |
| 10→20 | ~15 000 XP | 10 |
| 20→30 | ~50 000 XP | 10 |

> Courbe exponentielle douce. Les valeurs exactes seront calibrées en playtest.

---

## 5. Équipement

### 5.1 Slots (7)
- 🗡️ Arme principale
- 🛡️ Offhand (bouclier ou arme secondaire)
- ⛑️ Casque
- 🥋 Armure
- 🥾 Bottes
- 💍 Bague × 2

### 5.2 Niveaux de rareté
| Rareté | Couleur | Bonus stats | Effets spéciaux |
|--------|---------|-------------|-----------------|
| Commun | ⬜ Gris | Stats de base | 0 |
| Peu commun | 🟩 Vert | +10% stats | 0 |
| Rare | 🟦 Bleu | +20% stats | 1 effet |
| Épique | 🟪 Violet | +35% stats | 2 effets |
| Légendaire | 🟨 Or | +50% stats | 2 effets + nom unique |

### 5.3 Crafting (Forge)
- Recette : **pièce équipement + matériaux de zone + or** → monte d'un tier de rareté
- Chaque upgrade aléatoire dans un **pool d'effets** défini pour le type d'objet
- Non destructif : l'objet original n'est pas consommé, il est transformé
- Interface : PNJ Forgeron en ville / camp

### 5.4 Enchantement
- Ajoute un **effet secondaire** à un équipement Rare ou supérieur
- L'effet est choisi dans une liste de 3 options aléatoires tirées du pool de l'objet
- Un seul enchantement par objet (remplacement possible, l'ancien est perdu)
- Coût : ressources d'enchantement (Pierre runique, Essence élémentaire) + or
- Interface : PNJ Enchanteur en ville / camp

> **Relation effets de rareté / enchantement :** les effets octroyés par la rareté (ex. Rare = 1 effet, Légendaire = 2 effets) sont distincts du slot d'enchantement. Un objet Légendaire peut avoir 2 effets de rareté **plus** 1 enchantement = 3 effets au total maximum. L'enchantement occupe toujours un slot séparé et n'écrase jamais les effets de rareté.

---

## 6. Architecture Technique

### 6.1 Approche : Data-Driven avec ScriptableObjects

Toutes les données de jeu (classes, races, sorts, équipements, compagnons, ennemis) sont définies dans des **ScriptableObjects Unity**. Les systèmes sont génériques et pilotés par ces données — ajouter une classe = créer un fichier SO sans toucher au code.

### 6.2 Structure du projet Unity

```
Assets/
├── _Data/                        ← ScriptableObjects
│   ├── Classes/                  ← ClassSO (stats, skills ref)
│   ├── Races/                    ← RaceSO (bonuses, affinités)
│   ├── Skills/                   ← SkillSO (type, coût MP, cooldown, effet)
│   ├── Equipment/                ← EquipmentSO (slot, stats, raretés)
│   ├── Companions/               ← CompanionSO (skills, cooldowns)
│   └── Enemies/                  ← EnemySO (stats, AI level, loot table)
├── Scripts/
│   ├── Core/                     ← GameManager, SceneLoader, EventBus
│   ├── Combat/                   ← BattleManager, TurnSystem, ActionResolver, DamageCalculator
│   ├── Characters/               ← CharacterData, ClassSystem, RaceSystem
│   ├── Skills/                   ← SkillTree, SkillEffect, CooldownManager, StatusManager
│   ├── Equipment/                ← Inventory, RaritySystem, Crafting, Enchanting
│   ├── Companions/               ← CompanionSystem, CompanionController
│   ├── AI/                       ← BotBrain, ActionEvaluator, DifficultyScaler
│   ├── Input/                    ← InputRouter (Unity Input System)
│   ├── UI/                       ← BattleHUD, MenuSystem, CharacterSheet, InventoryUI
│   ├── Campaign/                 ← StoryManager, WorldMap, DialogueRunner (Yarn Spinner)
│   └── Arena/                    ← MatchManager, DraftSystem, LocalMultiplayer
└── Scenes/
    ├── MainMenu
    ├── CharacterCreation
    ├── Battle
    ├── WorldMap
    └── Arena
```

### 6.3 Communication inter-systèmes : EventBus

Les systèmes communiquent exclusivement via un EventBus central (pattern Observer) :

| Événement | Émetteur | Auditeurs |
|-----------|----------|-----------|
| `OnTurnStarted(CharacterData)` | TurnSystem | BattleHUD, BotBrain, CooldownManager |
| `OnActionResolved(ActionResult)` | ActionResolver | StatusManager, BattleHUD, CooldownManager |
| `OnCharacterDied(CharacterData)` | BattleManager | TurnSystem, BattleHUD, ArenaManager |
| `OnLevelUp(int newLevel)` | CharacterData | SkillTree, BattleHUD |
| `OnItemEquipped(EquipmentSO)` | Inventory | CharacterData (recalcul stats) |

### 6.4 IA des Bots — BotBrain

Architecture : `BotBrain` (ScriptableObject configurable par niveau) + `ActionEvaluator` (évalue et note chaque action possible à chaque tour).

| Niveau | Priorités d'évaluation |
|--------|------------------------|
| **Facile** | Action aléatoire pondérée ; ignore affinités élémentaires ; compagnon utilisé rarement |
| **Normal** | Cible le personnage ennemi le plus bas en HP ; exploite les faiblesses élémentaires ; soigne si HP allié < 30% ; utilise compagnon si 2+ ennemis ciblables |
| **Difficile** | Score de priorité sur toutes les actions possibles (dégâts / soins / CC pondérés) ; anticipe les statuts actifs ; priorise l'élimination du Soigneur ennemi ; utilise compagnon de manière optimale |

Les bots utilisent le même `ActionMenu` que les joueurs humains — seul le décideur change.

### 6.5 Input — Unity Input System
- Nouveau Input System (package officiel Unity)
- Support simultané : clavier/souris (Joueur 1 par défaut) + jusqu'à 4 manettes (Xbox/PS/générique)
- `InputRouter` : assigne dynamiquement les devices aux joueurs selon le mode
- Hot-seat : indicateur visuel clair du joueur actif lors du passage de tour

---

## 7. Sauvegarde & Progression

- **3 slots** de sauvegarde manuelle
- **Auto-save** aux checkpoints (entrée de zone, après boss)
- Format **JSON** avec chiffrement AES-128 (clé dérivée d'un GUID machine + sel fixe)
- Compatible **Steam Cloud Save** (sync automatique du dossier de saves)
- Données sauvegardées : personnages, équipement, arbre de compétences, progression narrative (flags), or, ressources, compagnon associé

---

## 8. Intégration Steam

| Fonctionnalité | Implémentation |
|----------------|---------------|
| Achievements | Steamworks API — succès liés boss, combos, exploration, arène |
| Cloud Save | Steam Remote Storage — sync du dossier saves |
| SDK Unity | **Facepunch.Steamworks** (NuGet/UPM, C# natif Unity) |
| Tests sans App ID | `steam_appid.txt` (valeur `480` = Spacewar pour tests locaux) |

---

## 9. Audio

- Musique : pistes par contexte (World Map, Combat, Boss, Menus) — Unity Audio Mixer
- SFX : sons d'attaque, sorts, UI, statuts — AudioSource pooling
- Middleware envisagé : **Unity Audio Mixer** natif (suffisant au lancement) ; migration vers FMOD possible en post-launch
- Pas de voix off au lancement

---

## 10. Feuille de Route

| Étape | Contenu | Livrable |
|-------|---------|----------|
| 1 | ScriptableObjects de base, EventBus, GameManager | Fondations techniques |
| 2 | BattleManager, TurnSystem, DamageCalculator, UI combat | Combat 1v1 jouable (2 persos test) |
| 3 | 3 classes complètes, système élémentaire, statuts | Combat varié |
| 4 | Arène locale 1v1, InputRouter (manettes + clavier) | Arène 1v1 jouable |
| 5 | Bots Facile & Normal, mode 3v3, draft | Arène 3v3 complète |
| 6 | Inventory, rarités, crafting, enchantement | Système équipement |
| 7 | SkillTree, points compétences, réinitialisation | Progression complète |
| 8 | Companion system, 4 types de compagnons | Compagnons jouables |
| 9 | Yarn Spinner, WorldMap, exploration, 2 zones, boss | Demo campagne |
| 10 | Toutes les classes/races, contenus complets, Steam | Version commerciale |

---

## 11. Packages & Dépendances Unity

| Package | Usage |
|---------|-------|
| Unity Input System | Gestion manettes + clavier |
| Facepunch.Steamworks | Intégration Steam (achievements, cloud) |
| TextMeshPro | UI texte de qualité |
| DOTween (Asset Store) | Animations UI et combat fluides |
| Newtonsoft.Json | Sérialisation des sauvegardes |
| Yarn Spinner | Système de dialogues / narration campagne |
| Unity Audio Mixer | Gestion audio multi-couches |
