using System.Collections.Generic;
using System.Linq;

public class TurnSystem
{
    private List<CharacterData> _order;
    private int _currentIndex;
    private int _turnsThisRound;

    public CharacterData CurrentCharacter => _order[_currentIndex];
    public int TurnNumber { get; private set; } = 1;

    public TurnSystem(List<CharacterData> characters)
    {
        _order = characters.OrderByDescending(c => c.AGI).ToList();
        _currentIndex = 0;
        _turnsThisRound = 0;
        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public void NextTurn()
    {
        EventBus.Publish(new TurnEndedEvent { Character = CurrentCharacter });

        _turnsThisRound++;
        int aliveCount = _order.Count(c => !c.IsDead);

        // A full round has elapsed when all living characters have had a turn
        if (aliveCount > 0 && _turnsThisRound >= aliveCount)
        {
            TurnNumber++;
            _turnsThisRound = 0;
        }

        int steps = 0;
        do
        {
            _currentIndex = (_currentIndex + 1) % _order.Count;
            steps++;
        }
        while (CurrentCharacter.IsDead && steps < _order.Count);

        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public List<CharacterData> GetAliveCharacters() =>
        _order.Where(c => !c.IsDead).ToList();
}
