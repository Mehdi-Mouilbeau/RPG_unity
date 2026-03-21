using UnityEngine;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text[] playerNameTexts;
    [SerializeField] private TMP_Text[] playerHPTexts;
    [SerializeField] private TMP_Text[] playerMPTexts;
    [SerializeField] private TMP_Text[] enemyNameTexts;
    [SerializeField] private TMP_Text[] enemyHPTexts;
    [SerializeField] private TMP_Text activeTurnIndicator;

    private void OnEnable()
    {
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<ActionResolvedEvent>(OnActionResolved);
        EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Unsubscribe<ActionResolvedEvent>(OnActionResolved);
        EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
    }

    public void RefreshAll(BattleManager battle)
    {
        var players = battle.GetPlayerTeam();
        var enemies = battle.GetEnemyTeam();

        for (int i = 0; i < players.Count && i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = players[i].CharacterName;
            playerHPTexts[i].text   = $"HP: {players[i].CurrentHP}/{players[i].MaxHP}";
            playerMPTexts[i].text   = $"MP: {players[i].CurrentMP}/{players[i].MaxMP}";
        }

        for (int i = 0; i < enemies.Count && i < enemyNameTexts.Length; i++)
        {
            enemyNameTexts[i].text = enemies[i].CharacterName;
            enemyHPTexts[i].text   = $"HP: {enemies[i].CurrentHP}/{enemies[i].MaxHP}";
        }
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (activeTurnIndicator != null)
            activeTurnIndicator.text = $"Tour de : {e.Character.CharacterName}";
        if (BattleManager.Instance != null)
            RefreshAll(BattleManager.Instance);
    }

    private void OnActionResolved(ActionResolvedEvent e)
    {
        if (BattleManager.Instance != null)
            RefreshAll(BattleManager.Instance);
    }

    private void OnBattleEnded(BattleEndedEvent e)
    {
        if (activeTurnIndicator != null)
            activeTurnIndicator.text = e.PlayerWon ? "Victoire !" : "Défaite...";
    }
}
