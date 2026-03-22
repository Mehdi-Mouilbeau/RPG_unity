public class BotAction
{
    public CharacterData Target { get; }
    public SkillSO Skill { get; } // null = attaque basique

    public BotAction(CharacterData target, SkillSO skill = null)
    {
        Target = target;
        Skill  = skill;
    }
}
