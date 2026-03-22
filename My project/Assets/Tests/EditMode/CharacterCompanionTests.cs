using NUnit.Framework;
using UnityEngine;

public class CharacterCompanionTests
{
    private CharacterData MakeChar()
    {
        var c = new CharacterData();
        c.Initialize("Héros", 100, 50, 10, 5, 5, 5, 10, 5);
        return c;
    }

    private CompanionSkillSO MakeSkill(CompanionEffectType effect, int value, int cooldown = 3)
    {
        var s = ScriptableObject.CreateInstance<CompanionSkillSO>();
        s.skillName = "TestSkill";
        s.effectType = effect;
        s.value = value;
        s.cooldownTurns = cooldown;
        s.targetType = CompanionTargetType.EnemySingle;
        return s;
    }

    [Test]
    public void AssignCompanion_SetsCompanionInstance()
    {
        var c  = MakeChar();
        var so = ScriptableObject.CreateInstance<CompanionSO>();
        so.companionName = "Loup";
        c.AssignCompanion(so);
        Assert.IsNotNull(c.Companion);
        Assert.AreEqual(so, c.Companion.Definition);
    }

    [Test]
    public void AssignCompanion_Null_ClearsCompanion()
    {
        var c  = MakeChar();
        var so = ScriptableObject.CreateInstance<CompanionSO>();
        c.AssignCompanion(so);
        c.AssignCompanion(null);
        Assert.IsNull(c.Companion);
    }

    [Test]
    public void UseCompanionSkill_NoCompanion_ReturnsFalse()
    {
        var c     = MakeChar();
        var enemy = MakeChar();
        var skill = MakeSkill(CompanionEffectType.DirectDamage, 10);

        var result = c.UseCompanionSkill(skill, enemy, new[] { c }, new[] { enemy });
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void UseCompanionSkill_WithCompanion_ExecutesEffect()
    {
        var c     = MakeChar();
        var enemy = MakeChar();
        var skill = MakeSkill(CompanionEffectType.DirectDamage, 15);
        var so    = ScriptableObject.CreateInstance<CompanionSO>();
        so.skills = new[] { skill };
        c.AssignCompanion(so);

        var result = c.UseCompanionSkill(skill, enemy, new[] { c }, new[] { enemy });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(85, enemy.CurrentHP);
    }

    [Test]
    public void TickCompanionCooldowns_DecreasesCooldown()
    {
        var c     = MakeChar();
        var enemy = MakeChar();
        var skill = MakeSkill(CompanionEffectType.DirectDamage, 10, cooldown: 3);
        var so    = ScriptableObject.CreateInstance<CompanionSO>();
        so.skills = new[] { skill };
        c.AssignCompanion(so);

        c.UseCompanionSkill(skill, enemy, new[] { c }, new[] { enemy });
        Assert.AreEqual(3, c.Companion.GetCooldown(skill));

        c.TickCompanionCooldowns();
        Assert.AreEqual(2, c.Companion.GetCooldown(skill));
    }
}
