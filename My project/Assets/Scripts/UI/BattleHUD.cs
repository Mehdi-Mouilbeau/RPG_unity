using TMPro;
using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text[] playerNameTexts;
    [SerializeField] private TMP_Text[] playerHPTexts;
    [SerializeField] private TMP_Text[] playerMPTexts;
    [SerializeField] private TMP_Text[] enemyNameTexts;
    [SerializeField] private TMP_Text[] enemyHPTexts;
    [SerializeField] private TMP_Text activeTurnIndicator;

    [Header("Status Display")]
    [SerializeField] private StatusDisplayUI[] playerStatusDisplays;
    [SerializeField] private StatusDisplayUI[] enemyStatusDisplays;

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
            SetHPText(playerHPTexts[i], players[i].CurrentHP, players[i].MaxHP);
            SetMPText(playerMPTexts[i], players[i].CurrentMP, players[i].MaxMP);
        }

        for (int i = 0; i < enemies.Count && i < enemyNameTexts.Length; i++)
        {
            enemyNameTexts[i].text = enemies[i].CharacterName;
            SetHPText(enemyHPTexts[i], enemies[i].CurrentHP, enemies[i].MaxHP);
        }
    }

    /// <summary>Appelé une fois au démarrage après StartBattle.</summary>
    public void InitializeStatusDisplays(BattleManager battle)
    {
        var players = battle.GetPlayerTeam();
        var enemies = battle.GetEnemyTeam();

        for (int i = 0; i < players.Count && i < playerStatusDisplays.Length; i++)
            if (playerStatusDisplays[i] != null)
                playerStatusDisplays[i].Initialize(players[i]);

        for (int i = 0; i < enemies.Count && i < enemyStatusDisplays.Length; i++)
            if (enemyStatusDisplays[i] != null)
                enemyStatusDisplays[i].Initialize(enemies[i]);
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
            activeTurnIndicator.gameObject.SetActive(false);
    }

    private static void SetHPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text = $"HP: {current}/{max}";
        label.color = GetHealthColor(max > 0 ? (float)current / max : 0f);
    }

    private static void SetMPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text = $"MP: {current}/{max}";
        label.color = GetMPColor(max > 0 ? (float)current / max : 0f);
    }

    private static Color GetHealthColor(float ratio)
    {
        if (ratio > 0.5f) return Color.white;
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }

    private static Color GetMPColor(float ratio)
    {
        if (ratio > 0.5f) return new Color(0.2f, 0.6f, 1f);
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }
}
