using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Flux de sélection :
///   1. Ton personnage  → Race → Classe  (marcheur, ne combat pas)
///   2. Équipier 1/3    → Race → Classe
///   3. Équipier 2/3    → Race → Classe
///   4. Équipier 3/3    → Race → Classe
///   5. Compagnon       → assigné à l'équipe (party[0])
///   → WorldMap
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("Panel — Race")]
    [SerializeField] private GameObject racePanel;
    [SerializeField] private Transform  raceListContainer;
    [SerializeField] private GameObject raceButtonPrefab;
    [SerializeField] private TMP_Text   raceDescriptionText;

    [Header("Panel — Classe")]
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private Transform  characterListContainer;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private TMP_Text   classDescriptionText;
    [FormerlySerializedAs("btnBackFromRace")]
    [SerializeField] private Button     btnBackFromClass;

    [Header("Panel — Compagnon")]
    [SerializeField] private GameObject companionPanel;
    [SerializeField] private Transform  companionListContainer;
    [SerializeField] private GameObject companionButtonPrefab;
    [SerializeField] private TMP_Text   companionDescText;
    [SerializeField] private Button     btnBackFromCompanion;

    [Header("Indicateur de progression (optionnel)")]
    [SerializeField] private TMP_Text slotHeaderText;

    private ClassSO _pendingClass;
    private RaceSO  _pendingRace;

    // État interne
    private enum Step { MainCharacter, PartyMember, Companion }
    private Step _step = Step.MainCharacter;

    private readonly List<CharacterData> _partyMembers = new();
    private int _partySlot = 0;
    private const int PartySize = 3;

    private readonly List<GameObject> _spawnedRaceButtons      = new();
    private readonly List<GameObject> _spawnedClassButtons     = new();
    private readonly List<GameObject> _spawnedCompanionButtons = new();

    private void Start()
    {
        if (btnBackFromClass     != null) btnBackFromClass.onClick.AddListener(ShowRacePanel);
        if (btnBackFromCompanion != null) btnBackFromCompanion.onClick.AddListener(ShowClassPanel);

        // Créer le perso principal automatiquement (juste un marcheur, sans race/classe)
        var mainChar = new CharacterData();
        mainChar.Initialize("Héros", 100, 50, 10, 10, 10, 10, 10, 5);
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetActiveCharacter(mainChar);
            GameSession.Instance.Gold = 200;
        }

        // Aller directement à la sélection des équipiers
        _step      = Step.PartyMember;
        _partySlot = 0;
        _partyMembers.Clear();

        UpdateHeader();
        SetPanel(racePanel,      true);
        SetPanel(characterPanel, false);
        SetPanel(companionPanel, false);
        BuildRaceList();
    }

    // ── Header ────────────────────────────────────────────────────────────

    private void UpdateHeader()
    {
        if (slotHeaderText == null) return;
        slotHeaderText.text = _step switch
        {
            Step.PartyMember => $"Équipier {_partySlot + 1} / {PartySize}",
            Step.Companion   => "Choix du compagnon",
            _                => ""
        };
    }

    // ── Panel Race ────────────────────────────────────────────────────────

    private void BuildRaceList()
    {
        ClearList(_spawnedRaceButtons);
        if (raceDescriptionText != null) raceDescriptionText.text = "";

        var registry = GameDataRegistry.Instance;
        if (registry == null || registry.races == null || registry.races.Length == 0)
        {
            _pendingRace = null;
            ShowClassPanel();
            return;
        }

        foreach (var raceSO in registry.races)
        {
            if (raceSO == null) continue;
            var captured = raceSO;
            SpawnButton(raceButtonPrefab ?? characterButtonPrefab, raceListContainer,
                _spawnedRaceButtons, raceSO.raceName, () => OnRaceSelected(captured));
        }

        LayoutButtons(raceListContainer, _spawnedRaceButtons);
    }

    private void OnRaceSelected(RaceSO raceSO)
    {
        _pendingRace = raceSO;
        if (raceDescriptionText != null)
            raceDescriptionText.text = raceSO.passiveDescription ?? raceSO.description ?? "";
        ShowClassPanel();
    }

    private void ShowRacePanel()
    {
        ClearList(_spawnedClassButtons);
        SetPanel(characterPanel, false);
        SetPanel(companionPanel, false);
        SetPanel(racePanel,      true);
    }

    // ── Panel Classe ──────────────────────────────────────────────────────

    private void ShowClassPanel()
    {
        ClearList(_spawnedClassButtons);
        if (classDescriptionText != null) classDescriptionText.text = "";

        var registry = GameDataRegistry.Instance;
        if (registry == null || registry.classes == null)
        {
            Debug.LogError("[CharacterSelectUI] GameDataRegistry introuvable.");
            return;
        }

        foreach (var classSO in registry.classes)
        {
            if (classSO == null) continue;
            if (!IsClassAvailableForRace(classSO, _pendingRace)) continue;
            var captured = classSO;
            SpawnButton(characterButtonPrefab, characterListContainer, _spawnedClassButtons,
                classSO.className, () => OnClassSelected(captured));
        }

        LayoutButtons(characterListContainer, _spawnedClassButtons);
        SetPanel(racePanel,      false);
        SetPanel(companionPanel, false);
        SetPanel(characterPanel, true);
    }

    private static bool IsClassAvailableForRace(ClassSO classSO, RaceSO race)
    {
        if (classSO.allowedRaces == null || classSO.allowedRaces.Length == 0) return true;
        if (race == null) return true;
        foreach (var r in classSO.allowedRaces)
            if (r == race) return true;
        return false;
    }

    private void OnClassSelected(ClassSO classSO)
    {
        _pendingClass = classSO;
        if (classDescriptionText != null)
            classDescriptionText.text = classSO.description ?? "";

        if (_step == Step.PartyMember)
            ConfirmPartyMember(classSO);
    }

    // ── Étape 1 : Équipiers de combat (×3) ───────────────────────────────

    private void ConfirmPartyMember(ClassSO classSO)
    {
        var race = ResolveRace();
        if (race == null) return;

        var member = GameSession.CreatePredefinedCharacter(
            classSO.className, classSO, race, level: 1, companionSO: null);

        _partyMembers.Add(member);
        _partySlot++;

        if (_partySlot < PartySize)
        {
            _step = Step.PartyMember;
            UpdateHeader();
            ResetRaceClass();
            ShowRacePanel();
            BuildRaceList();
        }
        else
        {
            // Tous les équipiers choisis → choisir le compagnon
            _step = Step.Companion;
            UpdateHeader();
            ShowCompanionPanel();
        }
    }

    // ── Étape 3 : Compagnon (pour l'équipe) ───────────────────────────────

    private void ShowCompanionPanel()
    {
        ClearList(_spawnedCompanionButtons);
        if (companionDescText != null) companionDescText.text = "";

        var companions = GameDataRegistry.Instance?.companions;
        if (companions != null && companions.Length > 0)
        {
            foreach (var companion in companions)
            {
                if (companion == null) continue;
                var captured = companion;
                SpawnButton(companionButtonPrefab, companionListContainer, _spawnedCompanionButtons,
                    companion.companionName, () =>
                    {
                        if (companionDescText != null)
                            companionDescText.text = captured.description ?? "";
                        SelectCompanion(captured);
                    });
            }
        }
        else
        {
            SpawnButton(companionButtonPrefab, companionListContainer, _spawnedCompanionButtons,
                "Sans compagnon", () =>
                {
                    if (companionDescText != null) companionDescText.text = "Aucun compagnon";
                    SelectCompanion(null);
                });
        }

        LayoutButtons(companionListContainer, _spawnedCompanionButtons);
        SetPanel(racePanel,      false);
        SetPanel(characterPanel, false);
        SetPanel(companionPanel, true);
    }

    private void SelectCompanion(CompanionSO companionSO)
    {
        // Assigner le compagnon au premier équipier
        if (companionSO != null && _partyMembers.Count > 0)
            _partyMembers[0].AssignCompanion(companionSO);

        if (GameSession.Instance != null)
            GameSession.Instance.SetParty(_partyMembers);

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("WorldMap");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private RaceSO ResolveRace()
    {
        var race = _pendingRace;
        if (race == null)
        {
            var races = GameDataRegistry.Instance?.races;
            if (races != null && races.Length > 0) race = races[0];
        }
        if (race == null) Debug.LogError("[CharacterSelectUI] Aucune race disponible.");
        return race;
    }

    private void ResetRaceClass()
    {
        _pendingRace  = null;
        _pendingClass = null;
    }

    private void SpawnButton(GameObject prefab, Transform container, List<GameObject> list,
        string label, System.Action onClick)
    {
        if (container == null) return;

        GameObject go;
        if (prefab != null)
        {
            go = Instantiate(prefab, container);
        }
        else
        {
            go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            go.transform.SetParent(container, false);
            go.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f, 1f);
            go.layer = 5;

            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(go.transform, false);
            lblGO.layer = 5;
            var lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            var tmp = lblGO.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 16f;
            tmp.color     = Color.white;
        }

        list.Add(go);
        var btn  = go.GetComponent<Button>();
        var lbl2 = go.GetComponentInChildren<TMP_Text>();
        if (lbl2 != null) lbl2.text = label;
        if (btn  != null && onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private static void LayoutButtons(Transform container, List<GameObject> buttons)
    {
        if (container == null) return;
        float yOffset = 10f;
        foreach (var go in buttons)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) continue;
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.pivot            = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -yOffset);
            rt.sizeDelta        = new Vector2(0f, 45f);
            yOffset += 50f;
        }
    }

    private static void ClearList(List<GameObject> list)
    {
        foreach (var go in list)
            if (go != null) Destroy(go);
        list.Clear();
    }

    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
