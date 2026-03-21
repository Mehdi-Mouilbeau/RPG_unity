using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    [SerializeField] private Button attackButton;
    [SerializeField] private Button passButton;

    private System.Action<BattleEndedEvent> _onBattleEnded;

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackPressed);
        passButton.onClick.AddListener(OnPassPressed);
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        _onBattleEnded = _ => SetMenuActive(false);
        EventBus.Subscribe(_onBattleEnded);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
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

    private void SetMenuActive(bool active) => gameObject.SetActive(active);
}
