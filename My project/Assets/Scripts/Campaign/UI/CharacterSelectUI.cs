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
        foreach (var go in _spawnedCompanionButtons)
            if (go != null) Destroy(go);
        _spawnedCompanionButtons.Clear();

        if (companionDescText != null) companionDescText.text = "";

        if (characterPanel != null) characterPanel.SetActive(false);
        if (companionPanel  != null) companionPanel.SetActive(true);

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
