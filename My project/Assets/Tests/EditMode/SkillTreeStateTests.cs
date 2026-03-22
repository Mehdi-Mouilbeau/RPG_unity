using NUnit.Framework;
using UnityEngine;

public class SkillTreeStateTests
{
    // ── helpers ───────────────────────────────────────────────────────────

    private static SkillTreeSO BuildTree(params SkillNode[] nodes)
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = nodes;
        return tree;
    }

    private static SkillNode CommonNode(string id, int unlockLevel = 1, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.Common, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    private static SkillNode SpecANode(string id, int unlockLevel = 10, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    private static SkillNode SpecBNode(string id, int unlockLevel = 10, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    // ── tests ──────────────────────────────────────────────────────────────

    [Test]
    public void CanUnlock_BasicCommonNode_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        Assert.IsTrue(state.CanUnlock("n1", characterLevel: 1));
    }

    [Test]
    public void Unlock_CommonNode_IsUnlocked()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsTrue(state.IsUnlocked("n1"));
    }

    [Test]
    public void CanUnlock_LevelGateNotMet_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1", unlockLevel: 5));
        var state = new SkillTreeState(tree);
        Assert.IsFalse(state.CanUnlock("n1", characterLevel: 4));
    }

    [Test]
    public void CanUnlock_LevelGateMet_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1", unlockLevel: 5));
        var state = new SkillTreeState(tree);
        Assert.IsTrue(state.CanUnlock("n1", characterLevel: 5));
    }

    [Test]
    public void CanUnlock_PrerequisiteNotUnlocked_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1"), CommonNode("n2", prereqs: "n1"));
        var state = new SkillTreeState(tree);
        Assert.IsFalse(state.CanUnlock("n2", characterLevel: 1));
    }

    [Test]
    public void CanUnlock_PrerequisiteUnlocked_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1"), CommonNode("n2", prereqs: "n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsTrue(state.CanUnlock("n2", characterLevel: 1));
    }

    [Test]
    public void CanUnlock_AlreadyUnlocked_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsFalse(state.CanUnlock("n1", characterLevel: 1));
    }

    [Test]
    public void Unlock_FirstSpecANode_SetsChosenSpec()
    {
        var tree = BuildTree(SpecANode("a1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        Assert.AreEqual(SkillBranch.SpecA, state.ChosenSpec);
    }

    [Test]
    public void CanUnlock_OppositeSpecAfterChoosing_ReturnsFalse()
    {
        var tree = BuildTree(SpecANode("a1"), SpecBNode("b1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        Assert.IsFalse(state.CanUnlock("b1", characterLevel: 10));
    }

    [Test]
    public void GetResetCost_Level15_Returns750()
    {
        var tree = BuildTree();
        var state = new SkillTreeState(tree);
        Assert.AreEqual(750, state.GetResetCost(15));
    }

    [Test]
    public void Reset_ClearsUnlockedNodesAndSpec()
    {
        var tree = BuildTree(SpecANode("a1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        state.Reset();
        Assert.IsFalse(state.IsUnlocked("a1"));
        Assert.IsNull(state.ChosenSpec);
        Assert.AreEqual(0, state.GetUnlockedCount());
    }
}
