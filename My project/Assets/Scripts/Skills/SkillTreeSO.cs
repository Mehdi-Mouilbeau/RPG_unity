using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillTree", menuName = "RPG/SkillTree")]
public class SkillTreeSO : ScriptableObject
{
    [Header("Identité")]
    public ClassSO characterClass;
    public string specAName = "Spécialisation A";
    public string specBName = "Spécialisation B";

    [Header("Nœuds")]
    public SkillNode[] nodes = System.Array.Empty<SkillNode>();

    public SkillNode GetNode(string nodeId)
    {
        foreach (var node in nodes)
            if (node.nodeId == nodeId) return node;
        return null;
    }

    public SkillNode[] GetNodesForBranch(SkillBranch branch)
        => System.Array.FindAll(nodes, n => n.branch == branch);
}
