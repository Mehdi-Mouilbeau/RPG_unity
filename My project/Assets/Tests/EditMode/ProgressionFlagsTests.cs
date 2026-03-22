using NUnit.Framework;

public class ProgressionFlagsTests
{
    [Test]
    public void IsSet_AfterSet_ReturnsTrue()
    {
        var flags = new ProgressionFlags();
        flags.Set("village_visited");
        Assert.IsTrue(flags.IsSet("village_visited"));
    }

    [Test]
    public void IsSet_NeverSet_ReturnsFalse()
    {
        var flags = new ProgressionFlags();
        Assert.IsFalse(flags.IsSet("village_visited"));
    }

    [Test]
    public void Reset_ClearsAllFlags()
    {
        var flags = new ProgressionFlags();
        flags.Set("a");
        flags.Set("b");
        flags.Reset();
        Assert.IsFalse(flags.IsSet("a"));
        Assert.IsFalse(flags.IsSet("b"));
    }

    [Test]
    public void Flags_AreIndependentFromEachOther()
    {
        var flags = new ProgressionFlags();
        flags.Set("flag_a");
        Assert.IsTrue(flags.IsSet("flag_a"));
        Assert.IsFalse(flags.IsSet("flag_b"));
    }

    [Test]
    public void Set_NullKey_DoesNotThrow()
    {
        var flags = new ProgressionFlags();
        Assert.DoesNotThrow(() => flags.Set(null));
        Assert.IsFalse(flags.IsSet(null));
    }

    [Test]
    public void RoundTrip_SaveLoadFlags_RestoresState()
    {
        var flags = new ProgressionFlags();
        flags.Set("boss_defeated");
        flags.Set("chest_1_opened");

        var saved = flags.GetAllAsList();
        var flags2 = new ProgressionFlags();
        flags2.LoadFrom(saved);

        Assert.IsTrue(flags2.IsSet("boss_defeated"));
        Assert.IsTrue(flags2.IsSet("chest_1_opened"));
        Assert.IsFalse(flags2.IsSet("village_visited"));
    }
}
