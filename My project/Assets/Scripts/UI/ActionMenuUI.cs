using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    [SerializeField] private Button     attackButton;
    [SerializeField] private Button     passButton;
    [SerializeField] private Button     skillsButton;
    [SerializeField] private Button     itemsButton;
    [SerializeField] private Button     companionButton;
    [SerializeField] private SkillMenuUI    skillMenu;
    [SerializeField] private ItemMenuUI     itemMenu;
    [SerializeField] private CompanionMenuUI companionMenu;
    [SerializeField] private TMP_Text   playerTurnLabel;

    private System.Action<BattleEndedEvent> _onBattleEnded;

    private void Start()
    {
        attackButton.onClick.AddListener(OnAttackPressed);
        passButton.onClick.AddListener(OnPassPressed);
        if (skillsButton   != null) skillsButton.onClick.AddListener(OnSkillsPressed);
        if (itemsButton    != null) itemsButton.onClick.AddListener(OnItemsPressed);
        if (companionButton != null) companionButton.onClick.AddListener(OnCompanionPressed);

        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<PlayerTurnEvent>(OnPlayerTurn);
        _onBattleEnded = _ => SetMenuActive(false);
        EventBus.Subscribe(_onBattleEnded);

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

        if (isPlayerTurn) RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        if (itemsButton != null)
            itemsButton.interactable = character.Inventory.Consumables.Count > 0;

        if (companionButton != null)
            companionButton.interactable = character.Companion != null;
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

    private void OnSkillsPressed()
    {
        if (skillMenu != null) skillMenu.Show();
        SetMenuActive(false);
    }

    private void OnItemsPressed()
    {
        if (itemMenu != null) itemMenu.Show();
        SetMenuActive(false);
    }

    private void OnCompanionPressed()
    {
        if (companionMenu != null) companionMenu.Show();
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
