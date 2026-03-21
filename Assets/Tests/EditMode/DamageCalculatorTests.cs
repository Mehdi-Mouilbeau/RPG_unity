using NUnit.Framework;

public class DamageCalculatorTests
{
    private CharacterData MakeChar(int atk = 0, int def = 0, int mag = 0, int res = 0,
                                   int lck = 0, ElementType affinity = ElementType.None)
    {
        var c = new CharacterData();
        c.Initialize("Test", 200, 50, atk, def, mag, res, 10, lck);
        c.ElementalAffinity_TestOnly = affinity;
        return c;
    }

    [Test]
    public void Physical_BasicDamage_IsATKx2MinusDEF()
    {
        var attacker = MakeChar(atk: 20);
        var defender = MakeChar(def: 10);
        int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: false, out _);
        Assert.AreEqual(30, dmg);
    }

    [Test]
    public void Physical_HighDEF_DamageMinimumIsOne()
    {
        var attacker = MakeChar(atk: 5);
        var defender = MakeChar(def: 100);
        int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: false, out _);
        Assert.AreEqual(1, dmg);
    }

    [Test]
    public void Magical_BasicDamage_IsMAGx2MinusRES()
    {
        var attacker = MakeChar(mag: 15);
        var defender = MakeChar(res: 5);
        int dmg = DamageCalculator.CalculateMagical(attacker, defender,
            ElementType.Fire, critOverride: false, out _);
        Assert.AreEqual(25, dmg);
    }

    [Test]
    public void ElementalAdvantage_Multiplies1_25()
    {
        var attacker = MakeChar(mag: 20);
        var defender = MakeChar(affinity: ElementType.Nature);
        int dmg = DamageCalculator.CalculateMagical(attacker, defender,
            ElementType.Fire, critOverride: false, out _);
        Assert.AreEqual(50, dmg);
    }

    [Test]
    public void CriticalHit_Multiplies1_5()
    {
        var attacker = MakeChar(atk: 20);
        var defender = MakeChar(def: 10);
        int dmg = DamageCalculator.CalculatePhysical(attacker, defender, critOverride: true, out bool wasCrit);
        Assert.AreEqual(45, dmg);
        Assert.IsTrue(wasCrit); // also verify WasCritical is set
    }

    [Test]
    public void PowerMultiplier_ScalesDamage()
    {
        var attacker = MakeChar(atk: 20);
        var defender = MakeChar(def: 10);
        int dmg = DamageCalculator.CalculatePhysical(attacker, defender,
            critOverride: false, powerMultiplier: 2f, out _);
        Assert.AreEqual(60, dmg);
    }
}
