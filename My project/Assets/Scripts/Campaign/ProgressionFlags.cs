using System;
using System.Collections.Generic;

public class ProgressionFlags
{
    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

    public void Set(string key)
    {
        if (key == null) return;
        _flags[key] = true;
    }

    public bool IsSet(string key)
    {
        if (key == null) return false;
        return _flags.TryGetValue(key, out bool val) && val;
    }

    public void Reset()
    {
        _flags.Clear();
    }

    // ── Sérialisation (pour SaveSystem) ───────────────────────────────────

    [Serializable]
    public struct FlagEntry
    {
        public string key;
        public bool value;
    }

    public List<FlagEntry> GetAllAsList()
    {
        var list = new List<FlagEntry>();
        foreach (var kv in _flags)
            list.Add(new FlagEntry { key = kv.Key, value = kv.Value });
        return list;
    }

    public void LoadFrom(List<FlagEntry> entries)
    {
        _flags.Clear();
        if (entries == null) return;
        foreach (var e in entries)
            if (e.key != null) _flags[e.key] = e.value;
    }
}
