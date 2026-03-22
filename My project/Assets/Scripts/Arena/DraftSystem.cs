using System;
using System.Collections.Generic;

public class DraftSystem
{
    // Snake order for 3v3: P1, P2, P2, P1, P1, P2
    private static readonly int[] SnakeOrder = { 0, 1, 1, 0, 0, 1 };

    public List<string> AvailableRoster { get; }
    public bool IsComplete => _pickIndex >= SnakeOrder.Length;
    public int  CurrentPickPlayer => IsComplete ? -1 : SnakeOrder[_pickIndex];

    public event Action<int, string> OnPick;

    private readonly Dictionary<int, List<string>> _teams = new()
        { { 0, new List<string>() }, { 1, new List<string>() } };
    private int _pickIndex;

    public DraftSystem(List<string> roster)
    {
        AvailableRoster = new List<string>(roster);
    }

    public void Ban(int playerIndex, string characterName)
    {
        if (!AvailableRoster.Contains(characterName))
            throw new ArgumentException($"'{characterName}' introuvable dans le roster");
        AvailableRoster.Remove(characterName);
    }

    public void Pick(int playerIndex, string characterName)
    {
        if (IsComplete)
            throw new InvalidOperationException("Le draft est terminé");
        if (SnakeOrder[_pickIndex] != playerIndex)
            throw new InvalidOperationException($"C'est au joueur {SnakeOrder[_pickIndex]} de pick");
        if (!AvailableRoster.Contains(characterName))
            throw new ArgumentException($"'{characterName}' non disponible");

        AvailableRoster.Remove(characterName);
        _teams[playerIndex].Add(characterName);
        _pickIndex++;
        OnPick?.Invoke(playerIndex, characterName);
    }

    public List<string> GetTeam(int playerIndex) => _teams[playerIndex];
}
