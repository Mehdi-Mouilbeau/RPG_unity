# SkillTree + Progression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the XP/level-up system and skill tree (common trunk + two specialization branches) per spec §4.4–4.5.

**Architecture:** `XPSystem` (static, pure data) drives level-up logic and publishes `LevelUpEvent`; `SkillTreeState` (runtime class, no MonoBehaviour) manages unlock state per character; `SkillTreeSO` (ScriptableObject) defines tree structure; `CharacterData` owns Experience, SkillPoints, and the runtime state instance.

**Tech Stack:** Unity 6, C#, NUnit EditMode tests, ScriptableObjects, EventBus

---

## File Map

| File | Action | Responsibility |
|------|--------|----------------|
| `My project/Assets/Scripts/Data/Enums.cs` | Modify | Add `SkillBranch` enum |
| `My project/Assets/Scripts/Core/GameEvents.cs` | Modify | Add `LevelUpEvent` struct |
| `My project/Assets/Scripts/Skills/SkillNode.cs` | Create | Serializable node data (id, skill, cost, level gate, branch, prerequisites) |
| `My project/Assets/Scripts/Skills/XPSystem.cs` | Create | Static class: 30-level XP threshold table, level-from-XP logic |
| `My project/Assets/Scripts/Skills/SkillTreeSO.cs` | Create | ScriptableObject: nodes[], specAName, specBName, GetNode(), GetNodesForBranch() |
| `My project/Assets/Scripts/Skills/SkillTreeState.cs` | Create | Runtime state: unlocked set, chosen spec, CanUnlock(), Unlock(), Reset(), GetResetCost() |
| `My project/Assets/Scripts/Characters/CharacterData.cs` | Modify | Add Experience, SkillPoints, SkillTree state; GainXP(), SpendSkillPoint(), ResetSkillTree() |
| `My project/Assets/Scripts/Data/ClassSO.cs` | Modify | Add `SkillTreeSO skillTree` field |
| `My project/Editor/RPGAssetCreator.cs` | Modify | Create starter Guerrier SkillTreeSO asset |
| `My project/Assets/Tests/EditMode/XPSystemTests.cs` | Create | Unit tests for XP/level logic |
| `My project/Assets/Tests/EditMode/SkillTreeStateTests.cs` | Create | Unit tests for unlock/spec/reset logic |
| `My project/Assets/Tests/EditMode/CharacterProgressionTests.cs` | Create | Integration tests for GainXP, SpendSkillPoint, ResetSkillTree on CharacterData |

---

## Task 1: Create feature branch + foundation data types

**Files:**
- Create branch: `feature/plan4-skill-tree`
- Modify: `My project/Assets/Scripts/Data/Enums.cs`
- Modify: `My project/Assets/Scripts/Core/GameEvents.cs`
- Create: `My project/Assets/Scripts/Skills/SkillNode.cs`

- [ ] **Step 1: Create feature branch**

```bash
cd "C:/Users/Misscrazy/Documents/projets/RPG"
git checkout -b feature/plan4-skill-tree
```

- [ ] **Step 2: Add `SkillBranch` enum to `Enums.cs`**

Append to the end of `My project/Assets/Scripts/Data/Enums.cs`:

```csharp
public enum SkillBranch { Common, SpecA, SpecB }
```

- [ ] **Step 3: Add `LevelUpEvent` to `GameEvents.cs`**

Append to `My project/Assets/Scripts/Core/GameEvents.cs`:

```csharp
public struct LevelUpEvent { public CharacterData Character; public int NewLevel; public int SkillPointsGained; }
```

- [ ] **Step 4: Create `SkillNode.cs`**

Create `My project/Assets/Scripts/Skills/SkillNode.cs`:

```csharp
[System.Serializable]
public class SkillNode
{
    public string nodeId;
    public SkillSO skill;
    public int pointCost = 1;
    public int unlockLevel = 1;
    public SkillBranch branch;
    public string[] prerequisiteNodeIds = System.Array.Empty<string>();
}
```

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Data/Enums.cs" \
        "My project/Assets/Scripts/Core/GameEvents.cs" \
        "My project/Assets/Scripts/Skills/SkillNode.cs"
git commit -m "feat: add SkillBranch enum, LevelUpEvent, SkillNode data class"
```

---

## Task 2: XPSystem + tests

**Files:**
- Create: `My project/Assets/Scripts/Skills/XPSystem.cs`
- Create: `My project/Assets/Tests/EditMode/XPSystemTests.cs`

**XP thresholds (spec §4.5, exponential curve):**

| Level | Cumulative XP | Level | Cumulative XP |
|-------|--------------|-------|--------------|
| 1 | 0 | 16 | 8 100 |
| 2 | 100 | 17 | 9 800 |
| 3 | 200 | 18 | 11 700 |
| 4 | 350 | 19 | 13 500 |
| 5 | 500 | 20 | 15 000 |
| 6 | 700 | 21 | 17 500 |
| 7 | 950 | 22 | 20 200 |
| 8 | 1 250 | 23 | 23 100 |
| 9 | 1 600 | 24 | 26 200 |
| 10 | 2 000 | 25 | 29 600 |
| 11 | 2 600 | 26 | 33 400 |
| 12 | 3 300 | 27 | 37 600 |
| 13 | 4 200 | 28 | 42 200 |
| 14 | 5 300 | 29 | 46 000 |
| 15 | 6 600 | 30 | 50 000 |

- [ ] **Step 1: Write failing tests**

Create `My project/Assets/Tests/EditMode/XPSystemTests.cs`:

```csharp
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
        // 150 XP puts us between L2 (100) and L3 (200) → still L2
        Assert.AreEqual(2, XPSystem.GetLevel(150));
        // 999 XP is between L9 (1600... wait, 999 < 950? no — 950 is L7, 1250 is L8
        // 999 is between L7 threshold (950) and L8 threshold (1250) → L7
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
```

- [ ] **Step 2: Run tests to verify they fail**

Open Unity Test Runner (Window > General > Test Runner), run `XPSystemTests` — expect compile errors until `XPSystem` is created.

- [ ] **Step 3: Implement `XPSystem.cs`**

Create `My project/Assets/Scripts/Skills/XPSystem.cs`:

```csharp
public static class XPSystem
{
    public const int MaxLevel = 30;

    // Index = level - 1. CumulativeXPThresholds[0] = 0 (level 1), [29] = 50000 (level 30).
    private static readonly int[] CumulativeXPThresholds =
    {
            0,   // L1
          100,   // L2
          200,   // L3
          350,   // L4
          500,   // L5
          700,   // L6
          950,   // L7
        1_250,   // L8
        1_600,   // L9
        2_000,   // L10
        2_600,   // L11
        3_300,   // L12
        4_200,   // L13
        5_300,   // L14
        6_600,   // L15
        8_100,   // L16
        9_800,   // L17
       11_700,   // L18
       13_500,   // L19
       15_000,   // L20
       17_500,   // L21
       20_200,   // L22
       23_100,   // L23
       26_200,   // L24
       29_600,   // L25
       33_400,   // L26
       37_600,   // L27
       42_200,   // L28
       46_000,   // L29
       50_000,   // L30
    };

    /// <summary>Returns the level corresponding to the given cumulative XP (1–30).</summary>
    public static int GetLevel(int xp)
    {
        int level = 1;
        for (int i = 1; i < CumulativeXPThresholds.Length; i++)
        {
            if (xp >= CumulativeXPThresholds[i])
                level = i + 1;
            else
                break;
        }
        return level;
    }

    /// <summary>Returns cumulative XP required to reach the given level.</summary>
    public static int CumulativeXPForLevel(int level)
    {
        if (level < 1) return 0;
        if (level > MaxLevel) level = MaxLevel;
        return CumulativeXPThresholds[level - 1];
    }

    /// <summary>Returns XP needed to go from currentLevel to currentLevel+1. Returns 0 at max level.</summary>
    public static int XPForNextLevel(int currentLevel)
    {
        if (currentLevel >= MaxLevel) return 0;
        return CumulativeXPThresholds[currentLevel] - CumulativeXPThresholds[currentLevel - 1];
    }

    public static bool IsMaxLevel(int level) => level >= MaxLevel;
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run `XPSystemTests` in Unity Test Runner — expect 8 PASS.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Skills/XPSystem.cs" \
        "My project/Assets/Tests/EditMode/XPSystemTests.cs"
git commit -m "feat: add XPSystem with 30-level XP threshold table"
```

---

## Task 3: SkillTreeSO ScriptableObject

**Files:**
- Create: `My project/Assets/Scripts/Skills/SkillTreeSO.cs`

No unit tests — this is a data container; it will be tested indirectly via SkillTreeState tests.

- [ ] **Step 1: Create `SkillTreeSO.cs`**

Create `My project/Assets/Scripts/Skills/SkillTreeSO.cs`:

```csharp
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewSkillTree", menuName = "RPG/SkillTree")]
public class SkillTreeSO : ScriptableObject
{
    [Header("Identité")]
    public ClassSO characterClass;
    public string specAName = "Spécialisation A";
    public string specBName = "Spécialisation B";

    [Header("Nœuds")]
    public SkillNode[] nodes = System.Array.Empty<SkillNode>();

    public SkillNode GetNode(string nodeId)
    {
        foreach (var node in nodes)
            if (node.nodeId == nodeId) return node;
        return null;
    }

    public SkillNode[] GetNodesForBranch(SkillBranch branch)
        => System.Array.FindAll(nodes, n => n.branch == branch);
}
```

- [ ] **Step 2: Commit**

```bash
git add "My project/Assets/Scripts/Skills/SkillTreeSO.cs"
git commit -m "feat: add SkillTreeSO ScriptableObject"
```

---

## Task 4: SkillTreeState runtime + tests

**Files:**
- Create: `My project/Assets/Scripts/Skills/SkillTreeState.cs`
- Create: `My project/Assets/Tests/EditMode/SkillTreeStateTests.cs`

- [ ] **Step 1: Write failing tests**

Create `My project/Assets/Tests/EditMode/SkillTreeStateTests.cs`:

```csharp
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeStateTests
{
    // ── helpers ───────────────────────────────────────────────────────────

    private static SkillTreeSO BuildTree(params SkillNode[] nodes)
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = nodes;
        return tree;
    }

    private static SkillNode CommonNode(string id, int unlockLevel = 1, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.Common, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    private static SkillNode SpecANode(string id, int unlockLevel = 10, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    private static SkillNode SpecBNode(string id, int unlockLevel = 10, params string[] prereqs)
        => new SkillNode { nodeId = id, branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = unlockLevel, prerequisiteNodeIds = prereqs };

    // ── tests ──────────────────────────────────────────────────────────────

    [Test]
    public void CanUnlock_BasicCommonNode_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        Assert.IsTrue(state.CanUnlock("n1", characterLevel: 1));
    }

    [Test]
    public void Unlock_CommonNode_IsUnlocked()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsTrue(state.IsUnlocked("n1"));
    }

    [Test]
    public void CanUnlock_LevelGateNotMet_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1", unlockLevel: 5));
        var state = new SkillTreeState(tree);
        Assert.IsFalse(state.CanUnlock("n1", characterLevel: 4));
    }

    [Test]
    public void CanUnlock_LevelGateMet_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1", unlockLevel: 5));
        var state = new SkillTreeState(tree);
        Assert.IsTrue(state.CanUnlock("n1", characterLevel: 5));
    }

    [Test]
    public void CanUnlock_PrerequisiteNotUnlocked_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1"), CommonNode("n2", prereqs: "n1"));
        var state = new SkillTreeState(tree);
        Assert.IsFalse(state.CanUnlock("n2", characterLevel: 1));
    }

    [Test]
    public void CanUnlock_PrerequisiteUnlocked_ReturnsTrue()
    {
        var tree = BuildTree(CommonNode("n1"), CommonNode("n2", prereqs: "n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsTrue(state.CanUnlock("n2", characterLevel: 1));
    }

    [Test]
    public void CanUnlock_AlreadyUnlocked_ReturnsFalse()
    {
        var tree = BuildTree(CommonNode("n1"));
        var state = new SkillTreeState(tree);
        state.Unlock("n1", characterLevel: 1);
        Assert.IsFalse(state.CanUnlock("n1", characterLevel: 1));
    }

    [Test]
    public void Unlock_FirstSpecANode_SetsChosenSpec()
    {
        var tree = BuildTree(SpecANode("a1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        Assert.AreEqual(SkillBranch.SpecA, state.ChosenSpec);
    }

    [Test]
    public void CanUnlock_OppositeSpecAfterChoosing_ReturnsFalse()
    {
        var tree = BuildTree(SpecANode("a1"), SpecBNode("b1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        Assert.IsFalse(state.CanUnlock("b1", characterLevel: 10));
    }

    [Test]
    public void GetResetCost_Level15_Returns750()
    {
        var tree = BuildTree();
        var state = new SkillTreeState(tree);
        Assert.AreEqual(750, state.GetResetCost(15));
    }

    [Test]
    public void Reset_ClearsUnlockedNodesAndSpec()
    {
        var tree = BuildTree(SpecANode("a1"));
        var state = new SkillTreeState(tree);
        state.Unlock("a1", characterLevel: 10);
        state.Reset();
        Assert.IsFalse(state.IsUnlocked("a1"));
        Assert.IsNull(state.ChosenSpec);
        Assert.AreEqual(0, state.GetUnlockedCount());
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run `SkillTreeStateTests` in Unity Test Runner — expect compile errors.

- [ ] **Step 3: Implement `SkillTreeState.cs`**

Create `My project/Assets/Scripts/Skills/SkillTreeState.cs`:

```csharp
using System.Collections.Generic;

public class SkillTreeState
{
    private readonly SkillTreeSO _tree;
    private readonly HashSet<string> _unlocked = new HashSet<string>();

    public SkillBranch? ChosenSpec { get; private set; }

    public SkillTreeState(SkillTreeSO tree)
    {
        _tree = tree;
    }

    public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

    public int GetUnlockedCount() => _unlocked.Count;

    public bool CanUnlock(string nodeId, int characterLevel)
    {
        var node = _tree.GetNode(nodeId);
        if (node == null) return false;
        if (_unlocked.Contains(nodeId)) return false;
        if (characterLevel < node.unlockLevel) return false;

        // Branch check: if a spec is already chosen, can't unlock the other branch
        if (node.branch != SkillBranch.Common && ChosenSpec.HasValue && ChosenSpec.Value != node.branch)
            return false;

        // Prerequisite check
        if (node.prerequisiteNodeIds != null)
        {
            foreach (var prereq in node.prerequisiteNodeIds)
                if (!_unlocked.Contains(prereq)) return false;
        }

        return true;
    }

    /// <summary>Unlocks the node. Returns false if not allowed. Automatically sets ChosenSpec on first branch node.</summary>
    public bool Unlock(string nodeId, int characterLevel)
    {
        if (!CanUnlock(nodeId, characterLevel)) return false;

        var node = _tree.GetNode(nodeId);
        if (node.branch != SkillBranch.Common && !ChosenSpec.HasValue)
            ChosenSpec = node.branch;

        _unlocked.Add(nodeId);
        return true;
    }

    /// <summary>Cost in gold to reset the skill tree (level × 50).</summary>
    public int GetResetCost(int characterLevel) => characterLevel * 50;

    /// <summary>Resets all unlocked nodes and clears chosen spec.</summary>
    public void Reset()
    {
        _unlocked.Clear();
        ChosenSpec = null;
    }

    public IReadOnlyCollection<string> GetUnlockedNodeIds() => _unlocked;
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run `SkillTreeStateTests` in Unity Test Runner — expect 11 PASS.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Skills/SkillTreeState.cs" \
        "My project/Assets/Tests/EditMode/SkillTreeStateTests.cs"
git commit -m "feat: add SkillTreeState runtime with unlock/spec/reset logic"
```

---

## Task 5: CharacterData integration + tests

**Files:**
- Modify: `My project/Assets/Scripts/Characters/CharacterData.cs`
- Create: `My project/Assets/Tests/EditMode/CharacterProgressionTests.cs`

- [ ] **Step 1: Write failing tests**

Create `My project/Assets/Tests/EditMode/CharacterProgressionTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class CharacterProgressionTests
{
    private CharacterData MakeChar(int level = 1)
    {
        var c = new CharacterData();
        c.Initialize("Héros", 100, 50, 10, 10, 10, 10, 10, 5);
        // Force level via GainXP: give enough XP for the target level
        if (level > 1)
        {
            int xp = XPSystem.CumulativeXPForLevel(level);
            c.GainXP(xp);
        }
        return c;
    }

    [Test]
    public void GainXP_BelowLevelThreshold_LevelUnchanged()
    {
        var c = MakeChar();
        c.GainXP(50);
        Assert.AreEqual(1, c.Level);
        Assert.AreEqual(50, c.Experience);
    }

    [Test]
    public void GainXP_ReachesLevel2_LevelUpdated()
    {
        var c = MakeChar();
        c.GainXP(100);
        Assert.AreEqual(2, c.Level);
    }

    [Test]
    public void GainXP_LevelUp_GrantsSkillPoint()
    {
        var c = MakeChar();
        c.GainXP(100); // L1 → L2
        Assert.AreEqual(1, c.SkillPoints);
    }

    [Test]
    public void GainXP_MultiLevelUp_GrantsMultipleSkillPoints()
    {
        var c = MakeChar();
        c.GainXP(500); // L1 → L5 = 4 levels gained = 4 points
        Assert.AreEqual(4, c.SkillPoints);
        Assert.AreEqual(5, c.Level);
    }

    [Test]
    public void GainXP_AtMaxLevel_ExperienceDoesNotIncrease()
    {
        var c = MakeChar(30);
        int xpBefore = c.Experience;
        c.GainXP(1000);
        Assert.AreEqual(xpBefore, c.Experience);
    }

    [Test]
    public void GainXP_PublishesLevelUpEvent()
    {
        var c = MakeChar();
        LevelUpEvent? received = null;
        System.Action<LevelUpEvent> handler = e => received = e;
        EventBus.Subscribe<LevelUpEvent>(handler);
        c.GainXP(100);
        EventBus.Unsubscribe<LevelUpEvent>(handler);
        Assert.IsNotNull(received);
        Assert.AreEqual(2, received.Value.NewLevel);
    }

    [Test]
    public void SpendSkillPoint_NoSkillTree_ReturnsFalse()
    {
        var c = MakeChar();
        c.GainXP(100); // get 1 point
        Assert.IsFalse(c.SpendSkillPoint("n1"));
    }

    [Test]
    public void SpendSkillPoint_ValidNode_DecreasesSkillPoints()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100); // 1 point
        c.InitSkillTree(tree);
        Assert.IsTrue(c.SpendSkillPoint("n1"));
        Assert.AreEqual(0, c.SkillPoints);
    }

    [Test]
    public void ResetSkillTree_RefundsPoints()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100); // 1 point
        c.InitSkillTree(tree);
        c.SpendSkillPoint("n1"); // spend it
        Assert.AreEqual(0, c.SkillPoints);

        bool resetOk = c.ResetSkillTree(gold: 1000, out int cost);
        Assert.IsTrue(resetOk);
        Assert.AreEqual(50, cost); // level 1 × 50
        Assert.AreEqual(1, c.SkillPoints); // refunded
    }

    [Test]
    public void ResetSkillTree_NotEnoughGold_ReturnsFalse()
    {
        var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
        tree.nodes = new[] { new SkillNode { nodeId = "n1", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1, prerequisiteNodeIds = new string[0] } };

        var c = MakeChar();
        c.GainXP(100);
        c.InitSkillTree(tree);
        c.SpendSkillPoint("n1");

        bool resetOk = c.ResetSkillTree(gold: 0, out int cost);
        Assert.IsFalse(resetOk);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run `CharacterProgressionTests` — expect compile errors (missing members on `CharacterData`).

- [ ] **Step 3: Modify `CharacterData.cs`**

Add after the `Inventory` property:

```csharp
// ── Progression ───────────────────────────────────────────────────────
public int Experience { get; private set; }
public int SkillPoints { get; private set; }

private SkillTreeState _skillTreeState;
public SkillTreeState SkillTree => _skillTreeState;

public void InitSkillTree(SkillTreeSO tree)
{
    _skillTreeState = new SkillTreeState(tree);
}

public void GainXP(int amount)
{
    if (amount <= 0) return;
    if (XPSystem.IsMaxLevel(Level)) return;

    int oldLevel = Level;
    Experience += amount;
    int newLevel = XPSystem.GetLevel(Experience);
    if (newLevel > oldLevel)
    {
        int gained = newLevel - oldLevel;
        Level = newLevel;
        SkillPoints += gained;
        EventBus.Publish(new LevelUpEvent { Character = this, NewLevel = Level, SkillPointsGained = gained });
    }
}

public bool SpendSkillPoint(string nodeId)
{
    if (SkillPoints <= 0) return false;
    if (_skillTreeState == null) return false;
    if (!_skillTreeState.CanUnlock(nodeId, Level)) return false;

    bool ok = _skillTreeState.Unlock(nodeId, Level);
    if (ok) SkillPoints--;
    return ok;
}

/// <summary>Resets skill tree and refunds points. Returns false if gold is insufficient.</summary>
public bool ResetSkillTree(int gold, out int goldCost)
{
    goldCost = _skillTreeState?.GetResetCost(Level) ?? 0;
    if (_skillTreeState == null) return false;
    if (gold < goldCost) return false;

    int refund = _skillTreeState.GetUnlockedCount();
    _skillTreeState.Reset();
    SkillPoints += refund;
    return true;
}
```

Note: `Level` is already `{ get; private set; }` so change its setter from `private set` to `private set` — no change needed. However, `Level` is currently set only by `InitializeFromSO`. Now `GainXP` also sets `Level`. Since `Level` has a `private set`, this is fine (same class).

- [ ] **Step 4: Run tests to verify they pass**

Run `CharacterProgressionTests` in Unity Test Runner — expect 10 PASS.

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Characters/CharacterData.cs" \
        "My project/Assets/Tests/EditMode/CharacterProgressionTests.cs"
git commit -m "feat: add XP/level-up and skill point system to CharacterData"
```

---

## Task 6: ClassSO update + RPGAssetCreator starter tree + merge

**Files:**
- Modify: `My project/Assets/Scripts/Data/ClassSO.cs`
- Modify: `My project/Editor/RPGAssetCreator.cs`

- [ ] **Step 1: Add `skillTree` field to `ClassSO.cs`**

Add to `ClassSO.cs` after the `startingSkills` field:

```csharp
[Header("Arbre de compétences")]
public SkillTreeSO skillTree;
```

- [ ] **Step 2: Update `InitializeFromSO` in `CharacterData.cs` to wire the skill tree**

In `My project/Assets/Scripts/Characters/CharacterData.cs`, inside `InitializeFromSO`, replace the existing block:

```csharp
        if (classSO.startingSkills != null)
            Skills.AddRange(classSO.startingSkills);
    }
```

with:

```csharp
        if (classSO.startingSkills != null)
            Skills.AddRange(classSO.startingSkills);

        if (classSO.skillTree != null)
            InitSkillTree(classSO.skillTree);
    }
```

- [ ] **Step 3: Modify `RPGAssetCreator.cs` to create a starter Guerrier skill tree**

Read the current `RPGAssetCreator.cs` first, then:

1. In `EnsureFolder` calls inside `CreateFolders()`, add:
   ```csharp
   EnsureFolder("Assets/_Data/SkillTrees");
   ```

2. In `CreateAllAssets()`, add at the end:
   ```csharp
   CreateStarterSkillTrees();
   ```

3. Add the new method:
   ```csharp
   private static void CreateStarterSkillTrees()
   {
       var tree = CreateOrLoad<SkillTreeSO>("Assets/_Data/SkillTrees/GuerrierSkillTree.asset");
       tree.specAName = "Chevalier";
       tree.specBName = "Berserker";
       tree.nodes = new SkillNode[]
       {
           // ── Tronc commun (L1-10) ──────────────────────────────────────────
           new SkillNode { nodeId = "g_coup_puissant",    branch = SkillBranch.Common, pointCost = 1, unlockLevel = 1,  prerequisiteNodeIds = new string[0] },
           new SkillNode { nodeId = "g_cri_de_guerre",    branch = SkillBranch.Common, pointCost = 1, unlockLevel = 2,  prerequisiteNodeIds = new[] { "g_coup_puissant" } },
           new SkillNode { nodeId = "g_posture_de_garde", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 3,  prerequisiteNodeIds = new[] { "g_coup_puissant" } },
           new SkillNode { nodeId = "g_attaque_en_chaine", branch = SkillBranch.Common, pointCost = 1, unlockLevel = 5, prerequisiteNodeIds = new[] { "g_cri_de_guerre" } },
           new SkillNode { nodeId = "g_endurance",        branch = SkillBranch.Common, pointCost = 1, unlockLevel = 7,  prerequisiteNodeIds = new[] { "g_posture_de_garde" } },

           // ── Branche Chevalier (SpecA, L10-30, 8 nœuds) ───────────────────
           new SkillNode { nodeId = "c_bouclier_sacre",    branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 10, prerequisiteNodeIds = new[] { "g_endurance" } },
           new SkillNode { nodeId = "c_mur_de_fer",        branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 11, prerequisiteNodeIds = new[] { "c_bouclier_sacre" } },
           new SkillNode { nodeId = "c_frappe_divine",     branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 12, prerequisiteNodeIds = new[] { "c_bouclier_sacre" } },
           new SkillNode { nodeId = "c_benediction",       branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 14, prerequisiteNodeIds = new[] { "c_frappe_divine" } },
           new SkillNode { nodeId = "c_aura_de_lumiere",   branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 16, prerequisiteNodeIds = new[] { "c_benediction" } },
           new SkillNode { nodeId = "c_armure_sacree",     branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 18, prerequisiteNodeIds = new[] { "c_mur_de_fer" } },
           new SkillNode { nodeId = "c_jugement",          branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 22, prerequisiteNodeIds = new[] { "c_aura_de_lumiere" } },
           new SkillNode { nodeId = "c_paladin_divin",     branch = SkillBranch.SpecA, pointCost = 1, unlockLevel = 27, prerequisiteNodeIds = new[] { "c_jugement", "c_armure_sacree" } },

           // ── Branche Berserker (SpecB, L10-30, 8 nœuds) ───────────────────
           new SkillNode { nodeId = "b_rage",               branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 10, prerequisiteNodeIds = new[] { "g_attaque_en_chaine" } },
           new SkillNode { nodeId = "b_peau_epaisse",       branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 11, prerequisiteNodeIds = new[] { "b_rage" } },
           new SkillNode { nodeId = "b_frenésie",           branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 12, prerequisiteNodeIds = new[] { "b_rage" } },
           new SkillNode { nodeId = "b_cri_primitif",       branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 14, prerequisiteNodeIds = new[] { "b_frenésie" } },
           new SkillNode { nodeId = "b_instinct_de_tueur",  branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 16, prerequisiteNodeIds = new[] { "b_frenésie" } },
           new SkillNode { nodeId = "b_pouls_de_guerre",    branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 18, prerequisiteNodeIds = new[] { "b_peau_epaisse" } },
           new SkillNode { nodeId = "b_carnage",            branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 22, prerequisiteNodeIds = new[] { "b_instinct_de_tueur" } },
           new SkillNode { nodeId = "b_avatar_du_chaos",    branch = SkillBranch.SpecB, pointCost = 1, unlockLevel = 27, prerequisiteNodeIds = new[] { "b_carnage", "b_pouls_de_guerre" } },
       };
       EditorUtility.SetDirty(tree);
       AssetDatabase.SaveAssets();
       Debug.Log("[RPGAssetCreator] GuerrierSkillTree created.");
   }
   ```

- [ ] **Step 4: Run all tests in Unity Test Runner**

Run all EditMode tests — expect all previous tests + new ones to PASS (no regressions).

- [ ] **Step 5: Commit**

```bash
git add "My project/Assets/Scripts/Data/ClassSO.cs" \
        "My project/Assets/Scripts/Characters/CharacterData.cs" \
        "My project/Editor/RPGAssetCreator.cs"
git commit -m "feat: wire SkillTree to ClassSO and add Guerrier starter skill tree asset"
```

- [ ] **Step 6: Merge to master**

```bash
git checkout master
git merge feature/plan4-skill-tree
git branch -d feature/plan4-skill-tree
```

---

## Summary

| Task | New files | Tests |
|------|-----------|-------|
| 1 — Foundation types | SkillNode.cs | — |
| 2 — XPSystem | XPSystem.cs | 8 tests |
| 3 — SkillTreeSO | SkillTreeSO.cs | — |
| 4 — SkillTreeState | SkillTreeState.cs | 11 tests |
| 5 — CharacterData integration | — | 10 tests |
| 6 — ClassSO + RPGAssetCreator | — | — |

**Total new tests: 29**
