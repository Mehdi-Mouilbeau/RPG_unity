using NUnit.Framework;
using UnityEngine;

public class CharacterProgressionTests
{
    private CharacterData MakeChar(int level = 1)
    {
        var c = new CharacterData();
        c.Initialize("Héros", 100, 50, 10, 10, 10, 10, 10, 5);
        if (level > 1)
            c.GainXP(XPSystem.CumulativeXPForLevel(level));
        return c;
    }

    [Test]
    public void GainXP_BelowLevelThreshold_LevelUnchanged()
    {
        var c = MakeChar();
        c.GainXP(50);
        Assert.AreEqual(1, c.Level);
        Assert.AreEqual(50, c.Experience);
    }

    [Test]
    public void GainXP_ReachesLevel2_LevelUpdated()
    {
        var c = MakeChar();
        c.GainXP(100);
        Assert.AreEqual(2, c.Level);
    }

    [Test]
    public void GainXP_LevelUp_GrantsSkillPoint()
    {
        var c = MakeChar();
        c.GainXP(100); // L1 → L2
        Assert.AreEqual(1, c.SkillPoints);
    }

    [Test]
    public void GainXP_MultiLevelUp_GrantsMultipleSkillPoints()
    {
        var c = MakeChar();
        c.GainXP(500); // L1 → L5 = 4 levels gained = 4 points
        Assert.AreEqual(4, c.SkillPoints);
        Assert.AreEqual(5, c.Level);
    }

    [Test]
    public void GainXP_AtMaxLevel_ExperienceDoesNotIncrease()
    {
        var c = MakeChar(30);
        int xpBefore = c.Experience;
        c.GainXP(1000);
        Assert.AreEqual(xpBefore, c.Experience);
    }

    [Test]
    public void GainXP_PublishesLevelUpEvent()
    {
        var c = MakeChar();
        LevelUpEvent? received = null;
        System.Action<LevelUpEvent> handler = e => received = e;
        EventBus.Subscribe<LevelUpEvent>(handler);
        c.GainXP(100);
        EventBus.Unsubscribe<LevelUpEvent>(handler);
        Assert.IsNotNull(received);
        Assert.AreEqual(2, received.Value.NewLevel);
    }

    [Test]
    public void SpendSkillPoint_NoSkillTree_ReturnsFalse()
    {
        var c = MakeChar();
        c.GainXP(100); // get 1 point
        Assert.IsFalse(c.SpendSkillPoint("n1"));
    }

    [Test]
    public void SpendSkillPoint_ValidNode_DecreasesSkillPoints()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100); // 1 point
        c.InitSkillTree(tree);
        Assert.IsTrue(c.SpendSkillPoint("n1"));
        Assert.AreEqual(0, c.SkillPoints);
    }

    [Test]
    public void ResetSkillTree_RefundsPoints()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100); // 1 point
        c.InitSkillTree(tree);
        c.SpendSkillPoint("n1");
        Assert.AreEqual(0, c.SkillPoints);

        bool resetOk = c.ResetSkillTree(gold: 1000, out int cost);
        Assert.IsTrue(resetOk);
        Assert.AreEqual(50, cost); // level 1 × 50
        Assert.AreEqual(1, c.SkillPoints); // refunded
    }

    [Test]
    public void ResetSkillTree_NotEnoughGold_ReturnsFalse()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100);
        c.InitSkillTree(tree);
        c.SpendSkillPoint("n1");

        bool resetOk = c.ResetSkillTree(gold: 0, out int cost);
        Assert.IsFalse(resetOk);
    }
}
