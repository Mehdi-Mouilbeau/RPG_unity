using NUnit.Framework;

public class StatusManagerTests
{
    [SetUp]
    public void Setup() => EventBus.Clear();

    private CharacterData MakeChar()
    {
        var c = new CharacterData();
        c.Initialize("Test", 200, 50, 10, 10, 10, 10, 10, 5);
        return c;
    }

    [Test]
    public void ApplyBurn_AddsStatusToCharacter()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
        Assert.IsTrue(c.HasStatus(StatusEffectType.Burn));
    }

    [Test]
    public void TickBurn_DealsDamageEachTurn()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
        StatusManager.Tick(c);
        Assert.Less(c.CurrentHP, 200);
    }

    [Test]
    public void TickStatus_DecrementsRemainingTurns()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 3));
        StatusManager.Tick(c);
        var status = c.ActiveStatuses.Find(s => s.type == StatusEffectType.Burn);
        Assert.AreEqual(2, status.remainingTurns);
    }

    [Test]
    public void TickStatus_RemovesWhenTurnsReachZero()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Burn, 1));
        StatusManager.Tick(c);
        Assert.IsFalse(c.HasStatus(StatusEffectType.Burn));
    }

    [Test]
    public void Apply_IgnoresImmune_WhenRaceIsImmune()
    {
        var c = MakeChar();
        c.SetImmunity_TestOnly(StatusEffectType.Poison);
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Poison, 3));
        Assert.IsFalse(c.HasStatus(StatusEffectType.Poison));
    }

    [Test]
    public void Shield_AbsorbsDamage()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Shield, 999, 50f));
        int remaining = StatusManager.AbsorbWithShield(c, 30);
        Assert.AreEqual(0, remaining);
        Assert.IsTrue(c.HasStatus(StatusEffectType.Shield));
    }

    [Test]
    public void Shield_RemovedWhenFullyDepleted()
    {
        var c = MakeChar();
        StatusManager.Apply(c, new StatusEffect(StatusEffectType.Shield, 999, 20f));
        int remaining = StatusManager.AbsorbWithShield(c, 50);
        Assert.AreEqual(30, remaining);
        Assert.IsFalse(c.HasStatus(StatusEffectType.Shield));
    }
}
