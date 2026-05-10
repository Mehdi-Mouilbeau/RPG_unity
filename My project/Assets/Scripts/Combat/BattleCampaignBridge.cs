using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pont entre la campagne (GameSession.PendingEncounter) et BattleManager.
/// Remplace BattleBootstrap quand on joue en mode campagne.
/// </summary>
public class BattleCampaignBridge : MonoBehaviour
{
    [SerializeField] private BotBrain       fallbackBotBrain;
    [SerializeField] private BossController bossController;

    private void Start()
    {
        var session = GameSession.Instance;
        if (session == null || session.PendingEncounter == null)
        {
            // Pas de session campagne — BattleBootstrap prend le relais
            return;
        }

        var encounter = session.PendingEncounter;

        // Seuls les équipiers combattent — le perso principal reste sur la map
        var allies = session.Party;
        if (allies == null || allies.Count == 0)
        {
            Debug.LogError("BattleCampaignBridge: aucun équipier dans session.Party.");
            return;
        }

        // Construire les ennemis depuis les EnemySO
        var enemies = new List<CharacterData>();
        BotBrain brain = null;

        foreach (var enemySO in encounter.Enemies)
        {
            var enemy = new CharacterData();
            enemy.Initialize(enemySO.enemyName,
                enemySO.hp, enemySO.mp,
                enemySO.atk, enemySO.def,
                enemySO.mag, enemySO.res,
                enemySO.agi, enemySO.lck);
            enemy.XPReward        = enemySO.xpReward;
            enemy.SourceLootTable = enemySO.lootTable;
            enemies.Add(enemy);
            brain = enemySO.botBrain ?? fallbackBotBrain;
        }

        BattleManager.Instance.StartBattle(allies, enemies, brain);

        // Si c'est un boss, initialiser BossController sur le premier ennemi
        CharacterData bossChar = null;
        System.Action<TurnStartedEvent> onTurn = null;

        if (encounter.IsBoss && bossController != null && enemies.Count > 0)
        {
            bossChar = enemies[0];
            bossController.Initialize(bossChar);

            // Câbler Cri des Morts : au début de chaque tour du boss en phase 2
            onTurn = turnEvt =>
            {
                if (turnEvt.Character == bossChar)
                {
                    var aliveAllies = BattleManager.Instance.GetAliveAllies();
                    bossController.TryUseCriDesMorts(aliveAllies.ToArray());
                }
            };
            EventBus.Subscribe<TurnStartedEvent>(onTurn);
        }

        session.PendingEncounter = null;

        // S'abonner à la fin du combat pour auto-save et retour WorldMap
        System.Action<BattleEndedEvent> onEnd = null;
        onEnd = evt =>
        {
            EventBus.Unsubscribe<BattleEndedEvent>(onEnd);

            if (onTurn != null)
                EventBus.Unsubscribe<TurnStartedEvent>(onTurn);

            if (evt.PlayerWon)
            {
                if (encounter.IsBoss)
                    EventBus.Publish(new BossDefeatedEvent { Player = allies[0] });

                if (encounter.IsBoss && encounter.Zone != null
                    && !string.IsNullOrEmpty(encounter.Zone.bossDefeatedFlagKey))
                    session.Flags.Set(encounter.Zone.bossDefeatedFlagKey);

                // Marquer un ennemi de scène comme vaincu (EnemyActor)
                if (!string.IsNullOrEmpty(encounter.DefeatedFlagKey))
                    session.Flags.Set(encounter.DefeatedFlagKey);

                // XP distribuée à tous les équipiers
                foreach (var member in session.Party)
                    member.GainXP(evt.XPGained);

                session.Save();
                // La transition de scène est maintenant gérée par VictoryScreenUI
            }
            // Pas de LoadScene en cas de défaite non plus — VictoryScreenUI s'en charge
        };
        EventBus.Subscribe<BattleEndedEvent>(onEnd);
    }
}
