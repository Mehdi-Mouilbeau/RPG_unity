using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sous-menu des consommables en combat. Même pattern que SkillMenuUI.
/// </summary>
public class ItemMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  itemListContainer;
    [SerializeField] private Button     btnBack;
    [SerializeField] private GameObject itemButtonPrefab;

    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
    private const float ButtonHeight  = 45f;
    private const float ButtonSpacing = 5f;

    private void Start()
    {
        if (btnBack != null) btnBack.onClick.AddListener(Hide);
        Hide();
    }

    public void Show()
    {
        var panelRT = panel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin       = new Vector2(0f, 0.5f);
            panelRT.anchorMax       = new Vector2(0f, 0.5f);
            panelRT.pivot           = new Vector2(0f, 0.5f);
            panelRT.anchoredPosition = new Vector2(10f, 0f);
            panelRT.sizeDelta       = new Vector2(280f, 300f);
        }

        var containerRT = itemListContainer.GetComponent<RectTransform>();
        if (containerRT != null)
        {
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;
        }

        panel.SetActive(true);
        BuildItemList();
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearList();
    }

    private void BuildItemList()
    {
        ClearList();

        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        var consumables = character.Inventory.Consumables;

        if (consumables.Count == 0)
        {
            SpawnButton("Aucun objet", usable: false, onClick: null);
        }
        else
        {
            foreach (var item in consumables)
            {
                bool usable = IsUsable(item, character);
                var captured = item;
                SpawnButton(item.itemName, usable, onClick: () => UseItem(captured, character));
            }
        }

        LayoutButtons();
    }

    private static bool IsUsable(ConsumableSO item, CharacterData c)
    {
        return item.effectType switch
        {
            ConsumableEffectType.HealHP     => c.CurrentHP < c.MaxHP,
            ConsumableEffectType.RestoreMP  => c.CurrentMP < c.MaxMP,
            ConsumableEffectType.CureStatus => c.HasStatus(StatusEffectType.Poison)
                                            || c.HasStatus(StatusEffectType.Burn),
            _ => true
        };
    }

    private void UseItem(ConsumableSO item, CharacterData character)
    {
        switch (item.effectType)
        {
            case ConsumableEffectType.HealHP:
                int heal = Mathf.RoundToInt(character.MaxHP * item.value);
                character.Heal(heal);
                break;
            case ConsumableEffectType.RestoreMP:
                character.RestoreMP(Mathf.RoundToInt(item.value));
                break;
            case ConsumableEffectType.CureStatus:
                character.ActiveStatuses.RemoveAll(
                    s => s.type == StatusEffectType.Poison || s.type == StatusEffectType.Burn);
                break;
        }

        character.Inventory.RemoveConsumable(item);
        Hide();
        BattleManager.Instance?.Pass();
    }

    private void SpawnButton(string label, bool usable, System.Action onClick)
    {
        var go = Instantiate(itemButtonPrefab, itemListContainer);
        _spawnedButtons.Add(go);

        var btn = go.GetComponent<Button>();
        var lbl = go.GetComponentInChildren<TMP_Text>();

        if (lbl != null) lbl.text = label;
        btn.interactable = usable;
        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private void LayoutButtons()
    {
        float yOffset = 10f;
        foreach (var go in _spawnedButtons)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -yOffset);
            rt.sizeDelta        = new Vector2(260f, ButtonHeight);
            yOffset += ButtonHeight + ButtonSpacing;
        }
    }

    private void ClearList()
    {
        foreach (var go in _spawnedButtons)
            if (go != null) Destroy(go);
        _spawnedButtons.Clear();
    }
}
