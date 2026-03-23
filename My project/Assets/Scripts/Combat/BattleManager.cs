using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private List<CharacterData> _playerTeam  = new();
    private List<CharacterData> _enemyTeam   = new();
    private TurnSystem _turnSystem;
    private BotBrain _enemyBotBrain;

    public bool IsPlayerTurn => _playerTeam.Contains(_turnSystem?.CurrentCharacter);
    public CharacterData ActiveCharacter => _turnSystem?.CurrentCharacter;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
    }

    public void StartBattle(List<CharacterData> players, List<CharacterData> enemies,
        BotBrain enemyBrain = null)
    {
        _playerTeam    = players;
        _enemyTeam     = enemies;
        _enemyBotBrain = enemyBrain;
        var allChars = new List<CharacterData>(players);
        allChars.AddRange(enemies);
        _turnSystem = new TurnSystem(allChars);
    }

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (!IsPlayerTurn)
            StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        BotAction action;
        if (_enemyBotBrain != null)
            action = _enemyBotBrain.Decide(ActiveCharacter, _enemyTeam, _playerTeam);
        else
            action = new BotAction(GetAliveAllies().FirstOrDefault());

        if (action?.Target != null)
        {
            if (action.Skill != null) ExecuteAction(action.Target, action.Skill);
            else                      ExecuteAction(action.Target);
        }
        else Pass();
    }

    public void ExecuteAction(CharacterData target, SkillSO skill = null)
    {
        var source = _turnSystem.CurrentCharacter;
        if (!StatusManager.CanAct(source))
        {
            Debug.Log($"{source.CharacterName} ne peut pas agir ce tour !");
            EndCurrentTurn();
            return;
        }

        // Tick status effects — may kill characters (Burn/Poison)
        StatusManager.Tick(source);
        if (CheckBattleEnd()) return;

        ActionResult result = skill == null
            ? ActionResolver.ResolveBasicAttack(source, target)
            : ActionResolver.ResolveSkill(source, target, skill);
        Debug.Log(result.Description);

        if (CheckBattleEnd()) return;
        EndCurrentTurn();
    }

    public void Pass()
    {
        // Tick status effects — may kill characters (Burn/Poison)
        StatusManager.Tick(_turnSystem.CurrentCharacter);
        if (CheckBattleEnd()) return;
        EndCurrentTurn();
    }

    private void EndCurrentTurn()
    {
        // Note: TurnSystem.NextTurn() publishes TurnStartedEvent for the next character
        _turnSystem.NextTurn();
    }

    private bool CheckBattleEnd()
    {
        bool playersAllDead = _playerTeam.All(c => c.IsDead);
        bool enemiesAllDead = _enemyTeam.All(c => c.IsDead);
        if (!playersAllDead && !enemiesAllDead) return false;

        int xp = 0;
        var loot = new List<EquipmentSO>();

        if (enemiesAllDead)
        {
            foreach (var enemy in _enemyTeam)
            {
                xp += enemy.XPReward;
                if (enemy.SourceLootTable != null)
                {
                    var item = enemy.SourceLootTable.Roll();
                    if (item != null) loot.Add(item);
                }
            }
        }

        EventBus.Publish(new BattleEndedEvent
        {
            PlayerWon = enemiesAllDead,
            XPGained  = xp,
            Loot      = loot
        });
        return true;
    }

    public List<CharacterData> GetEnemyTeam() => _enemyTeam;
    public List<CharacterData> GetPlayerTeam() => _playerTeam;
    public List<CharacterData> GetAliveEnemies() => _enemyTeam.Where(c => !c.IsDead).ToList();
    public List<CharacterData> GetAliveAllies()  => _playerTeam.Where(c => !c.IsDead).ToList();
}
