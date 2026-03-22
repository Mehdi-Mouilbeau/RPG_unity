using NUnit.Framework;
using UnityEngine;

public class CompanionSystemTests
{
    private CharacterData MakeChar(string name = "Héros", int hp = 100)
    {
        var c = new CharacterData();
        c.Initialize(name, hp, 50, 10, 5, 5, 5, 10, 5);
        return c;
    }

    private CompanionSkillSO MakeSkill(string name, CompanionEffectType effect, int value,
        CompanionTargetType target = CompanionTargetType.EnemySingle, int cooldown = 3)
    {
        var s = ScriptableObject.CreateInstance<CompanionSkillSO>();
        s.skillName = name;
        s.effectType = effect;
        s.value = value;
        s.targetType = target;
        s.cooldownTurns = cooldown;
        return s;
    }

    private CompanionInstance MakeInstance(CompanionSkillSO skill)
    {
        var so = ScriptableObject.CreateInstance<CompanionSO>();
        so.skills = new[] { skill };
        return new CompanionInstance(so);
    }

    [Test]
    public void Execute_DirectDamage_DealsDamageToTarget()
    {
        var user  = MakeChar("User");
        var enemy = MakeChar("Enemy", 100);
        var skill = MakeSkill("Morsure", CompanionEffectType.DirectDamage, 30);
        var inst  = MakeInstance(skill);

        var result = CompanionSystem.Execute(inst, skill, user, enemy, new[] { user }, new[] { enemy });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(70, enemy.CurrentHP);
        Assert.AreEqual(30, result.TotalValue);
    }

    [Test]
    public void Execute_DirectDamage_AllEnemies_HitsAll()
    {
        var user = MakeChar("User");
        var e1   = MakeChar("E1", 100);
        var e2   = MakeChar("E2", 100);
        var skill = MakeSkill("Souffle", CompanionEffectType.DirectDamage, 20, CompanionTargetType.AllEnemies);
        var inst  = MakeInstance(skill);

        var result = CompanionSystem.Execute(inst, skill, user, e1, new[] { user }, new[] { e1, e2 });

        Assert.IsTrue(result.Success);
        Assert.AreEqual(80, e1.CurrentHP);
        Assert.AreEqual(80, e2.CurrentHP);
        Assert.AreEqual(40, result.TotalValue);
    }

    [Test]
    public void Execute_Heal_RestoresHP()
    {
        var user  = MakeChar("User");
        var ally  = MakeChar("Ally", 100);
        ally.TakeDamage(40);
        var skill = MakeSkill("Soin", CompanionEffectType.Heal, 25, CompanionTargetType.AllySingle);
        var inst  = MakeInstance(skill);

        var result = CompanionSystem.Execute(inst, skill, user, ally, new[] { ally }, new CharacterData[0]);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(85, ally.CurrentHP);
    }

    [Test]
    public void Execute_RemoveStatuses_ClearsNegativeStatuses()
    {
        var user = MakeChar("User");
        var ally = MakeChar("Ally");
        ally.ActiveStatuses.Add(new StatusEffect(StatusEffectType.Poison, 3, 5f));
        ally.ActiveStatuses.Add(new StatusEffect(StatusEffectType.Burn,   2, 3f));
        var skill = MakeSkill("Purification", CompanionEffectType.RemoveStatuses, 0, CompanionTargetType.AllySingle);
        var inst  = MakeInstance(skill);

        var result = CompanionSystem.Execute(inst, skill, user, ally, new[] { ally }, new CharacterData[0]);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, ally.ActiveStatuses.Count);
    }

    [Test]
    public void Execute_SkillOnCooldown_ReturnsFalse()
    {
        var user  = MakeChar("User");
        var enemy = MakeChar("Enemy");
        var skill = MakeSkill("Morsure", CompanionEffectType.DirectDamage, 20);
        var inst  = MakeInstance(skill);

        CompanionSystem.Execute(inst, skill, user, enemy, new[] { user }, new[] { enemy });
        var result2 = CompanionSystem.Execute(inst, skill, user, enemy, new[] { user }, new[] { enemy });

        Assert.IsFalse(result2.Success);
    }

    [Test]
    public void Execute_NullCompanion_ReturnsFalse()
    {
        var user  = MakeChar("User");
        var enemy = MakeChar("Enemy");
        var skill = MakeSkill("Morsure", CompanionEffectType.DirectDamage, 20);

        var result = CompanionSystem.Execute(null, skill, user, enemy, new[] { user }, new[] { enemy });

        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Execute_PublishesCompanionActivatedEvent()
    {
        var user  = MakeChar("User");
        var enemy = MakeChar("Enemy");
        var skill = MakeSkill("Morsure", CompanionEffectType.DirectDamage, 10);
        var inst  = MakeInstance(skill);

        CompanionActivatedEvent? received = null;
        System.Action<CompanionActivatedEvent> handler = e => received = e;
        EventBus.Subscribe<CompanionActivatedEvent>(handler);
        CompanionSystem.Execute(inst, skill, user, enemy, new[] { user }, new[] { enemy });
        EventBus.Unsubscribe<CompanionActivatedEvent>(handler);

        Assert.IsNotNull(received);
        Assert.AreEqual(user, received.Value.Owner);
        Assert.AreEqual(skill, received.Value.Skill);
    }
}
