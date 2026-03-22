using NUnit.Framework;

public class CharacterDataTests
{
    private CharacterData CreateTestCharacter()
    {
        var data = new CharacterData();
        data.Initialize("TestHero", 100, 50, 10, 10, 10, 10, 10, 5);
        return data;
    }

    [Test]
    public void TakeDamage_ReducesHP()
    {
        var c = CreateTestCharacter();
        c.TakeDamage(30);
        Assert.AreEqual(70, c.CurrentHP);
    }

    [Test]
    public void TakeDamage_CannotGoBelowZero()
    {
        var c = CreateTestCharacter();
        c.TakeDamage(200);
        Assert.AreEqual(0, c.CurrentHP);
    }

    [Test]
    public void Heal_IncreasesHP()
    {
        var c = CreateTestCharacter();
        c.TakeDamage(50);
        c.Heal(20);
        Assert.AreEqual(70, c.CurrentHP);
    }

    [Test]
    public void Heal_CannotExceedMaxHP()
    {
        var c = CreateTestCharacter();
        c.Heal(999);
        Assert.AreEqual(100, c.CurrentHP);
    }

    [Test]
    public void IsDead_WhenHPIsZero()
    {
        var c = CreateTestCharacter();
        c.TakeDamage(100);
        Assert.IsTrue(c.IsDead);
    }

    [Test]
    public void IsAlive_WhenHPIsPositive()
    {
        var c = CreateTestCharacter();
        Assert.IsFalse(c.IsDead);
    }
}
