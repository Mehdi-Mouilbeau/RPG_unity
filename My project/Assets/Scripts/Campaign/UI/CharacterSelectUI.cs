using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [System.Serializable]
    public struct PredefinedCharacterConfig
    {
        public string      characterName;
        public ClassSO     classSO;
        public RaceSO      raceSO;
        public int         level;
        public CompanionSO companionSO;
        [TextArea] public string description;
    }

    [SerializeField] private PredefinedCharacterConfig[] characters;
    [SerializeField] private Button[]   selectButtons;
    [SerializeField] private TMP_Text   descriptionText;

    private void Start()
    {
        for (int i = 0; i < selectButtons.Length && i < characters.Length; i++)
        {
            int index = i;
            selectButtons[i].onClick.AddListener(() => SelectCharacter(index));
        }
    }

    private void SelectCharacter(int index)
    {
        var config = characters[index];
        var character = GameSession.CreatePredefinedCharacter(
            config.characterName,
            config.classSO,
            config.raceSO,
            config.level,
            config.companionSO);

        GameSession.Instance.SetActiveCharacter(character);
        GameSession.Instance.Gold = 200;
        SceneLoader.Instance.LoadScene("WorldMap");
    }

    public void ShowDescription(int index)
    {
        if (descriptionText != null && index < characters.Length)
            descriptionText.text = characters[index].description;
    }
}
