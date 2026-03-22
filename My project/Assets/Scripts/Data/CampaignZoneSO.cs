using UnityEngine;

[CreateAssetMenu(fileName = "NewZone", menuName = "RPG/Campaign/Zone")]
public class CampaignZoneSO : ScriptableObject
{
    [Header("Identité")]
    public string zoneName;
    public string sceneKey;         // nom de la scène Unity à charger
    public string flagKey;          // clé dans ProgressionFlags pour "zone visitée"

    [Header("Rencontres aléatoires")]
    public EnemySO[] enemyPool;
    public BotBrain defaultBotBrain;

    [Header("Boss")]
    public EnemySO boss;            // null si pas de boss
    public string bossDefeatedFlagKey;

    public EnemySO GetRandomEnemy()
    {
        if (enemyPool == null || enemyPool.Length == 0) return null;
        return enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
    }
}
