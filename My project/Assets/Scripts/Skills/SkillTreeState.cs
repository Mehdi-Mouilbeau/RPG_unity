using System.Collections.Generic;

public class SkillTreeState
{
    private readonly SkillTreeSO _tree;
    private readonly HashSet<string> _unlocked = new HashSet<string>();

    public SkillBranch? ChosenSpec { get; private set; }

    public SkillTreeState(SkillTreeSO tree)
    {
        _tree = tree;
    }

    public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

    public int GetUnlockedCount() => _unlocked.Count;

    public bool CanUnlock(string nodeId, int characterLevel)
    {
        var node = _tree.GetNode(nodeId);
        if (node == null) return false;
        if (_unlocked.Contains(nodeId)) return false;
        if (characterLevel < node.unlockLevel) return false;

        // Branch check: if a spec is already chosen, can't unlock the other branch
        if (node.branch != SkillBranch.Common && ChosenSpec.HasValue && ChosenSpec.Value != node.branch)
            return false;

        // Prerequisite check
        if (node.prerequisiteNodeIds != null)
        {
            foreach (var prereq in node.prerequisiteNodeIds)
                if (!_unlocked.Contains(prereq)) return false;
        }

        return true;
    }

    /// <summary>Unlocks the node. Returns false if not allowed. Automatically sets ChosenSpec on first branch node.</summary>
    public bool Unlock(string nodeId, int characterLevel)
    {
        if (!CanUnlock(nodeId, characterLevel)) return false;

        var node = _tree.GetNode(nodeId);
        if (node.branch != SkillBranch.Common && !ChosenSpec.HasValue)
            ChosenSpec = node.branch;

        _unlocked.Add(nodeId);
        return true;
    }

    /// <summary>Cost in gold to reset the skill tree (level × 50).</summary>
    public int GetResetCost(int characterLevel) => characterLevel * 50;

    /// <summary>Resets all unlocked nodes and clears chosen spec.</summary>
    public void Reset()
    {
        _unlocked.Clear();
        ChosenSpec = null;
    }

    public IReadOnlyCollection<string> GetUnlockedNodeIds() => _unlocked;
}
