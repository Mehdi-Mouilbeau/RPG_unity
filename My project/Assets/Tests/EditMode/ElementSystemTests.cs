using NUnit.Framework;

public class ElementSystemTests
{
    [Test]
    public void Fire_VsNature_ReturnsAdvantage()
        => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Nature));

    [Test]
    public void Fire_VsWater_ReturnsDisadvantage()
        => Assert.AreEqual(0.75f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Water));

    [Test]
    public void Fire_VsFire_ReturnsNeutral()
        => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Fire));

    [Test]
    public void Light_VsDark_ReturnsAdvantage()
        => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Light, ElementType.Dark));

    [Test]
    public void Dark_VsLight_ReturnsAdvantage()
        => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Dark, ElementType.Light));

    [Test]
    public void Fire_VsLight_ReturnsNeutral()
        => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.Fire, ElementType.Light));

    [Test]
    public void None_VsAnything_ReturnsNeutral()
        => Assert.AreEqual(1.0f, ElementSystem.GetModifier(ElementType.None, ElementType.Fire));

    [Test]
    public void Water_VsFire_ReturnsAdvantage()
        => Assert.AreEqual(1.25f, ElementSystem.GetModifier(ElementType.Water, ElementType.Fire));

    [Test]
    public void Nature_VsFire_ReturnsDisadvantage()
        => Assert.AreEqual(0.75f, ElementSystem.GetModifier(ElementType.Nature, ElementType.Fire));
}
