using NUnit.Framework;
using System.Collections.Generic;

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
}
