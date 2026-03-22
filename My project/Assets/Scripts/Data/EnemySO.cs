using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RPG/Campaign/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identité")]
    public string enemyName;
    public ElementType elementalAffinity;

    [Header("Stats")]
    public int hp  = 100;
    public int mp  = 0;
    public int atk = 10;
    public int def = 5;
    public int mag = 5;
    public int res = 5;
    public int agi = 8;
    public int lck = 3;

    [Header("Combat")]
    public BotBrain botBrain;
    public int xpReward = 30;

    [Header("Loot")]
    public LootTableSO lootTable;
}
