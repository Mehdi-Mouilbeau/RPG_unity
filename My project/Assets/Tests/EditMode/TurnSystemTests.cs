using NUnit.Framework;
using System.Collections.Generic;

public class TurnSystemTests
{
    [SetUp]
    public void Setup() => EventBus.Clear();

    private CharacterData MakeChar(string name, int agi)
    {
        var c = new CharacterData();
        c.Initialize(name, 100, 50, 10, 10, 10, 10, agi, 0);
        return c;
    }

    [Test]
    public void Initiative_OrderedByAGIDescending()
    {
        var slow  = MakeChar("Slow",  5);
        var fast  = MakeChar("Fast",  20);
        var mid   = MakeChar("Mid",   12);
        var chars = new List<CharacterData> { slow, fast, mid };
        var ts = new TurnSystem(chars);
        Assert.AreEqual("Fast", ts.CurrentCharacter.CharacterName);
    }

    [Test]
    public void NextTurn_AdvancesToNextCharacter()
    {
        var a = MakeChar("A", 20);
        var b = MakeChar("B", 10);
        var ts = new TurnSystem(new List<CharacterData> { a, b });
        ts.NextTurn();
        Assert.AreEqual("B", ts.CurrentCharacter.CharacterName);
    }

    [Test]
    public void NextTurn_WrapsAroundToFirst()
    {
        var a = MakeChar("A", 20);
        var b = MakeChar("B", 10);
        var ts = new TurnSystem(new List<CharacterData> { a, b });
        ts.NextTurn();
        ts.NextTurn();
        Assert.AreEqual("A", ts.CurrentCharacter.CharacterName);
    }

    [Test]
    public void NextTurn_SkipsDeadCharacters()
    {
        var a = MakeChar("A", 20);
        var b = MakeChar("B", 15);
        var c = MakeChar("C", 10);
        b.TakeDamage(200);
        var ts = new TurnSystem(new List<CharacterData> { a, b, c });
        ts.NextTurn();
        Assert.AreEqual("C", ts.CurrentCharacter.CharacterName);
    }

    [Test]
    public void TurnNumber_IncrementsEachFullRound()
    {
        var a = MakeChar("A", 20);
        var b = MakeChar("B", 10);
        var ts = new TurnSystem(new List<CharacterData> { a, b });
        Assert.AreEqual(1, ts.TurnNumber);
        ts.NextTurn();
        ts.NextTurn();
        Assert.AreEqual(2, ts.TurnNumber);
    }
}
