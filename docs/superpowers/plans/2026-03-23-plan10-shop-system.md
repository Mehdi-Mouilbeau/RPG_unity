# Shop System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Système de boutique générique — les NPCs marchands ouvrent un panneau de vente d'équipements et de consommables, avec déduction de l'or et sauvegarde automatique.

**Architecture:** `ShopSO` (données), `ShopOpenedEvent` (EventBus), `NpcInteractor` (déclencheur), `ShopUI` (affichage) — pattern EventBus + SkillMenuUI pour les boutons dynamiques.

**Tech Stack:** Unity 6, C#, TextMeshPro, Unity UI, EventBus, NUnit (EditMode tests)

**Spec:** `docs/superpowers/specs/2026-03-23-shop-system-design.md`

---

## File Map

| Fichier | Action |
|---------|--------|
| `My project/Assets/Scripts/Data/ShopSO.cs` | Nouveau ScriptableObject |
| `My project/Assets/Scripts/Core/GameEvents.cs` | Ajouter `ShopOpenedEvent` |
| `My project/Assets/Scripts/Exploration/NpcInteractor.cs` | Ajouter `shopData` + publish event |
| `My project/Assets/Scripts/UI/ShopUI.cs` | Nouveau MonoBehaviour |
| `My project/Assets/Tests/EditMode/ShopSystemTests.cs` | Nouveaux tests EditMode |

---

## Task 1 : ShopSO + GameEvents + NpcInteractor + tests

**Files:**
- Create: `My project/Assets/Scripts/Data/ShopSO.cs`
- Modify: `My project/Assets/Scripts/Core/GameEvents.cs`
- Modify: `My project/Assets/Scripts/Exploration/NpcInteractor.cs`
- Create: `My project/Assets/Tests/EditMode/ShopSystemTests.cs`

- [ ] **Créer `ShopSO.cs`** :

```csharp
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShop", menuName = "RPG/Shop")]
public class ShopSO : ScriptableObject
{
    public string shopName;

    [Serializable]
    public struct EquipmentEntry
    {
        public EquipmentSO item;
        public int         price;
    }

    [Serializable]
    public struct ConsumableEntry
    {
        public ConsumableSO item;
        public int          price;
    }

    public EquipmentEntry[]  equipmentItems;
    public ConsumableEntry[] consumableItems;
}
```

- [ ] **Ajouter `ShopOpenedEvent` dans `GameEvents.cs`** — à la fin du fichier :

```csharp
public struct ShopOpenedEvent { public ShopSO Shop; }
```

- [ ] **Mettre à jour `NpcInteractor.cs`** — remplacer le fichier entier :

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attaché à un PNJ. Quand le joueur entre dans le trigger et appuie sur E :
/// - Si shopData est assigné : ouvre la boutique via ShopOpenedEvent
/// - Sinon : affiche le dialogue (stub Yarn Spinner)
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private string dialogueNode = "Villageois";
    [SerializeField] private ShopSO shopData;

    private bool _playerNearby;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }

    private void Update()
    {
        if (!_playerNearby) return;
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (shopData != null)
            EventBus.Publish(new ShopOpenedEvent { Shop = shopData });
        else
            Debug.Log($"[NPC] Dialogue : {dialogueNode} (Yarn Spinner sera câblé en Plan 7)");
    }
}
```

- [ ] **Créer les tests** — `My project/Assets/Tests/EditMode/ShopSystemTests.cs` :

```csharp
using NUnit.Framework;

public class ShopSystemTests
{
    // ── ShopSO data ───────────────────────────────────────────────────────

    [Test]
    public void ShopSO_EquipmentEntry_HasItemAndPrice()
    {
        var shop      = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        var equipment = UnityEngine.ScriptableObject.CreateInstance<EquipmentSO>();
        equipment.itemName = "Épée";

        shop.equipmentItems = new[] { new ShopSO.EquipmentEntry { item = equipment, price = 100 } };

        Assert.AreEqual(1, shop.equipmentItems.Length);
        Assert.AreEqual("Épée", shop.equipmentItems[0].item.itemName);
        Assert.AreEqual(100,    shop.equipmentItems[0].price);

        UnityEngine.Object.DestroyImmediate(equipment);
        UnityEngine.Object.DestroyImmediate(shop);
    }

    [Test]
    public void ShopSO_ConsumableEntry_HasItemAndPrice()
    {
        var shop      = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        var consumable = UnityEngine.ScriptableObject.CreateInstance<ConsumableSO>();
        consumable.itemName = "Potion de Soin";

        shop.consumableItems = new[] { new ShopSO.ConsumableEntry { item = consumable, price = 50 } };

        Assert.AreEqual(1, shop.consumableItems.Length);
        Assert.AreEqual("Potion de Soin", shop.consumableItems[0].item.itemName);
        Assert.AreEqual(50,               shop.consumableItems[0].price);

        UnityEngine.Object.DestroyImmediate(consumable);
        UnityEngine.Object.DestroyImmediate(shop);
    }

    // ── Inventory.Bag (used by buy logic) ─────────────────────────────────

    [Test]
    public void Inventory_Bag_CanAddEquipment()
    {
        var character = new CharacterData();
        character.Initialize("Test", 100, 50, 10, 5, 5, 5, 8, 3);

        var item = UnityEngine.ScriptableObject.CreateInstance<EquipmentSO>();
        item.itemName = "Épée";
        item.slot     = EquipmentSlot.MainWeapon;

        character.Inventory.Bag.Add(item);

        Assert.AreEqual(1, character.Inventory.Bag.Count);
        Assert.AreEqual("Épée", character.Inventory.Bag[0].itemName);

        UnityEngine.Object.DestroyImmediate(item);
    }

    [Test]
    public void Inventory_Bag_DefaultEmpty()
    {
        var character = new CharacterData();
        character.Initialize("Test", 100, 50, 10, 5, 5, 5, 8, 3);
        Assert.AreEqual(0, character.Inventory.Bag.Count);
    }

    // ── ShopOpenedEvent ───────────────────────────────────────────────────

    [Test]
    public void ShopOpenedEvent_HasShopField()
    {
        var shop = UnityEngine.ScriptableObject.CreateInstance<ShopSO>();
        shop.shopName = "Marchand";

        var evt = new ShopOpenedEvent { Shop = shop };

        Assert.AreEqual("Marchand", evt.Shop.shopName);
        UnityEngine.Object.DestroyImmediate(shop);
    }
}
```

- [ ] **Lancer les tests** — Window > General > Test Runner > EditMode > Run All
  - Résultat attendu : tous les nouveaux tests **PASS**

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/Data/ShopSO.cs" \
        "My project/Assets/Scripts/Core/GameEvents.cs" \
        "My project/Assets/Scripts/Exploration/NpcInteractor.cs" \
        "My project/Assets/Tests/EditMode/ShopSystemTests.cs"
git commit -m "feat: add ShopSO, ShopOpenedEvent, update NpcInteractor for shop support"
```

---

## Task 2 : ShopUI

**Files:**
- Create: `My project/Assets/Scripts/UI/ShopUI.cs`

- [ ] **Créer `ShopUI.cs`** :

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panneau de boutique. S'abonne à ShopOpenedEvent via EventBus.
/// Pattern identique à SkillMenuUI (boutons dynamiques, layout fixe).
/// </summary>
public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   shopNameText;
    [SerializeField] private TMP_Text   goldText;
    [SerializeField] private Transform  itemListContainer;
    [SerializeField] private Button     btnClose;
    [SerializeField] private GameObject itemButtonPrefab;

    private readonly List<GameObject> _spawnedButtons = new();
    private ShopSO _currentShop;

    private const float ButtonHeight  = 45f;
    private const float ButtonSpacing = 5f;

    private System.Action<ShopOpenedEvent> _onShopOpened;

    private void OnEnable()
    {
        _onShopOpened = e => Open(e.Shop);
        EventBus.Subscribe(_onShopOpened);
    }

    private void OnDisable()
    {
        if (_onShopOpened != null)
            EventBus.Unsubscribe(_onShopOpened);
    }

    private void Start()
    {
        if (btnClose != null) btnClose.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    public void Open(ShopSO shop)
    {
        if (shop == null) return;
        _currentShop = shop;

        ClearButtons();

        if (shopNameText != null) shopNameText.text = shop.shopName;
        RefreshGoldText();

        var session = GameSession.Instance;
        int gold    = session != null ? session.Gold : 0;

        if (shop.equipmentItems != null)
        {
            foreach (var entry in shop.equipmentItems)
            {
                if (entry.item == null) continue;
                var captured = entry;
                SpawnButton($"{entry.item.itemName} — {entry.price}G",
                    usable: gold >= entry.price,
                    onClick: () => BuyEquipment(captured));
            }
        }

        if (shop.consumableItems != null)
        {
            foreach (var entry in shop.consumableItems)
            {
                if (entry.item == null) continue;
                var captured = entry;
                SpawnButton($"{entry.item.itemName} — {entry.price}G",
                    usable: gold >= entry.price,
                    onClick: () => BuyConsumable(captured));
            }
        }

        LayoutButtons();

        var panelRT = panel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin        = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax        = new Vector2(0.5f, 0.5f);
            panelRT.pivot            = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta        = new Vector2(320f, 400f);
        }

        panel.SetActive(true);
    }

    private void BuyEquipment(ShopSO.EquipmentEntry entry)
    {
        var session = GameSession.Instance;
        if (session == null) return;
        var character = session.ActiveCharacter;
        if (character == null) return;
        if (session.Gold < entry.price) return;

        session.Gold -= entry.price;
        character.Inventory.Bag.Add(entry.item);
        session.Save();
        Open(_currentShop);
    }

    private void BuyConsumable(ShopSO.ConsumableEntry entry)
    {
        var session = GameSession.Instance;
        if (session == null) return;
        var character = session.ActiveCharacter;
        if (character == null) return;
        if (session.Gold < entry.price) return;

        session.Gold -= entry.price;
        character.Inventory.AddConsumable(entry.item);
        session.Save();
        Open(_currentShop);
    }

    private void RefreshGoldText()
    {
        if (goldText == null) return;
        var session = GameSession.Instance;
        goldText.text = session != null ? $"Or : {session.Gold}G" : "Or : 0G";
    }

    public void Close()
    {
        panel.SetActive(false);
        ClearButtons();
        _currentShop = null;
    }

    private void SpawnButton(string label, bool usable, System.Action onClick)
    {
        var go  = Instantiate(itemButtonPrefab, itemListContainer);
        _spawnedButtons.Add(go);

        var btn = go.GetComponent<Button>();
        var lbl = go.GetComponentInChildren<TMP_Text>();

        if (lbl != null) lbl.text = label;
        if (btn != null)
        {
            btn.interactable = usable;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
        }
    }

    private void LayoutButtons()
    {
        float yOffset = 10f;
        foreach (var go in _spawnedButtons)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) continue;
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -yOffset);
            rt.sizeDelta        = new Vector2(260f, ButtonHeight);
            yOffset += ButtonHeight + ButtonSpacing;
        }
    }

    private void ClearButtons()
    {
        foreach (var go in _spawnedButtons)
            if (go != null) Destroy(go);
        _spawnedButtons.Clear();
    }
}
```

- [ ] **Vérifier compilation** dans Unity — aucune erreur.

- [ ] **Commit**

```bash
git add "My project/Assets/Scripts/UI/ShopUI.cs"
git commit -m "feat: add ShopUI with dynamic item buttons and gold management"
```

---

## Task 3 : Setup scène Village (Unity Editor — manuel)

- [ ] **Créer le panneau ShopUI dans la scène Village** :
  - Clic droit sur Canvas → **UI > Panel** → renommer `ShopPanel`
  - Sous ShopPanel :
    - `ShopNameText` (UI > Text - TextMeshPro)
    - `GoldText` (UI > Text - TextMeshPro)
    - `ItemListContainer` (Create Empty)
    - `BtnClose` (UI > Button - TextMeshPro, texte "Fermer")
  - Sur `ShopPanel` : **Add Component** → `ShopUI`
  - Câbler les champs : `Panel`, `Shop Name Text`, `Gold Text`, `Item List Container`, `Btn Close`, `Item Button Prefab` → `SkillButtonPrefab`

- [ ] **Créer des assets ShopSO** (clic droit dans Project → Create > RPG > Shop) :
  - `BoutiqueArmes` : shopName = "Marchand d'armes", ajouter des EquipmentSO de slots MainWeapon/Offhand
  - `ApothicaireDeSoin` : shopName = "Apothicaire", ajouter `PotionDeSoin` (50G), `Antidote` (30G), `Ether` (40G)

- [ ] **Assigner ShopSO aux NPCs marchands** :
  - Sélectionner un NPC avec `NpcInteractor` dans la scène Village
  - Assigner son `ShopSO` dans le champ `Shop Data`

- [ ] **Tester en Play** :
  - Entrer dans le Village, approcher un marchand, appuyer E
  - Le panneau ShopUI s'ouvre avec les items et l'or affiché
  - Acheter un item → or diminue, bouton grisé si insuffisant
  - Sauvegarder/charger → items persistés
