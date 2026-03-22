using System.IO;
using UnityEngine;

public static class SaveSystem
{
    /// <summary>Inject a path for EditMode tests (null = use default).</summary>
    public static string OverridePath = null;

    private static string GetPath() =>
        OverridePath ?? Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(GetPath(), json);
    }

    public static SaveData Load()
    {
        string path = GetPath();
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave() => File.Exists(GetPath());

    public static void Delete()
    {
        string path = GetPath();
        if (File.Exists(path)) File.Delete(path);
    }
}
