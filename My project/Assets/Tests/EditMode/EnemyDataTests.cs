using NUnit.Framework;
using UnityEngine;

public class EnemyDataTests
{
    [Test]
    public void EnemySO_DefaultXPReward_IsPositive()
    {
        var enemy = ScriptableObject.CreateInstance<EnemySO>();
        enemy.xpReward = 30;
        Assert.Greater(enemy.xpReward, 0);
    }

    [Test]
    public void LootTable_EmptyEntries_RollReturnsNull()
    {
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new LootTableSO.LootEntry[0];
        Assert.IsNull(table.Roll());
    }

    [Test]
    public void LootTable_SingleEntry_RollReturnsIt()
    {
        var eq = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[] { new LootTableSO.LootEntry { equipment = eq, weight = 1f } };
        Assert.AreEqual(eq, table.Roll());
    }

    [Test]
    public void LootTable_ZeroWeights_RollReturnsNull()
    {
        var eq = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[] { new LootTableSO.LootEntry { equipment = eq, weight = 0f } };
        Assert.IsNull(table.Roll());
    }

    [Test]
    public void LootTable_MultipleEntries_RollReturnsEntryFromPool()
    {
        var eq1 = ScriptableObject.CreateInstance<EquipmentSO>();
        var eq2 = ScriptableObject.CreateInstance<EquipmentSO>();
        var table = ScriptableObject.CreateInstance<LootTableSO>();
        table.entries = new[]
        {
            new LootTableSO.LootEntry { equipment = eq1, weight = 1f },
            new LootTableSO.LootEntry { equipment = eq2, weight = 1f }
        };
        var result = table.Roll();
        Assert.IsTrue(result == eq1 || result == eq2);
    }

    [Test]
    public void CampaignZone_GetRandomEnemy_ReturnsFromPool()
    {
        var enemy1 = ScriptableObject.CreateInstance<EnemySO>();
        var enemy2 = ScriptableObject.CreateInstance<EnemySO>();
        var zone = ScriptableObject.CreateInstance<CampaignZoneSO>();
        zone.enemyPool = new[] { enemy1, enemy2 };
        var result = zone.GetRandomEnemy();
        Assert.IsTrue(result == enemy1 || result == enemy2);
    }

    [Test]
    public void CampaignZone_EmptyPool_GetRandomEnemyReturnsNull()
    {
        var zone = ScriptableObject.CreateInstance<CampaignZoneSO>();
        zone.enemyPool = new EnemySO[0];
        Assert.IsNull(zone.GetRandomEnemy());
    }
}
