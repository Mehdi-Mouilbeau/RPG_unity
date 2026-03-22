using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    [SerializeField] private Button attackButton;
    [SerializeField] private Button passButton;
    // Placeholder fields for Plan 2 — skills/items submenus not yet implemented
    [SerializeField] private Button skillsButton;
    [SerializeField] private Button itemsButton;
    [SerializeField] private TMP_Text playerTurnLabel;

    private System.Action<BattleEndedEvent> _onBattleEnded;

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackPressed);
        passButton.onClick.AddListener(OnPassPressed);
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
        _onBattleEnded = _ => SetMenuActive(false);
        EventBus.Subscribe(_onBattleEnded);

        // Sync visibility in case TurnStartedEvent fired before we subscribed
        bool isPlayerTurn = BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn;
        SetMenuActive(isPlayerTurn);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEvent>(OnPlayerTurn);
        EventBus.Unsubscribe(_onBattleEnded);
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        bool isPlayerTurn = BattleManager.Instance != null && BattleManager.Instance.IsPlayerTurn;
        SetMenuActive(isPlayerTurn);
    }

    private void OnAttackPressed()
    {
        var enemies = BattleManager.Instance.GetAliveEnemies();
        if (enemies.Count > 0)
            BattleManager.Instance.ExecuteAction(enemies[0]);
        SetMenuActive(false);
    }

    private void OnPassPressed()
    {
        BattleManager.Instance.Pass();
        SetMenuActive(false);
    }

    private void OnPlayerTurn(PlayerTurnEvent e)
    {
        if (playerTurnLabel == null) return;
        playerTurnLabel.text = e.PlayerIndex switch
        {
            0 => "JOUEUR 1",
            1 => "JOUEUR 2",
            _ => ""
        };
    }

    private void SetMenuActive(bool active) => gameObject.SetActive(active);
}
