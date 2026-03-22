using NUnit.Framework;

public class BossPhaseLogicTests
{
    [Test]
    public void Phase2_NotActive_AtStart()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsFalse(logic.Phase2Active);
    }

    [Test]
    public void ShouldTransition_AboveThreshold_ReturnsFalse()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        // 160 HP / 300 max = 53% > 50% → no transition
        Assert.IsFalse(logic.CheckAndTransition(currentHP: 160));
    }

    [Test]
    public void ShouldTransition_AtExactThreshold_ReturnsTrue()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        // 150 HP / 300 max = 50% → transition
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 150));
        Assert.IsTrue(logic.Phase2Active);
    }

    [Test]
    public void ShouldTransition_BelowThreshold_ReturnsTrue()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 100));
    }

    [Test]
    public void ShouldTransition_NotCalledTwice_AfterPhase2Active()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.IsTrue(logic.CheckAndTransition(currentHP: 100));   // first → true
        Assert.IsFalse(logic.CheckAndTransition(currentHP: 50));   // already phase 2 → false
    }

    [Test]
    public void ATKBoostMultiplier_Is1_25()
    {
        var logic = new BossPhaseLogic(maxHP: 300, phaseThreshold: 0.5f);
        Assert.AreEqual(1.25f, logic.Phase2ATKMultiplier, delta: 0.001f);
    }
}
