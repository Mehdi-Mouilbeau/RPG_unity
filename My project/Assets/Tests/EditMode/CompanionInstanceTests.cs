using NUnit.Framework;
using UnityEngine;

public class CompanionInstanceTests
{
    private CompanionSkillSO MakeSkill(string name, int cooldown = 3)
    {
        var s = ScriptableObject.CreateInstance<CompanionSkillSO>();
        s.skillName = name;
        s.cooldownTurns = cooldown;
        s.value = 20;
        return s;
    }

    private CompanionInstance MakeInstance(params CompanionSkillSO[] skills)
    {
        var so = ScriptableObject.CreateInstance<CompanionSO>();
        so.skills = skills;
        return new CompanionInstance(so);
    }

    [Test]
    public void CanUse_NewInstance_ReturnsTrue()
    {
        var skill = MakeSkill("Attaque");
        var c = MakeInstance(skill);
        Assert.IsTrue(c.CanUse(skill));
    }

    [Test]
    public void Use_SetsCorrectCooldown()
    {
        var skill = MakeSkill("Attaque", cooldown: 4);
        var c = MakeInstance(skill);
        c.Use(skill);
        Assert.AreEqual(4, c.GetCooldown(skill));
    }

    [Test]
    public void CanUse_AfterUse_ReturnsFalse()
    {
        var skill = MakeSkill("Attaque");
        var c = MakeInstance(skill);
        c.Use(skill);
        Assert.IsFalse(c.CanUse(skill));
    }

    [Test]
    public void TickCooldowns_DecrementsByOne()
    {
        var skill = MakeSkill("Attaque", cooldown: 3);
        var c = MakeInstance(skill);
        c.Use(skill);
        c.TickCooldowns();
        Assert.AreEqual(2, c.GetCooldown(skill));
    }

    [Test]
    public void TickCooldowns_AfterFullExpiry_CanUseAgain()
    {
        var skill = MakeSkill("Attaque", cooldown: 3);
        var c = MakeInstance(skill);
        c.Use(skill);
        c.TickCooldowns();
        c.TickCooldowns();
        c.TickCooldowns();
        Assert.IsTrue(c.CanUse(skill));
    }

    [Test]
    public void CanUse_NullSkill_ReturnsFalse()
    {
        var c = MakeInstance();
        Assert.IsFalse(c.CanUse(null));
    }
}
