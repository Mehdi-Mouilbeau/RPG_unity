using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string characterName;
    public string classKey;
    public string raceKey;
    public int    level;
    public int    experience;
    public int    currentHP;
    public int    currentMP;
    public int    gold;
    public string companionKey;

    // List instead of Dictionary because Dictionary is not serializable by JsonUtility
    public List<ProgressionFlags.FlagEntry> flags            = new List<ProgressionFlags.FlagEntry>();
    public List<string>                     equippedItemKeys  = new List<string>();
    public List<string>                     consumableKeys    = new List<string>();

    // Équipe de combat (3 personnages)
    public List<PartyMemberSaveData> party = new List<PartyMemberSaveData>();
}

[Serializable]
public class PartyMemberSaveData
{
    public string characterName;
    public string classKey;
    public string raceKey;
    public int    level;
    public int    experience;
    public int    currentHP;
    public int    currentMP;
}
