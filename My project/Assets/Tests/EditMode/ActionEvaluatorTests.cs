using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class ActionEvaluatorTests
{
    private CharacterData MakeChar(string name, int hp = 100, int mp = 50,
        int atk = 20, int mag = 15, int agi = 10)
    {
        var c = new CharacterData();
        c.Initialize(name, hp, mp, atk, 10, mag, 10, agi, 5);
        return c;
    }

    [Test]
    public void Easy_ReturnsAction_WhenEnemiesAlive()
    {
        var actor   = MakeChar("Bot");
        var enemies = new List<CharacterData> { MakeChar("E1"), MakeChar("E2") };

        var action = ActionEvaluator.EvaluateEasy(actor, new List<CharacterData> { actor }, enemies);

        Assert.IsNotNull(action);
        Assert.IsNotNull(action.Target);
        Assert.IsFalse(action.Target.IsDead);
    }

    [Test]
    public void Easy_ReturnsNull_WhenAllEnemiesDead()
    {
        var actor = MakeChar("Bot");
        var dead  = MakeChar("Dead");
        dead.TakeDamage(999);

        var action = ActionEvaluator.EvaluateEasy(actor,
            new List<CharacterData> { actor },
            new List<CharacterData> { dead });

        Assert.IsNull(action);
    }

    [Test]
    public void Easy_NeverTargetsDeadEnemy()
    {
        var actor  = MakeChar("Bot");
        var dead   = MakeChar("Dead");  dead.TakeDamage(999);
        var alive  = MakeChar("Alive");
        var enemies = new List<CharacterData> { dead, alive };

        for (int i = 0; i < 30; i++)
        {
            var action = ActionEvaluator.EvaluateEasy(actor,
                new List<CharacterData> { actor }, enemies);
            Assert.AreEqual(alive, action.Target);
        }
    }

    [Test]
    public void Normal_TargetsLowestHPEnemy()
    {
        var actor   = MakeChar("Bot");
        var weakE   = MakeChar("Weak",   hp: 20);
        var strongE = MakeChar("Strong", hp: 100);
        weakE.TakeDamage(15); // weakE = 5 HP restants

        var enemies = new List<CharacterData> { strongE, weakE };

        for (int i = 0; i < 10; i++)
        {
            var action = ActionEvaluator.EvaluateNormal(actor,
                new List<CharacterData> { actor }, enemies);
            Assert.AreEqual(weakE, action.Target);
        }
    }

    [Test]
    public void Normal_HealsAlly_WhenBelowThirtyPercent()
    {
        var actor = MakeChar("Healer", mp: 100, mag: 20);
        var healSkill = UnityEngine.ScriptableObject.CreateInstance<SkillSO>();
        healSkill.skillName    = "Soin";
        healSkill.damageType   = SkillDamageType.Healing;
        healSkill.targetType   = SkillTargetType.SingleAlly;
        healSkill.mpCost       = 10;
        healSkill.powerMultiplier = 1f;
        actor.Skills.Add(healSkill);

        var dyingAlly = MakeChar("Dying", hp: 100);
        dyingAlly.TakeDamage(75); // 25 HP = 25% < 30%

        var enemy = MakeChar("Enemy");

        var action = ActionEvaluator.EvaluateNormal(actor,
            new List<CharacterData> { actor, dyingAlly },
            new List<CharacterData> { enemy });

        Assert.AreEqual(healSkill, action.Skill);
        Assert.AreEqual(dyingAlly, action.Target);
    }

    [Test]
    public void Normal_ExploitsElementalWeakness()
    {
        var actor = MakeChar("Mage", mp: 100);
        var fireSkill = UnityEngine.ScriptableObject.CreateInstance<SkillSO>();
        fireSkill.skillName    = "BouleDeFeu";
        fireSkill.damageType   = SkillDamageType.Magical;
        fireSkill.element      = ElementType.Fire;
        fireSkill.mpCost       = 10;
        fireSkill.powerMultiplier = 1f;
        actor.Skills.Add(fireSkill);

        var natureEnemy = MakeChar("NatureEnemy");
        natureEnemy.ElementalAffinity_TestOnly = ElementType.Nature; // Fire > Nature

        var action = ActionEvaluator.EvaluateNormal(actor,
            new List<CharacterData> { actor },
            new List<CharacterData> { natureEnemy });

        Assert.AreEqual(fireSkill, action.Skill);
    }
}
