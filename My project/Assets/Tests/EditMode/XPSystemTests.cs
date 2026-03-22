using NUnit.Framework;

public class XPSystemTests
{
    [Test]
    public void GetLevel_ZeroXP_ReturnsLevel1()
    {
        Assert.AreEqual(1, XPSystem.GetLevel(0));
    }

    [Test]
    public void GetLevel_ExactThreshold_ReturnsCorrectLevel()
    {
        Assert.AreEqual(2,  XPSystem.GetLevel(100));
        Assert.AreEqual(5,  XPSystem.GetLevel(500));
        Assert.AreEqual(10, XPSystem.GetLevel(2000));
        Assert.AreEqual(20, XPSystem.GetLevel(15000));
        Assert.AreEqual(30, XPSystem.GetLevel(50000));
    }

    [Test]
    public void GetLevel_BetweenThresholds_ReturnsLowerLevel()
    {
        // 150 XP is between L2 (100) and L3 (200) → L2
        Assert.AreEqual(2, XPSystem.GetLevel(150));
        // 999 XP is between L7 (950) and L8 (1250) → L7
        Assert.AreEqual(7, XPSystem.GetLevel(999));
    }

    [Test]
    public void GetLevel_AboveMaxXP_ReturnsMaxLevel()
    {
        Assert.AreEqual(30, XPSystem.GetLevel(100000));
    }

    [Test]
    public void IsMaxLevel_Level30_ReturnsTrue()
    {
        Assert.IsTrue(XPSystem.IsMaxLevel(30));
    }

    [Test]
    public void IsMaxLevel_Level29_ReturnsFalse()
    {
        Assert.IsFalse(XPSystem.IsMaxLevel(29));
    }

    [Test]
    public void CumulativeXPForLevel_ReturnsCorrectValues()
    {
        Assert.AreEqual(0,     XPSystem.CumulativeXPForLevel(1));
        Assert.AreEqual(100,   XPSystem.CumulativeXPForLevel(2));
        Assert.AreEqual(2000,  XPSystem.CumulativeXPForLevel(10));
        Assert.AreEqual(50000, XPSystem.CumulativeXPForLevel(30));
    }

    [Test]
    public void XPForNextLevel_Level1_Returns100()
    {
        Assert.AreEqual(100, XPSystem.XPForNextLevel(1));
    }

    [Test]
    public void XPForNextLevel_MaxLevel_Returns0()
    {
        Assert.AreEqual(0, XPSystem.XPForNextLevel(30));
    }
}
