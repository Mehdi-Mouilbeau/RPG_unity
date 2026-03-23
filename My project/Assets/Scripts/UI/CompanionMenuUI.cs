using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sous-menu des compétences du compagnon en combat. Même pattern que SkillMenuUI.
/// </summary>
public class CompanionMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  skillListContainer;
    [SerializeField] private Button     btnBack;
    [SerializeField] private GameObject skillButtonPrefab;

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

        var containerRT = skillListContainer.GetComponent<RectTransform>();
        if (containerRT != null)
        {
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = Vector2.zero;
            containerRT.offsetMax = Vector2.zero;
        }

        panel.SetActive(true);
        BuildSkillList();
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearList();
    }

    private void BuildSkillList()
    {
        ClearList();

        var bm        = BattleManager.Instance;
        var character = bm?.ActiveCharacter;
        if (character?.Companion?.Definition == null) return;

        var companion = character.Companion;
        var skills    = companion.Definition.skills;

        if (skills == null || skills.Length == 0)
        {
            SpawnButton("Aucune compétence", usable: false, onClick: null);
        }
        else
        {
            foreach (var skill in skills)
            {
                int cd     = companion.GetCooldown(skill);
                bool usable = cd == 0;
                string label = cd > 0 ? $"{skill.skillName} [CD:{cd}]" : skill.skillName;
                var captured = skill;
                SpawnButton(label, usable, onClick: () => UseSkill(captured, character));
            }
        }

        LayoutButtons();
    }

    private void UseSkill(CompanionSkillSO skill, CharacterData character)
    {
        var bm = BattleManager.Instance;
        if (bm == null) { Hide(); return; }

        var target = ResolveTarget(skill, bm);
        character.UseCompanionSkill(
            skill, target,
            bm.GetAliveAllies().ToArray(),
            bm.GetAliveEnemies().ToArray());

        Hide();
        bm.Pass();
    }

    private static CharacterData ResolveTarget(CompanionSkillSO skill, BattleManager bm)
    {
        return skill.targetType switch
        {
            CompanionTargetType.EnemySingle or CompanionTargetType.AllEnemies =>
                bm.GetAliveEnemies().FirstOrDefault(),
            CompanionTargetType.AllySingle or CompanionTargetType.AllAllies =>
                bm.GetAliveAllies().FirstOrDefault(),
            _ => bm.GetAliveEnemies().FirstOrDefault()
        };
    }

    private void SpawnButton(string label, bool usable, System.Action onClick)
    {
        var go = Instantiate(skillButtonPrefab, skillListContainer);
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
