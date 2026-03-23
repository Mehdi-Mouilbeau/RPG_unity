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
