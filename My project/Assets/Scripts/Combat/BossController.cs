using System.Linq;
using UnityEngine;

/// <summary>
/// MonoBehaviour attaché dans la scène Battle pour gérer les phases du boss.
/// Délègue la logique à BossPhaseLogic (pur C#).
/// </summary>
public class BossController : MonoBehaviour
{
    private BossPhaseLogic _logic;
    private CharacterData  _boss;
    private bool           _cryUsed;

    private System.Action<ActionResolvedEvent> _onAction;

    public void Initialize(CharacterData boss)
    {
        _boss  = boss;
        _logic = new BossPhaseLogic(maxHP: boss.MaxHP, phaseThreshold: 0.5f);

        _onAction = evt => OnActionResolved(evt);
        EventBus.Subscribe<ActionResolvedEvent>(_onAction);
    }

    private void OnDestroy()
    {
        if (_onAction != null)
            EventBus.Unsubscribe<ActionResolvedEvent>(_onAction);
    }

    private void OnActionResolved(ActionResolvedEvent evt)
    {
        if (_boss == null || _logic == null) return;

        bool transitioned = _logic.CheckAndTransition(_boss.CurrentHP);
        if (!transitioned) return;

        // Armure de Crâne : bouclier = 30% HP max
        int shieldValue = Mathf.RoundToInt(_boss.MaxHP * 0.3f);
        var shieldStatus = new StatusEffect
        {
            type      = StatusEffectType.Shield,
            duration  = 999,
            shieldHP  = shieldValue
        };
        _boss.ActiveStatuses.Add(shieldStatus);

        // ATK +25% via AddBaseATKBonus
        int atkBoost = Mathf.RoundToInt(_boss.ATK * (_logic.Phase2ATKMultiplier - 1f));
        _boss.AddBaseATKBonus(atkBoost);

        EventBus.Publish(new BossPhaseEvent { Phase = 2, Boss = _boss });

        Debug.Log($"[Boss] {_boss.CharacterName} passe en Phase 2 ! Bouclier activé ({shieldValue} HP).");
    }

    /// <summary>
    /// Appelé par BattleCampaignBridge quand c'est le tour du boss en phase 2.
    /// Retourne true si Cri des Morts a été déclenché ce tour.
    /// </summary>
    public bool TryUseCriDesMorts(CharacterData[] targets)
    {
        if (!_logic.Phase2Active || _cryUsed || targets == null) return false;

        _cryUsed = true;
        foreach (var target in targets.Where(t => t != null && !t.IsDead))
        {
            var poison = new StatusEffect
            {
                type     = StatusEffectType.Poison,
                duration = 3
            };
            target.ActiveStatuses.Add(poison);
        }

        Debug.Log("[Boss] Cri des Morts : tous les alliés du joueur sont empoisonnés !");
        return true;
    }
}
