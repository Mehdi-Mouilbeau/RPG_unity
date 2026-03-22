using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [SerializeField] private BotBrain defaultBotBrain;

    private List<CharacterData> _team0 = new();
    private List<CharacterData> _team1 = new();
    private bool _team1IsBot;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
    }

    private void OnDestroy() =>
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);

    /// <summary>Launches an arena match. team1IsBot = true if P2 is bot-controlled.</summary>
    public void StartArena(List<CharacterData> team0, List<CharacterData> team1,
        bool team1IsBot = false, BotBrain botBrain = null)
    {
        _team0      = team0;
        _team1      = team1;
        _team1IsBot = team1IsBot;

        BattleManager.Instance.StartBattle(team0, team1,
            team1IsBot ? (botBrain ?? defaultBotBrain) : null);
    }

    /// <returns>0 = P1, 1 = P2, -1 = bot</returns>
    public int GetPlayerIndex(CharacterData character)
    {
        if (_team0.Contains(character)) return 0;
        if (_team1.Contains(character)) return _team1IsBot ? -1 : 1;
        return -1;
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        int idx = GetPlayerIndex(e.Character);
        EventBus.Publish(new PlayerTurnEvent { PlayerIndex = idx, Character = e.Character });
    }
}
