using UnityEngine;

/// <summary>
/// Registre central pour résoudre les clés string en ScriptableObjects lors du chargement de sauvegarde.
/// Placé dans Assets/Resources/GameDataRegistry.asset pour être chargé via Resources.Load.
/// </summary>
[CreateAssetMenu(fileName = "GameDataRegistry", menuName = "RPG/GameDataRegistry")]
public class GameDataRegistry : ScriptableObject
{
    [Header("Classes")]
    public ClassSO[] classes;

    [Header("Races")]
    public RaceSO[] races;

    [Header("Equipment")]
    public EquipmentSO[] equipment;

    [Header("Companions")]
    public CompanionSO[] companions;

    private static GameDataRegistry _instance;

    public static GameDataRegistry Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GameDataRegistry>("GameDataRegistry");
            return _instance;
        }
    }

    public ClassSO GetClass(string key)
    {
        if (classes == null || key == null) return null;
        foreach (var c in classes)
            if (c != null && c.className == key) return c;
        return null;
    }

    public RaceSO GetRace(string key)
    {
        if (races == null || key == null) return null;
        foreach (var r in races)
            if (r != null && r.raceName == key) return r;
        return null;
    }

    public EquipmentSO GetEquipment(string key)
    {
        if (equipment == null || key == null) return null;
        foreach (var e in equipment)
            if (e != null && e.itemName == key) return e;
        return null;
    }

    public CompanionSO GetCompanion(string key)
    {
        if (companions == null || key == null) return null;
        foreach (var c in companions)
            if (c != null && c.companionName == key) return c;
        return null;
    }

    [Header("Consumables")]
    public ConsumableSO[] consumables;

    public ConsumableSO GetConsumable(string key)
    {
        if (consumables == null || key == null) return null;
        foreach (var c in consumables)
            if (c != null && c.itemName == key) return c;
        return null;
    }
}
