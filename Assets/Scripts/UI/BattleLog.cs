using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleLog : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;
    [SerializeField] private int maxLines = 8;
    private readonly Queue<string> _lines = new();

    private void OnEnable()
    {
        EventBus.Subscribe<ActionResolvedEvent>(OnAction);
        EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
        EventBus.Subscribe<StatusAppliedEvent>(OnStatus);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ActionResolvedEvent>(OnAction);
        EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
        EventBus.Unsubscribe<StatusAppliedEvent>(OnStatus);
    }

    private void Log(string message)
    {
        _lines.Enqueue(message);
        if (_lines.Count > maxLines) _lines.Dequeue();
        if (logText != null)
            logText.text = string.Join("\n", _lines);
    }

    private void OnAction(ActionResolvedEvent e)     => Log(e.Result.Description);
    private void OnStatus(StatusAppliedEvent e)      => Log($"{e.Target.CharacterName} : {e.Status.type} !");
    private void OnBattleEnded(BattleEndedEvent e)   => Log(e.PlayerWon ? "=== VICTOIRE ===" : "=== DÉFAITE ===");
}
