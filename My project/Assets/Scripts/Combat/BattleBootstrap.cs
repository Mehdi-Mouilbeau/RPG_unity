using System.Collections.Generic;
using UnityEngine;

public class BattleBootstrap : MonoBehaviour
{
    [SerializeField] private ClassSO playerClass;
    [SerializeField] private RaceSO  playerRace;
    [SerializeField] private ClassSO enemyClass;
    [SerializeField] private RaceSO  enemyRace;
    [SerializeField] private BotBrain enemyBotBrain;

    private void Start()
    {
        var player = new CharacterData();
        player.InitializeFromSO("Héros", playerClass, playerRace, level: 1);

        var enemy = new CharacterData();
        enemy.InitializeFromSO("Ennemi", enemyClass, enemyRace, level: 1);

        BattleManager.Instance.StartBattle(
            new List<CharacterData> { player },
            new List<CharacterData> { enemy },
            enemyBotBrain);
    }
}
