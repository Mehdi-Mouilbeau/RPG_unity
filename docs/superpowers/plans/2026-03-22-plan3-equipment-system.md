# Plan 3 — Equipment System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a complete equipment system — EquipmentSO, inventory (equip/unequip), rarity multipliers, crafting (forge), and enchanting — integrated with CharacterData stat calculations.

**Architecture:** Pure data via ScriptableObjects (`EquipmentSO`), a pure C# `Inventory` class per `CharacterData`, and static service classes (`RaritySystem`, `ForgeSystem`, `EnchantSystem`). Character stats become computed properties (base + equipment bonus) so all formulas automatically benefit from gear. Events flow through the existing `EventBus`.

**Tech Stack:** Unity 6, C#, ScriptableObjects, NUnit EditMode tests (Unity Test Runner), existing EventBus + CharacterData architecture.

---

## Context — Codebase you will be modifying

**Unity project path:** `C:/Users/Misscrazy/Documents/projets/RPG/My project/`

**Existing files you MUST understand before touching:**

- `Assets/Scripts/Data/Enums.cs` — all enums (add new ones here)
- `Assets/Scripts/Core/GameEvents.cs` — all event structs (add `ItemEquippedEvent`)
- `Assets/Scripts/Characters/CharacterData.cs` — character runtime state; stats are currently auto-properties with private setters — we will refactor them into computed properties (base + equipment)
- `Assets/Scripts/Combat/DamageCalculator.cs`, `ActionResolver.cs` — read these to confirm they use `character.ATK`, `character.DEF` etc. directly — after the refactor, those calls will automatically include equipment bonuses
- `Assets/Editor/RPGAssetCreator.cs` — editor utility; add equipment creation at the end
- `Assets/Tests/EditMode/RPG.Tests.EditMode.asmdef` — test assembly

**Assembly definition:** `Assets/Scripts/RPG.Runtime.asmdef` covers all runtime scripts.

**Tests run via:** Unity Test Runner (`Window > General > Test Runner > EditMode`), NOT from CLI.

---

## File Map

```
Create (new files):
  Assets/Scripts/Equipment/EquipmentEffect.cs       — [Serializable] data class for one item effect
  Assets/Scripts/Equipment/EquipmentSO.cs            — ScriptableObject: slot, stats, rarity, effects
  Assets/Scripts/Equipment/RaritySystem.cs           — static: multipliers, colors, effect slots
  Assets/Scripts/Equipment/Inventory.cs              — pure C#: equip/unequip, bag, stat totals
  Assets/Scripts/Equipment/ForgeSystem.cs            — static: craft (upgrade rarity tier)
  Assets/Scripts/Equipment/EnchantSystem.cs          — static: enchant (add effect to Rare+)
  Assets/Tests/EditMode/RaritySystemTests.cs         — unit tests for RaritySystem
  Assets/Tests/EditMode/InventoryTests.cs            — unit tests for Inventory + CharacterData stat integration
  Assets/Tests/EditMode/ForgeSystemTests.cs          — unit tests for ForgeSystem
  Assets/Tests/EditMode/EnchantSystemTests.cs        — unit tests for EnchantSystem

Modify (existing files):
  Assets/Scripts/Data/Enums.cs                       — add EquipmentRarity, EquipmentEffectType enums
  Assets/Scripts/Core/GameEvents.cs                  — add ItemEquippedEvent struct
  Assets/Scripts/Characters/CharacterData.cs         — refactor stats to computed; add Inventory property
  Assets/Editor/RPGAssetCreator.cs                   — add CreateEquipmentFolder() + CreateStarterEquipment()
```

---

## Task 1 — Equipment Data Types

Add two enums to `Enums.cs` and create the `EquipmentEffect` data class.

**Files:**
- Modify: `Assets/Scripts/Data/Enums.cs`
- Create: `Assets/Scripts/Equipment/EquipmentEffect.cs`

- [ ] **Step 1.1: Add enums to Enums.cs**

Append these two enums at the end of `Assets/Scripts/Data/Enums.cs`:

```csharp
public enum EquipmentRarity { Common, Uncommon, Rare, Epic, Legendary }

public enum EquipmentEffectType
{
    None,
    StatBoost,       // flat stat increase (stored in value)
    DamageOnHit,     // deal extra damage on physical hit
    HealOnKill,      // restore % HP on kill (value = 0–1 fraction)
    ElementalResist, // reduce elemental damage of given element (value = 0–1 fraction)
    MpRegenPerTurn,  // restore flat MP each turn
    CritBoost,       // increase crit chance (value = 0–1 fraction added to base)
}
```

- [ ] **Step 1.2: Create EquipmentEffect.cs**

Create `Assets/Scripts/Equipment/EquipmentEffect.cs`:

```csharp
/// <summary>
/// A single special effect on an equipment piece.
/// Used both for rarity-granted effects and the enchantment slot.
/// Data-only — combat integration reads these in ActionResolver (future plan).
/// </summary>
[System.Serializable]
public class EquipmentEffect
{
    public string          effectId;    // unique identifier, e.g. "crit_boost_5"
    public string          displayName; // shown in UI, e.g. "Coup critique +5%"
    public EquipmentEffectType effectType;
    public ElementType     element;     // used when effectType == ElementalResist
    public float           value;       // meaning depends on effectType
}
```

- [ ] **Step 1.3: Commit**

```bash
git add "My project/Assets/Scripts/Data/Enums.cs" "My project/Assets/Scripts/Equipment/EquipmentEffect.cs"
git commit -m "feat: add EquipmentRarity + EquipmentEffectType enums and EquipmentEffect data class"
```

---

## Task 2 — EquipmentSO ScriptableObject

**Files:**
- Create: `Assets/Scripts/Equipment/EquipmentSO.cs`

- [ ] **Step 2.1: Create EquipmentSO.cs**

Create `Assets/Scripts/Equipment/EquipmentSO.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Data asset for one equipment piece.
/// Base stats are defined in the Inspector; EffectiveXxx properties apply the rarity multiplier.
/// </summary>
[CreateAssetMenu(menuName = "RPG/Equipment/Item", fileName = "New Equipment")]
public class EquipmentSO : ScriptableObject
{
    [Header("Identity")]
    public string         itemName;
    [TextArea] public string description;
    public EquipmentSlot  slot;
    public EquipmentRarity rarity;

    [Header("Base Stats (before rarity multiplier)")]
    public int hpBonus;
    public int mpBonus;
    public int atkBonus;
    public int defBonus;
    public int magBonus;
    public int resBonus;
    public int agiBonus;
    public int lckBonus;

    [Header("Effects")]
    /// <summary>Effects granted by rarity (1 for Rare, 2 for Epic/Legendary).</summary>
    public EquipmentEffect[] rarityEffects;
    /// <summary>Enchantment slot — null means no enchantment.</summary>
    public EquipmentEffect enchantmentEffect;

    [Header("Crafting / Forge")]
    /// <summary>Material names required to upgrade this item's rarity.</summary>
    public string[] craftingMaterials;
    public int      craftingGoldCost = 100;

    [Header("Effect Pool (for Forge & Enchant)")]
    /// <summary>
    /// IDs of possible effects that can be granted on upgrade or enchantment.
    /// The Forge/EnchantSystem draws randomly from this pool.
    /// </summary>
    public string[] effectPool;

    // ── Effective stat helpers (apply rarity multiplier) ──────────────────

    public int EffectiveHP  => ApplyRarity(hpBonus);
    public int EffectiveMP  => ApplyRarity(mpBonus);
    public int EffectiveATK => ApplyRarity(atkBonus);
    public int EffectiveDEF => ApplyRarity(defBonus);
    public int EffectiveMAG => ApplyRarity(magBonus);
    public int EffectiveRES => ApplyRarity(resBonus);
    public int EffectiveAGI => ApplyRarity(agiBonus);
    public int EffectiveLCK => ApplyRarity(lckBonus);

    private int ApplyRarity(int baseValue) =>
        Mathf.RoundToInt(baseValue * RaritySystem.GetStatMultiplier(rarity));
}
```

- [ ] **Step 2.2: Commit**

```bash
git add "My project/Assets/Scripts/Equipment/EquipmentSO.cs"
git commit -m "feat: add EquipmentSO ScriptableObject"
```

---

## Task 3 — RaritySystem + Tests

Pure static class. No Unity dependencies for logic — easily unit-tested.

**Files:**
- Create: `Assets/Scripts/Equipment/RaritySystem.cs`
- Create: `Assets/Tests/EditMode/RaritySystemTests.cs`

- [ ] **Step 3.1: Write failing tests first**

Create `Assets/Tests/EditMode/RaritySystemTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class RaritySystemTests
{
    [Test]
    public void GetStatMultiplier_Common_Returns1()
    {
        Assert.AreEqual(1.00f, RaritySystem.GetStatMultiplier(EquipmentRarity.Common), 0.001f);
    }

    [Test]
    public void GetStatMultiplier_Legendary_Returns1Point5()
    {
        Assert.AreEqual(1.50f, RaritySystem.GetStatMultiplier(EquipmentRarity.Legendary), 0.001f);
    }

    [Test]
    public void GetEffectSlots_Common_Returns0()
    {
        Assert.AreEqual(0, RaritySystem.GetEffectSlots(EquipmentRarity.Common));
    }

    [Test]
    public void GetEffectSlots_Rare_Returns1()
    {
        Assert.AreEqual(1, RaritySystem.GetEffectSlots(EquipmentRarity.Rare));
    }

    [Test]
    public void GetEffectSlots_Epic_Returns2()
    {
        Assert.AreEqual(2, RaritySystem.GetEffectSlots(EquipmentRarity.Epic));
    }

    [Test]
    public void Upgrade_Common_ReturnsUncommon()
    {
        Assert.AreEqual(EquipmentRarity.Uncommon, RaritySystem.Upgrade(EquipmentRarity.Common));
    }

    [Test]
    public void Upgrade_Legendary_StaysLegendary()
    {
        Assert.AreEqual(EquipmentRarity.Legendary, RaritySystem.Upgrade(EquipmentRarity.Legendary));
    }

    [Test]
    public void CanUpgrade_Legendary_ReturnsFalse()
    {
        Assert.IsFalse(RaritySystem.CanUpgrade(EquipmentRarity.Legendary));
    }

    [Test]
    public void CanEnchant_Common_ReturnsFalse()
    {
        Assert.IsFalse(RaritySystem.CanEnchant(EquipmentRarity.Common));
    }

    [Test]
    public void CanEnchant_Rare_ReturnsTrue()
    {
        Assert.IsTrue(RaritySystem.CanEnchant(EquipmentRarity.Rare));
    }

    [Test]
    public void GetRarityColor_Legendary_IsGold()
    {
        var color = RaritySystem.GetRarityColor(EquipmentRarity.Legendary);
        Assert.Greater(color.r, 0.9f);   // gold = R high
        Assert.Greater(color.g, 0.6f);   // gold = G medium-high
        Assert.Less(color.b, 0.2f);      // gold = B low
    }
}
```

- [ ] **Step 3.2: Verify test fails** (run via Unity Test Runner — all tests fail with "type not found")

- [ ] **Step 3.3: Implement RaritySystem.cs**

Create `Assets/Scripts/Equipment/RaritySystem.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Pure data service for equipment rarity rules.
/// Stat multipliers, effect slot counts, rarity colors, upgrade ladder.
/// </summary>
public static class RaritySystem
{
    /// <summary>Multiplier applied to all base stats of the equipment piece.</summary>
    public static float GetStatMultiplier(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common    => 1.00f,
        EquipmentRarity.Uncommon  => 1.10f,
        EquipmentRarity.Rare      => 1.20f,
        EquipmentRarity.Epic      => 1.35f,
        EquipmentRarity.Legendary => 1.50f,
        _                         => 1.00f,
    };

    /// <summary>How many rarity-granted effect slots this rarity provides.</summary>
    public static int GetEffectSlots(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Rare      => 1,
        EquipmentRarity.Epic      => 2,
        EquipmentRarity.Legendary => 2,
        _                         => 0,
    };

    /// <summary>UI color for this rarity tier.</summary>
    public static Color GetRarityColor(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common    => new Color(0.70f, 0.70f, 0.70f), // grey
        EquipmentRarity.Uncommon  => new Color(0.20f, 0.80f, 0.20f), // green
        EquipmentRarity.Rare      => new Color(0.20f, 0.40f, 1.00f), // blue
        EquipmentRarity.Epic      => new Color(0.60f, 0.10f, 0.90f), // purple
        EquipmentRarity.Legendary => new Color(1.00f, 0.75f, 0.00f), // gold
        _                         => Color.white,
    };

    /// <summary>Returns the next rarity tier. Returns the same tier if already Legendary.</summary>
    public static EquipmentRarity Upgrade(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common   => EquipmentRarity.Uncommon,
        EquipmentRarity.Uncommon => EquipmentRarity.Rare,
        EquipmentRarity.Rare     => EquipmentRarity.Epic,
        EquipmentRarity.Epic     => EquipmentRarity.Legendary,
        _                        => rarity,
    };

    /// <summary>Whether this item can still be upgraded by the Forge.</summary>
    public static bool CanUpgrade(EquipmentRarity rarity) =>
        rarity != EquipmentRarity.Legendary;

    /// <summary>Whether this item is eligible for enchantment (Rare or better).</summary>
    public static bool CanEnchant(EquipmentRarity rarity) =>
        rarity >= EquipmentRarity.Rare;
}
```

- [ ] **Step 3.4: Run tests — all 11 must pass** (Unity Test Runner > EditMode > RaritySystemTests)

- [ ] **Step 3.5: Commit**

```bash
git add "My project/Assets/Scripts/Equipment/RaritySystem.cs" "My project/Assets/Tests/EditMode/RaritySystemTests.cs"
git commit -m "feat: add RaritySystem with stat multipliers, upgrade ladder, enchant eligibility"
```

---

## Task 4 — Inventory + ItemEquippedEvent + Tests

Inventory is a pure C# class owned by each `CharacterData`. It manages equipped items (one per slot) and a bag of unequipped items.

**Files:**
- Modify: `Assets/Scripts/Core/GameEvents.cs`
- Create: `Assets/Scripts/Equipment/Inventory.cs`
- Create: `Assets/Tests/EditMode/InventoryTests.cs`

- [ ] **Step 4.1: Add ItemEquippedEvent to GameEvents.cs**

Add this line at the end of `Assets/Scripts/Core/GameEvents.cs`:

```csharp
public struct ItemEquippedEvent  { public CharacterData Owner; public EquipmentSO Item; }
```

The file should now look like:

```csharp
public struct TurnStartedEvent    { public CharacterData Character; }
public struct TurnEndedEvent      { public CharacterData Character; }
public struct ActionResolvedEvent { public ActionResult Result; }
public struct CharacterDiedEvent  { public CharacterData Character; }
public struct BattleEndedEvent    { public bool PlayerWon; }
public struct StatusAppliedEvent  { public CharacterData Target; public StatusEffect Status; }
public struct PlayerTurnEvent     { public int PlayerIndex; public CharacterData Character; }
public struct ItemEquippedEvent   { public CharacterData Owner; public EquipmentSO Item; }
```

- [ ] **Step 4.2: Write failing Inventory tests**

Create `Assets/Tests/EditMode/InventoryTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class InventoryTests
{
    // Helper: create a CharacterData with known stats
    private CharacterData MakeCharacter(string name = "Hero",
        int hp = 100, int mp = 50, int atk = 10, int def = 5,
        int mag = 8, int res = 4, int agi = 6, int lck = 3)
    {
        var c = new CharacterData();
        c.Initialize(name, hp, mp, atk, def, mag, res, agi, lck);
        return c;
    }

    // Helper: create an EquipmentSO in memory (no asset file needed)
    private EquipmentSO MakeItem(EquipmentSlot slot, int atkBonus = 0, int defBonus = 0,
        int hpBonus = 0, EquipmentRarity rarity = EquipmentRarity.Common)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.slot      = slot;
        item.atkBonus  = atkBonus;
        item.defBonus  = defBonus;
        item.hpBonus   = hpBonus;
        item.rarity    = rarity;
        return item;
    }

    [Test]
    public void Equip_PutsItemInEquippedSlot()
    {
        var c    = MakeCharacter();
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);

        c.Inventory.Equip(sword);

        Assert.AreEqual(sword, c.Inventory.Equipped[EquipmentSlot.MainWeapon]);
    }

    [Test]
    public void Equip_SameSlotTwice_MovesFirstToBag()
    {
        var c      = MakeCharacter();
        var sword1 = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        var sword2 = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 8);

        c.Inventory.Equip(sword1);
        c.Inventory.Equip(sword2);

        Assert.AreEqual(sword2, c.Inventory.Equipped[EquipmentSlot.MainWeapon]);
        Assert.Contains(sword1, c.Inventory.Bag);
    }

    [Test]
    public void Unequip_MovesItemToBag()
    {
        var c    = MakeCharacter();
        var helm = MakeItem(EquipmentSlot.Helmet, defBonus: 3);

        c.Inventory.Equip(helm);
        c.Inventory.Unequip(EquipmentSlot.Helmet);

        Assert.IsFalse(c.Inventory.Equipped.ContainsKey(EquipmentSlot.Helmet));
        Assert.Contains(helm, c.Inventory.Bag);
    }

    [Test]
    public void Equip_ItemAlreadyInBag_RemovedFromBag()
    {
        var c    = MakeCharacter();
        var ring = MakeItem(EquipmentSlot.Ring1);
        c.Inventory.Bag.Add(ring);   // manually add to bag first

        c.Inventory.Equip(ring);

        Assert.IsFalse(c.Inventory.Bag.Contains(ring));
        Assert.AreEqual(ring, c.Inventory.Equipped[EquipmentSlot.Ring1]);
    }

    [Test]
    public void GetTotalATK_SumsAllEquippedItems()
    {
        var c     = MakeCharacter();
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        var ring  = MakeItem(EquipmentSlot.Ring1,      atkBonus: 2);

        c.Inventory.Equip(sword);
        c.Inventory.Equip(ring);

        // Common rarity multiplier = 1.0, so effective = base
        Assert.AreEqual(7, c.Inventory.GetTotalATK());
    }

    [Test]
    public void GetTotalATK_UncommonItem_AppliesMultiplier()
    {
        var c    = MakeCharacter();
        var item = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 10,
            rarity: EquipmentRarity.Uncommon); // ×1.1 → 11

        c.Inventory.Equip(item);

        Assert.AreEqual(11, c.Inventory.GetTotalATK());
    }

    [Test]
    public void GetTotalHP_EmptyInventory_ReturnsZero()
    {
        var c = MakeCharacter();
        Assert.AreEqual(0, c.Inventory.GetTotalHP());
    }
}
```

- [ ] **Step 4.3: Verify tests fail** (run via Unity Test Runner — fail because Inventory doesn't exist yet)

- [ ] **Step 4.4: Implement Inventory.cs**

Create `Assets/Scripts/Equipment/Inventory.cs`:

```csharp
using System;
using System.Collections.Generic;

/// <summary>
/// Runtime inventory for one character.
/// Tracks equipped items (one per slot) and bag (unequipped items).
/// Publishes ItemEquippedEvent via EventBus when equipment changes.
/// </summary>
public class Inventory
{
    /// <summary>Currently equipped items, keyed by slot.</summary>
    public Dictionary<EquipmentSlot, EquipmentSO> Equipped { get; } = new();

    /// <summary>Unequipped items stored in the bag.</summary>
    public List<EquipmentSO> Bag { get; } = new();

    private readonly CharacterData _owner;

    public Inventory(CharacterData owner) => _owner = owner;

    /// <summary>
    /// Equip an item. If the slot is already occupied, the old item moves to the Bag.
    /// If the item was already in the Bag, it is removed from there first.
    /// Publishes ItemEquippedEvent.
    /// </summary>
    public void Equip(EquipmentSO item)
    {
        if (item == null) return;

        // Move previous occupant to Bag
        if (Equipped.TryGetValue(item.slot, out var previous) && previous != item)
            Bag.Add(previous);

        // Remove from Bag in case it was stored there
        Bag.Remove(item);

        Equipped[item.slot] = item;
        EventBus.Publish(new ItemEquippedEvent { Owner = _owner, Item = item });
    }

    /// <summary>
    /// Unequip the item in the given slot and move it to the Bag.
    /// Publishes ItemEquippedEvent with Item = null to signal a slot was cleared.
    /// </summary>
    public void Unequip(EquipmentSlot slot)
    {
        if (!Equipped.TryGetValue(slot, out var item)) return;
        Equipped.Remove(slot);
        Bag.Add(item);
        EventBus.Publish(new ItemEquippedEvent { Owner = _owner, Item = null });
    }

    // ── Stat aggregation ──────────────────────────────────────────────────

    public int GetTotalHP()  => Sum(e => e.EffectiveHP);
    public int GetTotalMP()  => Sum(e => e.EffectiveMP);
    public int GetTotalATK() => Sum(e => e.EffectiveATK);
    public int GetTotalDEF() => Sum(e => e.EffectiveDEF);
    public int GetTotalMAG() => Sum(e => e.EffectiveMAG);
    public int GetTotalRES() => Sum(e => e.EffectiveRES);
    public int GetTotalAGI() => Sum(e => e.EffectiveAGI);
    public int GetTotalLCK() => Sum(e => e.EffectiveLCK);

    private int Sum(Func<EquipmentSO, int> selector)
    {
        int total = 0;
        foreach (var item in Equipped.Values)
            total += selector(item);
        return total;
    }
}
```

- [ ] **Step 4.5: Run Inventory tests — all 7 must pass**

- [ ] **Step 4.6: Commit**

```bash
git add "My project/Assets/Scripts/Core/GameEvents.cs" \
        "My project/Assets/Scripts/Equipment/Inventory.cs" \
        "My project/Assets/Tests/EditMode/InventoryTests.cs"
git commit -m "feat: add Inventory system with equip/unequip/bag and ItemEquippedEvent"
```

---

## Task 5 — CharacterData Integration (Equipment Stat Bonuses)

Refactor `CharacterData` so that `ATK`, `DEF`, `MaxHP`, etc. automatically include equipment bonuses. Add an `Inventory` property. Existing tests must still pass.

**Files:**
- Modify: `Assets/Scripts/Characters/CharacterData.cs`

**Important:** This is a breaking refactor of the property declarations. Read the current file carefully before editing. The change:
- `public int MaxHP { get; private set; }` → stored in `_baseMaxHP`, property computed
- Same for `MaxMP`, `ATK`, `DEF`, `MAG`, `RES`, `AGI`, `LCK`
- `CurrentHP` and `CurrentMP` stay as mutable auto-properties (they track current combat state)
- `Initialize()` sets `_baseMaxHP` etc. instead of the properties

- [ ] **Step 5.1: Write the integration test first**

Add a new test file `Assets/Tests/EditMode/CharacterEquipmentTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class CharacterEquipmentTests
{
    private CharacterData MakeCharacter(int atk = 10, int def = 5, int hp = 100)
    {
        var c = new CharacterData();
        c.Initialize("Hero", hp, 50, atk, def, 8, 4, 6, 3);
        return c;
    }

    private EquipmentSO MakeItem(EquipmentSlot slot, int atkBonus = 0, int defBonus = 0, int hpBonus = 0)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.slot     = slot;
        item.atkBonus = atkBonus;
        item.defBonus = defBonus;
        item.hpBonus  = hpBonus;
        item.rarity   = EquipmentRarity.Common;
        return item;
    }

    [Test]
    public void ATK_WithNoEquipment_EqualsBaseATK()
    {
        var c = MakeCharacter(atk: 10);
        Assert.AreEqual(10, c.ATK);
    }

    [Test]
    public void ATK_WithWeapon_IncludesBonus()
    {
        var c    = MakeCharacter(atk: 10);
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        c.Inventory.Equip(sword);
        Assert.AreEqual(15, c.ATK);
    }

    [Test]
    public void MaxHP_WithArmor_IncludesBonus()
    {
        var c     = MakeCharacter(hp: 100);
        var armor = MakeItem(EquipmentSlot.Armor, hpBonus: 30);
        c.Inventory.Equip(armor);
        Assert.AreEqual(130, c.MaxHP);
    }

    [Test]
    public void Unequip_RemovesBonus()
    {
        var c    = MakeCharacter(atk: 10);
        var sword = MakeItem(EquipmentSlot.MainWeapon, atkBonus: 5);
        c.Inventory.Equip(sword);
        c.Inventory.Unequip(EquipmentSlot.MainWeapon);
        Assert.AreEqual(10, c.ATK);
    }

    [Test]
    public void CurrentHP_AfterInitialize_EqualsMaxHP()
    {
        var c = MakeCharacter(hp: 100);
        Assert.AreEqual(c.MaxHP, c.CurrentHP);
    }

    [Test]
    public void Heal_CappedByMaxHPWithEquipment()
    {
        var c     = MakeCharacter(hp: 100);
        var armor = MakeItem(EquipmentSlot.Armor, hpBonus: 50);
        c.Inventory.Equip(armor);
        c.TakeDamage(80);
        c.Heal(200);
        Assert.AreEqual(c.MaxHP, c.CurrentHP); // capped at 150
    }
}
```

- [ ] **Step 5.2: Verify test fails** (ATK_WithWeapon_IncludesBonus fails — CharacterData.ATK doesn't include equipment yet)

- [ ] **Step 5.3: Refactor CharacterData.cs**

Replace the full content of `Assets/Scripts/Characters/CharacterData.cs` with:

```csharp
using System.Collections.Generic;
using UnityEngine;

public class CharacterData
{
    public string CharacterName { get; private set; }

    // ── Base stats (set by Initialize; before equipment bonuses) ──────────
    private int _baseMaxHP, _baseMaxMP;
    private int _baseATK, _baseDEF, _baseMAG, _baseRES, _baseAGI, _baseLCK;

    // ── Effective stats = base + equipment bonus ──────────────────────────
    public int MaxHP => _baseMaxHP + Inventory.GetTotalHP();
    public int MaxMP => _baseMaxMP + Inventory.GetTotalMP();
    public int ATK   => _baseATK   + Inventory.GetTotalATK();
    public int DEF   => _baseDEF   + Inventory.GetTotalDEF();
    public int MAG   => _baseMAG   + Inventory.GetTotalMAG();
    public int RES   => _baseRES   + Inventory.GetTotalRES();
    public int AGI   => _baseAGI   + Inventory.GetTotalAGI();
    public int LCK   => _baseLCK   + Inventory.GetTotalLCK();

    public int CurrentHP { get; private set; }
    public int CurrentMP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    public ClassSO Class { get; private set; }
    public RaceSO Race { get; private set; }
    public int Level { get; private set; } = 1;
    public ElementType ElementalAffinity { get; private set; }

    public List<StatusEffect> ActiveStatuses { get; } = new();
    public List<SkillSO> Skills { get; } = new();
    public Dictionary<string, int> Cooldowns { get; } = new();

    // ── Inventory — lazy-initialized, always non-null after first access ──
    private Inventory _inventory;
    public Inventory Inventory => _inventory ??= new Inventory(this);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private HashSet<StatusEffectType> _testImmunities = new();
    public void SetImmunity_TestOnly(StatusEffectType type) => _testImmunities.Add(type);
    public ElementType ElementalAffinity_TestOnly { set => ElementalAffinity = value; }
#else
    private HashSet<StatusEffectType> _testImmunities = new();
#endif

    public void Initialize(string name, int hp, int mp, int atk, int def,
                           int mag, int res, int agi, int lck)
    {
        CharacterName = name;
        _baseMaxHP = hp; _baseMaxMP = mp;
        _baseATK = atk; _baseDEF = def;
        _baseMAG = mag; _baseRES = res;
        _baseAGI = agi; _baseLCK = lck;
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    public void InitializeFromSO(string name, ClassSO classSO, RaceSO raceSO, int level = 1)
    {
        CharacterName = name;
        Class = classSO;
        Race = raceSO;
        Level = level;

        int hp  = Mathf.RoundToInt((classSO.baseHP  + classSO.hpGrowth  * (level - 1)) * (1 + raceSO.hpModifier));
        int mp  = Mathf.RoundToInt((classSO.baseMP  + classSO.mpGrowth  * (level - 1)) * (1 + raceSO.mpModifier));
        int atk = Mathf.RoundToInt((classSO.baseATK + classSO.atkGrowth * (level - 1)) * (1 + raceSO.atkModifier));
        int def = Mathf.RoundToInt((classSO.baseDEF + classSO.defGrowth * (level - 1)) * (1 + raceSO.defModifier));
        int mag = Mathf.RoundToInt((classSO.baseMAG + classSO.magGrowth * (level - 1)) * (1 + raceSO.magModifier));
        int res = Mathf.RoundToInt((classSO.baseRES + classSO.resGrowth * (level - 1)) * (1 + raceSO.resModifier));
        int agi = Mathf.RoundToInt((classSO.baseAGI + classSO.agiGrowth * (level - 1)) * (1 + raceSO.agiModifier));
        int lck = Mathf.RoundToInt((classSO.baseLCK + classSO.lckGrowth * (level - 1)) * (1 + raceSO.lckModifier));

        Initialize(name, hp, mp, atk, def, mag, res, agi, lck);

        ElementalAffinity = raceSO.elementalAffinity != ElementType.None
            ? raceSO.elementalAffinity
            : classSO.elementalAffinity;

        if (classSO.startingSkills != null)
            Skills.AddRange(classSO.startingSkills);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP = System.Math.Max(0, CurrentHP - amount);
        if (IsDead) EventBus.Publish(new CharacterDiedEvent { Character = this });
    }

    public void Heal(int amount)
    {
        CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
    }

    public void SpendMP(int amount)
    {
        CurrentMP = System.Math.Max(0, CurrentMP - amount);
    }

    public void RestoreMP(int amount)
    {
        CurrentMP = System.Math.Min(MaxMP, CurrentMP + amount);
    }

    public int GetCooldown(SkillSO skill) =>
        Cooldowns.TryGetValue(skill.skillName, out int cd) ? cd : 0;

    public bool HasStatus(StatusEffectType type) =>
        ActiveStatuses.Exists(s => s.type == type);

    public bool IsImmuneToStatus(StatusEffectType type)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_testImmunities.Contains(type)) return true;
#endif
        return Race != null && System.Array.Exists(Race.statusImmunities, s => s == type);
    }
}
```

- [ ] **Step 5.4: Run ALL existing tests + new tests in Unity Test Runner**

Expected: All tests in `CharacterDataTests`, `DamageCalculatorTests`, `ActionEvaluatorTests`, etc. still pass.
Also: All 6 `CharacterEquipmentTests` pass.

- [ ] **Step 5.5: Commit**

```bash
git add "My project/Assets/Scripts/Characters/CharacterData.cs" \
        "My project/Assets/Tests/EditMode/CharacterEquipmentTests.cs"
git commit -m "feat: CharacterData stats now computed from base + equipment inventory bonus"
```

---

## Task 6 — ForgeSystem + Tests

ForgeSystem handles crafting: spending gold + materials to upgrade an item's rarity by one tier.

> **Note on mutating SOs:** In tests we use `ScriptableObject.CreateInstance<>()` which creates in-memory instances safe to mutate. In the editor, avoid mutating real asset files — runtime copies should be used when this system is connected to game flow (save system, Plan 7).

**Files:**
- Create: `Assets/Scripts/Equipment/ForgeSystem.cs`
- Create: `Assets/Tests/EditMode/ForgeSystemTests.cs`

- [ ] **Step 6.1: Write failing tests**

Create `Assets/Tests/EditMode/ForgeSystemTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class ForgeSystemTests
{
    private EquipmentSO MakeSword(EquipmentRarity rarity = EquipmentRarity.Common)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.itemName         = "Épée";
        item.slot             = EquipmentSlot.MainWeapon;
        item.rarity           = rarity;
        item.craftingGoldCost = 100;
        item.craftingMaterials = new[] { "FerBrut", "Charbon" };
        return item;
    }

    private string[] GoodMaterials() => new[] { "FerBrut", "Charbon", "Pierre" };

    [Test]
    public void Craft_Success_UpgradesRarity()
    {
        var sword  = MakeSword(EquipmentRarity.Common);
        var result = ForgeSystem.Craft(sword, gold: 200, playerMaterials: GoodMaterials());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(EquipmentRarity.Uncommon, sword.rarity);
    }

    [Test]
    public void Craft_InsufficientGold_Fails()
    {
        var sword  = MakeSword();
        var result = ForgeSystem.Craft(sword, gold: 50, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EquipmentRarity.Common, sword.rarity); // unchanged
    }

    [Test]
    public void Craft_MissingMaterial_Fails()
    {
        var sword  = MakeSword();
        var result = ForgeSystem.Craft(sword, gold: 200,
            playerMaterials: new[] { "FerBrut" }); // missing Charbon
        Assert.IsFalse(result.Success);
        Assert.AreEqual(EquipmentRarity.Common, sword.rarity);
    }

    [Test]
    public void Craft_LegendaryItem_Fails()
    {
        var sword  = MakeSword(EquipmentRarity.Legendary);
        var result = ForgeSystem.Craft(sword, gold: 1000, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Craft_NullItem_Fails()
    {
        var result = ForgeSystem.Craft(null, gold: 200, playerMaterials: GoodMaterials());
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Craft_Chain_CommonToUncommonToRare()
    {
        var sword = MakeSword(EquipmentRarity.Common);
        ForgeSystem.Craft(sword, 200, GoodMaterials());
        Assert.AreEqual(EquipmentRarity.Uncommon, sword.rarity);
        ForgeSystem.Craft(sword, 200, GoodMaterials());
        Assert.AreEqual(EquipmentRarity.Rare, sword.rarity);
    }
}
```

- [ ] **Step 6.2: Verify tests fail** (ForgeSystem not defined yet)

- [ ] **Step 6.3: Implement ForgeSystem.cs**

Create `Assets/Scripts/Equipment/ForgeSystem.cs`:

```csharp
/// <summary>
/// Forge system: upgrade an item's rarity by one tier.
/// Requires the player to have enough gold and all listed crafting materials.
/// </summary>
public static class ForgeSystem
{
    public class ForgeResult
    {
        public bool Success;
        public string Message;
        public EquipmentRarity NewRarity;
    }

    /// <summary>
    /// Attempts to upgrade the item's rarity by one tier.
    /// On success, mutates item.rarity in place (use runtime copies, not asset files).
    /// </summary>
    /// <param name="item">Item to upgrade.</param>
    /// <param name="gold">Gold the player currently has.</param>
    /// <param name="playerMaterials">Material names in the player's inventory.</param>
    public static ForgeResult Craft(EquipmentSO item, int gold, string[] playerMaterials)
    {
        if (item == null)
            return Fail("Aucun objet sélectionné.");

        if (!RaritySystem.CanUpgrade(item.rarity))
            return Fail($"Cet objet est déjà Légendaire — upgrade impossible.");

        if (gold < item.craftingGoldCost)
            return Fail($"Or insuffisant. Requis : {item.craftingGoldCost} or.");

        if (item.craftingMaterials != null)
        {
            foreach (var mat in item.craftingMaterials)
            {
                bool found = playerMaterials != null &&
                             System.Array.Exists(playerMaterials, m => m == mat);
                if (!found) return Fail($"Matériau manquant : {mat}.");
            }
        }

        item.rarity = RaritySystem.Upgrade(item.rarity);
        return new ForgeResult { Success = true, Message = "Amélioration réussie !", NewRarity = item.rarity };
    }

    private static ForgeResult Fail(string message) =>
        new ForgeResult { Success = false, Message = message };
}
```

- [ ] **Step 6.4: Run ForgeSystem tests — all 6 must pass**

- [ ] **Step 6.5: Commit**

```bash
git add "My project/Assets/Scripts/Equipment/ForgeSystem.cs" \
        "My project/Assets/Tests/EditMode/ForgeSystemTests.cs"
git commit -m "feat: add ForgeSystem — rarity upgrade via gold + materials"
```

---

## Task 7 — EnchantSystem + Tests

EnchantSystem handles enchanting: for Rare+ items, offer 3 random effect choices from the item's pool, then apply the chosen one to the enchantment slot.

**Files:**
- Create: `Assets/Scripts/Equipment/EnchantSystem.cs`
- Create: `Assets/Tests/EditMode/EnchantSystemTests.cs`

- [ ] **Step 7.1: Write failing tests**

Create `Assets/Tests/EditMode/EnchantSystemTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class EnchantSystemTests
{
    private EquipmentSO MakeItem(EquipmentRarity rarity, string[] pool = null)
    {
        var item = ScriptableObject.CreateInstance<EquipmentSO>();
        item.rarity     = rarity;
        item.effectPool = pool ?? new[] { "crit_5", "atk_3", "agi_2", "lck_4", "mp_regen" };
        return item;
    }

    // Standard enchant resources (Pierre runique + Essence élémentaire per spec §5.4)
    private static readonly string[] GoodResources = { "PierreRunique", "EssenceElementaire" };
    private static readonly string[] RequiredResources = { "PierreRunique", "EssenceElementaire" };

    [Test]
    public void GetOptions_CommonItem_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Common);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_RareItem_SucceedsWithUpTo3Options()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Options);
        Assert.LessOrEqual(result.Options.Length, 3);
        Assert.Greater(result.Options.Length, 0);
    }

    [Test]
    public void GetOptions_SmallPool_ReturnsAllOptions()
    {
        var item   = MakeItem(EquipmentRarity.Rare, pool: new[] { "crit_5", "atk_3" });
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Options.Length); // only 2 in pool
    }

    [Test]
    public void GetOptions_InsufficientGold_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 100, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_MissingEnchantResource_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: new[] { "PierreRunique" },           // missing EssenceElementaire
            requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void GetOptions_EmptyPool_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Rare, pool: new string[0]);
        var result = EnchantSystem.GetOptions(item, gold: 500, goldCost: 200,
            playerResources: GoodResources, requiredResources: RequiredResources);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void Apply_SetsEnchantmentEffect()
    {
        var item   = MakeItem(EquipmentRarity.Rare);
        var effect = new EquipmentEffect { effectId = "crit_5", displayName = "Critique +5%" };
        var result = EnchantSystem.Apply(item, effect);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("crit_5", item.enchantmentEffect.effectId);
    }

    [Test]
    public void Apply_CommonItem_Fails()
    {
        var item   = MakeItem(EquipmentRarity.Common);
        var effect = new EquipmentEffect { effectId = "crit_5", displayName = "Critique +5%" };
        var result = EnchantSystem.Apply(item, effect);
        Assert.IsFalse(result.Success);
        Assert.IsNull(item.enchantmentEffect);
    }

    [Test]
    public void Apply_ReplacesExistingEnchantment()
    {
        var item    = MakeItem(EquipmentRarity.Rare);
        var effect1 = new EquipmentEffect { effectId = "crit_5",  displayName = "Critique +5%" };
        var effect2 = new EquipmentEffect { effectId = "atk_3",   displayName = "ATK +3" };
        EnchantSystem.Apply(item, effect1);
        EnchantSystem.Apply(item, effect2);
        Assert.AreEqual("atk_3", item.enchantmentEffect.effectId);
    }
}
```

- [ ] **Step 7.2: Verify tests fail**

- [ ] **Step 7.3: Implement EnchantSystem.cs**

Create `Assets/Scripts/Equipment/EnchantSystem.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enchant system: offer random effects from an item's pool, then apply the chosen one.
/// Only Rare or better items can be enchanted (one enchantment slot per item).
/// Applying a new enchantment replaces the previous one.
///
/// Cost (per spec §5.4): enchantment resources (Pierre runique, Essence élémentaire) + or.
/// Note: like ForgeSystem, Apply mutates the SO in place — use runtime copies, not asset files.
/// </summary>
public static class EnchantSystem
{
    public class OptionsResult
    {
        public bool Success;
        public string Message;
        public EquipmentEffect[] Options;
    }

    public class ApplyResult
    {
        public bool Success;
        public string Message;
    }

    /// <summary>
    /// Validates eligibility and returns up to 3 randomly chosen effects from the item's pool.
    /// Call this to present the player with enchantment choices before calling Apply.
    /// </summary>
    /// <param name="item">Item to enchant.</param>
    /// <param name="gold">Gold the player currently has.</param>
    /// <param name="goldCost">Gold required for this enchantment.</param>
    /// <param name="playerResources">Resource names in the player's inventory.</param>
    /// <param name="requiredResources">Resource names required by the enchanter (e.g. PierreRunique, EssenceElementaire).</param>
    public static OptionsResult GetOptions(EquipmentSO item, int gold, int goldCost,
        string[] playerResources, string[] requiredResources)
    {
        if (item == null)
            return FailOptions("Aucun objet sélectionné.");

        if (!RaritySystem.CanEnchant(item.rarity))
            return FailOptions("L'enchantement requiert un objet Rare ou supérieur.");

        if (gold < goldCost)
            return FailOptions($"Or insuffisant. Requis : {goldCost} or.");

        if (requiredResources != null)
        {
            foreach (var res in requiredResources)
            {
                bool found = playerResources != null &&
                             System.Array.Exists(playerResources, r => r == res);
                if (!found) return FailOptions($"Ressource manquante : {res}.");
            }
        }

        if (item.effectPool == null || item.effectPool.Length == 0)
            return FailOptions("Aucun effet disponible pour cet objet.");

        // Draw up to 3 distinct effects from the pool (without replacement)
        var pool   = new List<string>(item.effectPool);
        var chosen = new List<EquipmentEffect>();
        int count  = System.Math.Min(3, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            chosen.Add(new EquipmentEffect
            {
                effectId    = pool[idx],
                displayName = pool[idx],    // designer assigns a proper name in real data
                effectType  = EquipmentEffectType.None,
            });
            pool.RemoveAt(idx);
        }

        return new OptionsResult { Success = true, Options = chosen.ToArray() };
    }

    /// <summary>
    /// Applies an enchantment effect to the item, replacing any existing enchantment.
    /// </summary>
    public static ApplyResult Apply(EquipmentSO item, EquipmentEffect effect)
    {
        if (item == null || effect == null)
            return new ApplyResult { Success = false, Message = "Données invalides." };

        if (!RaritySystem.CanEnchant(item.rarity))
            return new ApplyResult { Success = false, Message = "L'enchantement requiert un objet Rare ou supérieur." };

        item.enchantmentEffect = effect;
        return new ApplyResult { Success = true, Message = $"Enchantement appliqué : {effect.displayName}" };
    }

    private static OptionsResult FailOptions(string message) =>
        new OptionsResult { Success = false, Message = message };
}
```

- [ ] **Step 7.4: Run EnchantSystem tests — all 8 must pass**

- [ ] **Step 7.5: Run the full test suite** — verify ALL existing tests still pass (CharacterData, DamageCalculator, ActionEvaluator, DraftSystem, StatusManager, etc.)

- [ ] **Step 7.6: Commit**

```bash
git add "My project/Assets/Scripts/Equipment/EnchantSystem.cs" \
        "My project/Assets/Tests/EditMode/EnchantSystemTests.cs"
git commit -m "feat: add EnchantSystem — enchant Rare+ items with random pool effects"
```

---

## Task 8 — Starter Equipment Assets (RPGAssetCreator)

Add equipment creation to the editor utility so designers can generate starter gear via `RPG > Create Starter Assets`.

**Files:**
- Modify: `Assets/Editor/RPGAssetCreator.cs`

- [ ] **Step 8.1: Add Equipment folder creation**

In `CreateFolders()`, add:

```csharp
EnsureFolder("Assets/_Data/Equipment");
```

Full updated method:
```csharp
private static void CreateFolders()
{
    EnsureFolder("Assets/_Data");
    EnsureFolder("Assets/_Data/Skills");
    EnsureFolder("Assets/_Data/Races");
    EnsureFolder("Assets/_Data/Classes");
    EnsureFolder("Assets/_Data/AI");
    EnsureFolder("Assets/_Data/Arena");
    EnsureFolder("Assets/_Data/Equipment");
}
```

- [ ] **Step 8.2: Add CreateStarterEquipment() call to CreateAllAssets()**

Add `CreateStarterEquipment();` inside `CreateAllAssets()`, before `AssetDatabase.SaveAssets();`:

```csharp
[MenuItem("RPG/Create Starter Assets")]
public static void CreateAllAssets()
{
    CreateFolders();
    CreateSkills();
    CreateRaces();
    CreateClasses();
    CreateBotBrains();
    CreateArenaRoster();
    CreateStarterEquipment();   // ← new
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Debug.Log("RPG: All starter assets created successfully!");
}
```

- [ ] **Step 8.3: Implement CreateStarterEquipment()**

Add the following methods at the end of `RPGAssetCreator.cs` (before the closing `}` of the class, inside `#if UNITY_EDITOR`):

```csharp
// ────────────────────────────── EQUIPMENT ───────────────────────────────

private static void CreateStarterEquipment()
{
    // ── Warrior starter gear (physical focus) ──
    CreateEquipment("EpeeEnFer",
        itemName: "Épée en Fer", description: "Une lame solide, taillée pour le combat.",
        slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
        atkBonus: 6, defBonus: 0, hpBonus: 0, mpBonus: 0,
        magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 0,
        materials: new[] { "FerBrut", "Charbon" }, goldCost: 80,
        effectPool: new[] { "atk_3", "crit_5", "damage_on_hit" });

    CreateEquipment("BouclierEnBois",
        itemName: "Bouclier en Bois", description: "Protection basique en bois épais.",
        slot: EquipmentSlot.Offhand, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 4, hpBonus: 10, mpBonus: 0,
        magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 0,
        materials: new[] { "BoisDur" }, goldCost: 50,
        effectPool: new[] { "def_2", "hp_regen", "elemental_resist" });

    // ── Shared armor pieces ──
    CreateEquipment("CasqueEnCuir",
        itemName: "Casque en Cuir", description: "Un casque léger en cuir tanné.",
        slot: EquipmentSlot.Helmet, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 2, hpBonus: 15, mpBonus: 0,
        magBonus: 0, resBonus: 1, agiBonus: 0, lckBonus: 0,
        materials: new[] { "CuirBrut" }, goldCost: 40,
        effectPool: new[] { "def_2", "res_2", "hp_regen" });

    CreateEquipment("ArmureEnCuir",
        itemName: "Armure en Cuir", description: "Armure légère offrant une bonne mobilité.",
        slot: EquipmentSlot.Armor, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 5, hpBonus: 25, mpBonus: 0,
        magBonus: 0, resBonus: 2, agiBonus: 0, lckBonus: 0,
        materials: new[] { "CuirBrut", "FerBrut" }, goldCost: 100,
        effectPool: new[] { "def_3", "hp_regen", "elemental_resist" });

    CreateEquipment("BottesEnCuir",
        itemName: "Bottes en Cuir", description: "Des bottes confortables pour les longs voyages.",
        slot: EquipmentSlot.Boots, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 1, hpBonus: 0, mpBonus: 0,
        magBonus: 0, resBonus: 0, agiBonus: 3, lckBonus: 0,
        materials: new[] { "CuirBrut" }, goldCost: 30,
        effectPool: new[] { "agi_2", "lck_2", "crit_3" });

    CreateEquipment("AnneauBasique1",
        itemName: "Anneau Basique", description: "Un anneau simple qui porte chance.",
        slot: EquipmentSlot.Ring1, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 0, hpBonus: 0, mpBonus: 5,
        magBonus: 0, resBonus: 0, agiBonus: 0, lckBonus: 2,
        materials: new[] { "PierreGemme" }, goldCost: 60,
        effectPool: new[] { "lck_3", "crit_5", "mp_regen" });

    CreateEquipment("AnneauBasique2",
        itemName: "Anneau de Vigueur", description: "Un anneau qui renforce la résistance.",
        slot: EquipmentSlot.Ring2, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 0, hpBonus: 5, mpBonus: 0,
        magBonus: 0, resBonus: 2, agiBonus: 0, lckBonus: 0,
        materials: new[] { "PierreGemme" }, goldCost: 60,
        effectPool: new[] { "res_2", "hp_regen", "elemental_resist" });

    // ── Mage starter (magic focus) ──
    CreateEquipment("BatonArcanique",
        itemName: "Bâton Arcanique", description: "Un bâton chargé de puissance magique.",
        slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Common,
        atkBonus: 0, defBonus: 0, hpBonus: 0, mpBonus: 10,
        magBonus: 7, resBonus: 0, agiBonus: 0, lckBonus: 0,
        materials: new[] { "CristalArcanique", "BoisDur" }, goldCost: 90,
        effectPool: new[] { "mag_4", "mp_regen", "crit_5" });

    // ── Rare example (unlocked via Forge) ──
    CreateEquipment("EpeeRunique",
        itemName: "Épée Runique", description: "Lame gravée de runes anciennement oubliées.",
        slot: EquipmentSlot.MainWeapon, rarity: EquipmentRarity.Rare,
        atkBonus: 10, defBonus: 0, hpBonus: 0, mpBonus: 0,
        magBonus: 0, resBonus: 0, agiBonus: 2, lckBonus: 0,
        materials: new[] { "FerRunique", "Charbon", "CristalArcanique" }, goldCost: 250,
        effectPool: new[] { "atk_5", "crit_8", "damage_on_hit", "elemental_fire" });
}

private static void CreateEquipment(string assetName, string itemName, string description,
    EquipmentSlot slot, EquipmentRarity rarity,
    int atkBonus, int defBonus, int hpBonus, int mpBonus,
    int magBonus, int resBonus, int agiBonus, int lckBonus,
    string[] materials, int goldCost, string[] effectPool)
{
    string path = $"Assets/_Data/Equipment/{assetName}.asset";
    if (AssetDatabase.LoadAssetAtPath<EquipmentSO>(path) != null) return;
    var so = ScriptableObject.CreateInstance<EquipmentSO>();
    so.itemName          = itemName;
    so.description       = description;
    so.slot              = slot;
    so.rarity            = rarity;
    so.atkBonus          = atkBonus;
    so.defBonus          = defBonus;
    so.hpBonus           = hpBonus;
    so.mpBonus           = mpBonus;
    so.magBonus          = magBonus;
    so.resBonus          = resBonus;
    so.agiBonus          = agiBonus;
    so.lckBonus          = lckBonus;
    so.craftingMaterials = materials;
    so.craftingGoldCost  = goldCost;
    so.effectPool        = effectPool;
    AssetDatabase.CreateAsset(so, path);
}
```

- [ ] **Step 8.4: Run RPG > Create Starter Assets in Unity Editor**

Expected: 9 new `.asset` files appear in `Assets/_Data/Equipment/`:
- `EpeeEnFer`, `BouclierEnBois`, `CasqueEnCuir`, `ArmureEnCuir`, `BottesEnCuir`, `AnneauBasique1`, `AnneauBasique2`, `BatonArcanique`, `EpeeRunique`

- [ ] **Step 8.5: Verify in Unity Inspector**

Click any `.asset` in the Project window. Confirm the Inspector shows the correct slot, rarity, and stat values.

- [ ] **Step 8.6: Commit**

```bash
git add "My project/Assets/Editor/RPGAssetCreator.cs"
git commit -m "feat: RPGAssetCreator generates 9 starter equipment assets (8 Common + 1 Rare)"
```

---

## Final Verification

- [ ] **Run the full test suite in Unity Test Runner**

Expected result: ALL tests pass. This includes:
- `RaritySystemTests` (11 tests)
- `InventoryTests` (7 tests)
- `CharacterEquipmentTests` (6 tests)
- `ForgeSystemTests` (6 tests)
- `EnchantSystemTests` (8 tests)
- All existing Plan 1 + Plan 2 tests (CharacterDataTests, DamageCalculatorTests, etc.)

- [ ] **Play the game in Unity** — verify existing combat still works (equipment gives stat bonuses but doesn't break the battle flow)

---

## What Plan 3 Does NOT Include (Deferred)

- **Inventory UI** — InventoryUI panel, drag-and-drop, character sheet — deferred to a UI-focused plan
- **Save/Load** — equipment state persistence — Plan 7 (Save system)
- **Full effect integration** — `EquipmentEffect` applied in combat (DamageOnHit, MpRegenPerTurn, etc.) — Plan 4 (SkillTree + advanced effects)
- **Forge/Enchant UI** — NPC interfaces — deferred to UI plan
- **All 10 classes / 11 races** — full roster — Plan 10 (content completion)
