[System.Serializable]
public class SkillNode
{
    public string nodeId;
    public SkillSO skill;
    public int pointCost = 1;
    public int unlockLevel = 1;
    public SkillBranch branch;
    public string[] prerequisiteNodeIds = System.Array.Empty<string>();
}
