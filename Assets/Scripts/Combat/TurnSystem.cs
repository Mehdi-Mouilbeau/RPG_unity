using System.Collections.Generic;
using System.Linq;

public class TurnSystem
{
    private List<CharacterData> _order;
    private int _currentIndex;

    public CharacterData CurrentCharacter => _order[_currentIndex];
    public int TurnNumber { get; private set; } = 1;
    private int _totalCharacters;
    private int _stepsThisRound = 0;

    public TurnSystem(List<CharacterData> characters)
    {
        _order = characters.OrderByDescending(c => c.AGI).ToList();
        _currentIndex = 0;
        _totalCharacters = _order.Count;
        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public void NextTurn()
    {
        EventBus.Publish(new TurnEndedEvent { Character = CurrentCharacter });
        int steps = 0;
        do
        {
            _currentIndex = (_currentIndex + 1) % _order.Count;
            _stepsThisRound++;
            steps++;
            if (_stepsThisRound >= _totalCharacters)
            {
                TurnNumber++;
                _stepsThisRound = 0;
            }
        }
        while (CurrentCharacter.IsDead && steps < _order.Count);
        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public List<CharacterData> GetAliveCharacters() =>
        _order.Where(c => !c.IsDead).ToList();
}
