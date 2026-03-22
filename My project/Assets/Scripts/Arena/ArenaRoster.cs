using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArenaRoster", menuName = "RPG/Arena Roster")]
public class ArenaRoster : ScriptableObject
{
    [System.Serializable]
    public class RosterEntry
    {
        public string  displayName;
        public ClassSO characterClass;
        public RaceSO  characterRace;
    }

    public List<RosterEntry> entries = new();

    public List<string> GetNames() => entries.ConvertAll(e => e.displayName);

    public CharacterData CreateCharacter(string displayName, int level = 5)
    {
        var entry = entries.Find(e => e.displayName == displayName);
        if (entry == null)
        {
            Debug.LogWarning($"ArenaRoster: '{displayName}' introuvable");
            return null;
        }
        var c = new CharacterData();
        c.InitializeFromSO(displayName, entry.characterClass, entry.characterRace, level);
        return c;
    }
}
