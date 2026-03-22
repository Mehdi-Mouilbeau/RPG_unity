using UnityEngine;
using TMPro;

public class ArenaHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text  p1ActiveLabel;  // "► JOUEUR 1"
    [SerializeField] private TMP_Text  p2ActiveLabel;  // "► JOUEUR 2"
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text  resultText;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
        EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerTurnEvent>(OnPlayerTurn);
        EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
    }

    private void Start()
    {
        if (resultPanel) resultPanel.SetActive(false);
        SetActivePlayer(-1);
    }

    private void OnPlayerTurn(PlayerTurnEvent e) => SetActivePlayer(e.PlayerIndex);

    private void OnBattleEnded(BattleEndedEvent e)
    {
        SetActivePlayer(-1);
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText)
            resultText.text = e.PlayerWon ? "JOUEUR 1 GAGNE !" : "JOUEUR 2 GAGNE !";
    }

    private void SetActivePlayer(int playerIndex)
    {
        if (p1ActiveLabel) p1ActiveLabel.gameObject.SetActive(playerIndex == 0);
        if (p2ActiveLabel) p2ActiveLabel.gameObject.SetActive(playerIndex == 1);
    }
}
