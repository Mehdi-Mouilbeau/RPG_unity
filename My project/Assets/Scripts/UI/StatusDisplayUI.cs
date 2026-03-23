using TMPro;
using UnityEngine;

/// <summary>
/// Affiche les effets de statut actifs d'un combattant.
/// Un composant par combattant, placé sur un GameObject enfant du HUD.
/// </summary>
public class StatusDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private CharacterData _character;

    public void Initialize(CharacterData character)
    {
        _character = character;
        Refresh();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TurnStartedEvent>(OnRefresh);
        EventBus.Subscribe<ActionResolvedEvent>(OnRefresh);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnRefresh);
        EventBus.Unsubscribe<ActionResolvedEvent>(OnRefresh);
    }

    private void OnRefresh<T>(T _) => Refresh();

    private void Refresh()
    {
        if (_character == null || statusText == null) return;

        var sb = new System.Text.StringBuilder();
        foreach (var s in _character.ActiveStatuses)
        {
            string entry = s.type switch
            {
                StatusEffectType.Poison    => $"<color=#9B59B6>● Poison ({s.remainingTurns})</color>",
                StatusEffectType.Burn      => $"<color=#E67E22>● Brûlure ({s.remainingTurns})</color>",
                StatusEffectType.Shield    => "<color=#3498DB>● Bouclier</color>",
                StatusEffectType.Paralysis => $"<color=#F1C40F>● Paralysie ({s.remainingTurns})</color>",
                _                          => string.Empty
            };
            if (!string.IsNullOrEmpty(entry))
            {
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(entry);
            }
        }
        statusText.text = sb.ToString();
    }
}
