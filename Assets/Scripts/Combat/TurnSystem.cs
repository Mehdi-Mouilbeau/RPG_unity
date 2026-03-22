using System.Collections.Generic;
using System.Linq;

public class TurnSystem
{
    private List<CharacterData> _order;
    private int _currentIndex;

    public CharacterData CurrentCharacter => _order[_currentIndex];
    public int TurnNumber { get; private set; } = 1;

    public TurnSystem(List<CharacterData> characters)
    {
        _order = characters.OrderByDescending(c => c.AGI).ToList();
        _currentIndex = 0;
        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public void NextTurn()
    {
        EventBus.Publish(new TurnEndedEvent { Character = CurrentCharacter });

        int steps = 0;
        do
        {
            _currentIndex = (_currentIndex + 1) % _order.Count;
            steps++;
            // A full round has elapsed when we wrap back to index 0
            if (_currentIndex == 0) TurnNumber++;
        }
        while (CurrentCharacter.IsDead && steps < _order.Count);

        // Guard: if all characters are dead, log and return without publishing
        if (CurrentCharacter.IsDead)
        {
            UnityEngine.Debug.LogWarning("TurnSystem.NextTurn: all characters are dead.");
            return;
        }

        EventBus.Publish(new TurnStartedEvent { Character = CurrentCharacter });
    }

    public List<CharacterData> GetAliveCharacters() =>
        _order.Where(c => !c.IsDead).ToList();
}
