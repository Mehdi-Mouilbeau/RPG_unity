# Companion Select Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter un panneau de sélection de compagnon dans l'écran CharacterSelect — le joueur choisit son compagnon après avoir sélectionné son personnage.

**Architecture:** Refonte partielle de `CharacterSelectUI.cs` uniquement — ajout d'un `CompanionPanel` avec boutons générés depuis `GameDataRegistry.companions[]`. Aucun nouveau fichier. La persistence du compagnon est déjà assurée par `GameSession.Save/Load`.

**Tech Stack:** Unity 6, C#, TextMeshPro, Unity UI, NUnit (EditMode tests)

**Spec:** `docs/superpowers/specs/2026-03-23-companion-select-design.md`

---

## File Map

| Fichier | Action |
|---------|--------|
| `My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs` | Refonte partielle |
| `My project/Assets/Tests/EditMode/CompanionSelectTests.cs` | Nouveaux tests EditMode |

---

## Task 1 : Refonte de CharacterSelectUI

**Files:**
- Modify: `My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs`
- Create: `My project/Assets/Tests/EditMode/CompanionSelectTests.cs`

- [ ] **Créer les tests d'abord** — `My project/Assets/Tests/EditMode/CompanionSelectTests.cs` :

```csharp
using NUnit.Framework;

public class CompanionSelectTests
{
    // ── SelectCompanion null-safety ───────────────────────────────────────

    [Test]
    public void CreatePredefinedCharacter_NullCompanion_CharacterHasNoCompanion()
    {
        // GameSession.CreatePredefinedCharacter avec companionSO = null
        // ne doit pas lancer d'exception et doit retourner un personnage sans compagnon

        var classSO = UnityEngine.ScriptableObject.CreateInstance<ClassSO>();
        classSO.className = "Guerrier";
        classSO.baseHP = 100; classSO.baseMP = 20;
        classSO.baseATK = 10; classSO.baseDEF = 8;
        classSO.baseMAG = 3;  classSO.baseRES = 3;
        classSO.baseAGI = 5;  classSO.baseLCK = 4;

        var raceSO = UnityEngine.ScriptableObject.CreateInstance<RaceSO>();
        raceSO.raceName = "Humain";

        var character = GameSession.CreatePredefinedCharacter(
            "Héros", classSO, raceSO, level: 1, companionSO: null);

        Assert.IsNotNull(character);
        Assert.IsNull(character.Companion);

        UnityEngine.Object.DestroyImmediate(classSO);
        UnityEngine.Object.DestroyImmediate(raceSO);
    }

    [Test]
    public void CreatePredefinedCharacter_WithCompanion_CompanionAssigned()
    {
        var classSO = UnityEngine.ScriptableObject.CreateInstance<ClassSO>();
        classSO.className = "Mage";
        classSO.baseHP = 80; classSO.baseMP = 60;
        classSO.baseATK = 5; classSO.baseDEF = 4;
        classSO.baseMAG = 12; classSO.baseRES = 8;
        classSO.baseAGI = 6;  classSO.baseLCK = 5;

        var raceSO = UnityEngine.ScriptableObject.CreateInstance<RaceSO>();
        raceSO.raceName = "Elfe";

        var companionSO = UnityEngine.ScriptableObject.CreateInstance<CompanionSO>();
        companionSO.companionName = "Esprit";

        var character = GameSession.CreatePredefinedCharacter(
            "Mage", classSO, raceSO, level: 1, companionSO: companionSO);

        Assert.IsNotNull(character.Companion);
        Assert.AreEqual("Esprit", character.Companion.Definition.companionName);

        UnityEngine.Object.DestroyImmediate(classSO);
        UnityEngine.Object.DestroyImmediate(raceSO);
        UnityEngine.Object.DestroyImmediate(companionSO);
    }
}
```

- [ ] **Lancer les tests** — Window > General > Test Runner > EditMode > Run All
  - Ces deux tests doivent **PASS** dès maintenant (ils testent du code existant — vérification de non-régression)

- [ ] **Remplacer `CharacterSelectUI.cs`** entier :

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [System.Serializable]
    public struct PredefinedCharacterConfig
    {
        public string      characterName;
        public ClassSO     classSO;
        public RaceSO      raceSO;
        public int         level;
        [TextArea] public string description;
    }

    [Header("Character Panel")]
    [SerializeField] private PredefinedCharacterConfig[] characters;
    [SerializeField] private Button[]  selectButtons;
    [SerializeField] private TMP_Text  descriptionText;
    [SerializeField] private GameObject characterPanel;

    [Header("Companion Panel")]
    [SerializeField] private GameObject companionPanel;
    [SerializeField] private Transform  companionListContainer;
    [SerializeField] private Button     btnBack;
    [SerializeField] private TMP_Text   companionDescText;
    [SerializeField] private GameObject companionButtonPrefab;

    private PredefinedCharacterConfig       _pendingConfig;
    private readonly List<GameObject>       _spawnedCompanionButtons = new();

    private void Start()
    {
        for (int i = 0; i < selectButtons.Length && i < characters.Length; i++)
        {
            int index = i;
            selectButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }

        if (btnBack != null) btnBack.onClick.AddListener(ShowCharacterPanel);

        if (companionPanel  != null) companionPanel.SetActive(false);
        if (characterPanel  != null) characterPanel.SetActive(true);
    }

    // ── Character selection ───────────────────────────────────────────────

    private void SelectCharacter(int index)
    {
        if (index >= characters.Length) return;
        _pendingConfig = characters[index];
        ShowCompanionPanel();
    }

    public void ShowDescription(int index)
    {
        if (descriptionText != null && index < characters.Length)
            descriptionText.text = characters[index].description;
    }

    // ── Companion panel ───────────────────────────────────────────────────

    private void ShowCompanionPanel()
    {
        // Nettoyer les anciens boutons
        foreach (var go in _spawnedCompanionButtons)
            if (go != null) Destroy(go);
        _spawnedCompanionButtons.Clear();

        if (companionDescText != null) companionDescText.text = "";

        // Basculer les panels
        if (characterPanel != null) characterPanel.SetActive(false);
        if (companionPanel  != null) companionPanel.SetActive(true);

        // Générer les boutons
        var companions = GameDataRegistry.Instance?.companions;
        if (companions != null && companions.Length > 0)
        {
            foreach (var companion in companions)
            {
                if (companion == null) continue;
                var captured = companion;
                SpawnCompanionButton(companion.companionName, () =>
                {
                    if (companionDescText != null)
                        companionDescText.text = captured.description ?? "";
                    SelectCompanion(captured);
                });
            }
        }
        else
        {
            SpawnCompanionButton("Sans compagnon", () =>
            {
                if (companionDescText != null)
                    companionDescText.text = "Aucun compagnon";
                SelectCompanion(null);
            });
        }
    }

    private void SpawnCompanionButton(string label, System.Action onClick)
    {
        if (companionButtonPrefab == null || companionListContainer == null) return;

        var go  = Instantiate(companionButtonPrefab, companionListContainer);
        _spawnedCompanionButtons.Add(go);

        var btn = go.GetComponent<Button>();
        var lbl = go.GetComponentInChildren<TMP_Text>();

        if (lbl != null) lbl.text = label;
        if (btn != null && onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private void SelectCompanion(CompanionSO companionSO)
    {
        var character = GameSession.CreatePredefinedCharacter(
            _pendingConfig.characterName,
            _pendingConfig.classSO,
            _pendingConfig.raceSO,
            _pendingConfig.level,
            companionSO);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetActiveCharacter(character);
            GameSession.Instance.Gold = 200;
        }

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("WorldMap");
    }

    private void ShowCharacterPanel()
    {
        foreach (var go in _spawnedCompanionButtons)
            if (go != null) Destroy(go);
        _spawnedCompanionButtons.Clear();

        if (companionDescText != null) companionDescText.text = "";
        if (companionPanel    != null) companionPanel.SetActive(false);
        if (characterPanel    != null) characterPanel.SetActive(true);
    }
}
```

- [ ] **Vérifier compilation** dans Unity — aucune erreur.

- [ ] **Lancer les tests** — résultat attendu : tous les tests passent (les 2 nouveaux + tous les anciens)

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Campaign/UI/CharacterSelectUI.cs" \
        "My project/Assets/Tests/EditMode/CompanionSelectTests.cs"
git commit -m "feat: CharacterSelectUI adds companion selection panel"
```

---

## Task 2 : Setup scène CharacterSelect (Unity Editor — manuel)

Cette tâche est effectuée directement dans l'éditeur Unity.

- [ ] **Créer `CharacterPanel`** :
  - Clic droit sur le Canvas de la scène CharacterSelect → **Create Empty** → renommer `CharacterPanel`
  - Déplacer les boutons de sélection de personnage existants dedans

- [ ] **Créer `CompanionPanel`** :
  - Clic droit sur Canvas → **UI > Panel** → renommer `CompanionPanel`
  - Sous `CompanionPanel` :
    - Créer `CompanionListContainer` (Create Empty)
    - Créer `CompanionDescText` (UI > Text - TextMeshPro)
    - Créer `BtnBack` (UI > Button - TextMeshPro, texte "Retour")

- [ ] **Câbler `CharacterSelectUI`** dans l'Inspector :
  - `Character Panel` → `CharacterPanel`
  - `Companion Panel` → `CompanionPanel`
  - `Companion List Container` → `CompanionListContainer`
  - `Btn Back` → `BtnBack`
  - `Companion Desc Text` → `CompanionDescText`
  - `Companion Button Prefab` → `SkillButtonPrefab`
  - **Retirer le champ `Companion SO`** des configs de personnages dans l'Inspector (le champ n'existe plus dans le struct)

- [ ] **Tester en Play** :
  - Nouveau Jeu → CharacterSelect → cliquer un personnage → CompanionPanel s'affiche
  - Cliquer un compagnon → WorldMap se charge
  - Bouton Retour → CharacterPanel réapparaît
