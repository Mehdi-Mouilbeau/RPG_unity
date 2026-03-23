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
