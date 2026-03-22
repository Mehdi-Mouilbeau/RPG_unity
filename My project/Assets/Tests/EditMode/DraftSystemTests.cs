using NUnit.Framework;
using System.Collections.Generic;

public class DraftSystemTests
{
    private List<string> Roster(int n)
    {
        var r = new List<string>();
        for (int i = 0; i < n; i++) r.Add($"Char{i}");
        return r;
    }

    [Test]
    public void Ban_RemovesCharacterFromRoster()
    {
        var draft = new DraftSystem(Roster(6));
        draft.Ban(0, "Char0");
        Assert.IsFalse(draft.AvailableRoster.Contains("Char0"));
    }

    [Test]
    public void Ban_UnknownCharacter_ThrowsArgumentException()
    {
        var draft = new DraftSystem(Roster(6));
        Assert.Throws<System.ArgumentException>(() => draft.Ban(0, "Ghost"));
    }

    [Test]
    public void Pick_WrongPlayer_ThrowsInvalidOperationException()
    {
        var draft = new DraftSystem(Roster(8));
        draft.Ban(0, "Char0");
        draft.Ban(1, "Char1");
        // First pick is P1 (snake[0] == 0)
        Assert.Throws<System.InvalidOperationException>(() => draft.Pick(1, "Char2"));
    }

    [Test]
    public void SnakePick_FollowsOrder_0_1_1_0_0_1()
    {
        var draft = new DraftSystem(Roster(8));
        draft.Ban(0, "Char0");
        draft.Ban(1, "Char1");

        int[] expected = { 0, 1, 1, 0, 0, 1 };
        var chars = new[] { "Char2","Char3","Char4","Char5","Char6","Char7" };

        for (int i = 0; i < 6; i++)
        {
            Assert.AreEqual(expected[i], draft.CurrentPickPlayer);
            draft.Pick(expected[i], chars[i]);
        }
    }

    [Test]
    public void Draft_IsComplete_AfterSixPicks()
    {
        var draft = new DraftSystem(Roster(8));
        draft.Ban(0, "Char0");
        draft.Ban(1, "Char1");
        var order = new[] { 0, 1, 1, 0, 0, 1 };
        var chars = new[] { "Char2","Char3","Char4","Char5","Char6","Char7" };
        for (int i = 0; i < 6; i++) draft.Pick(order[i], chars[i]);

        Assert.IsTrue(draft.IsComplete);
        Assert.AreEqual(3, draft.GetTeam(0).Count);
        Assert.AreEqual(3, draft.GetTeam(1).Count);
    }
}
