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
        var player    = session.ActiveCharacter;

        if (player == null)
        {
            Debug.LogError("BattleCampaignBridge: ActiveCharacter est null.");
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
            enemies.Add(enemy);
            brain = enemySO.botBrain ?? fallbackBotBrain;
        }

        BattleManager.Instance.StartBattle(
            new List<CharacterData> { player }, enemies, brain);

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

            // Unsubscribe onTurn to avoid leaking the boss turn listener
            if (onTurn != null)
                EventBus.Unsubscribe<TurnStartedEvent>(onTurn);

            if (evt.PlayerWon)
            {
                if (encounter.IsBoss)
                    EventBus.Publish(new BossDefeatedEvent { Player = player });

                if (encounter.IsBoss && encounter.Zone != null
                    && !string.IsNullOrEmpty(encounter.Zone.bossDefeatedFlagKey))
                    session.Flags.Set(encounter.Zone.bossDefeatedFlagKey);

                session.Save();
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene("WorldMap");
            }
            else
            {
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene("MainMenu");
            }
        };
        EventBus.Subscribe<BattleEndedEvent>(onEnd);
    }
}
