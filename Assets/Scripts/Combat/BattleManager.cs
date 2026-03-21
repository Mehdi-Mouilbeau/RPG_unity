using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private List<CharacterData> _playerTeam  = new();
    private List<CharacterData> _enemyTeam   = new();
    private TurnSystem _turnSystem;

    public bool IsPlayerTurn => _playerTeam.Contains(_turnSystem?.CurrentCharacter);
    public CharacterData ActiveCharacter => _turnSystem?.CurrentCharacter;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartBattle(List<CharacterData> players, List<CharacterData> enemies)
    {
        _playerTeam = players;
        _enemyTeam  = enemies;
        var allChars = new List<CharacterData>(players);
        allChars.AddRange(enemies);
        _turnSystem = new TurnSystem(allChars);
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
        StatusManager.Tick(source);
        ActionResult result = skill == null
            ? ActionResolver.ResolveBasicAttack(source, target)
            : ActionResolver.ResolveSkill(source, target, skill);
        Debug.Log(result.Description);
        if (CheckBattleEnd()) return;
        EndCurrentTurn();
    }

    public void Pass()
    {
        StatusManager.Tick(_turnSystem.CurrentCharacter);
        EndCurrentTurn();
    }

    private void EndCurrentTurn()
    {
        _turnSystem.NextTurn();
    }

    private bool CheckBattleEnd()
    {
        bool playersAllDead = _playerTeam.All(c => c.IsDead);
        bool enemiesAllDead = _enemyTeam.All(c => c.IsDead);
        if (playersAllDead || enemiesAllDead)
        {
            EventBus.Publish(new BattleEndedEvent { PlayerWon = enemiesAllDead });
            return true;
        }
        return false;
    }

    public List<CharacterData> GetEnemyTeam() => _enemyTeam;
    public List<CharacterData> GetPlayerTeam() => _playerTeam;
    public List<CharacterData> GetAliveEnemies() => _enemyTeam.Where(c => !c.IsDead).ToList();
    public List<CharacterData> GetAliveAllies()  => _playerTeam.Where(c => !c.IsDead).ToList();
}
