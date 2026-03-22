using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau de sélection des compétences en combat.
/// Appelé par ActionMenuUI quand le joueur clique "Compétences".
/// </summary>
public class SkillMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  skillListContainer; // parent des boutons générés
    [SerializeField] private Button     btnBack;
    [SerializeField] private GameObject skillButtonPrefab; // prefab: Button + TMP_Text enfant

    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

    private void Start()
    {
        if (btnBack != null) btnBack.onClick.AddListener(Hide);
        Hide();
    }

    public void Show()
    {
        panel.SetActive(true);
        BuildSkillList();
    }

    public void Hide()
    {
        panel.SetActive(false);
        ClearSkillList();
    }

    private void BuildSkillList()
    {
        ClearSkillList();

        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        foreach (var skill in character.Skills)
        {
            var go = Instantiate(skillButtonPrefab, skillListContainer);
            _spawnedButtons.Add(go);

            var btn = go.GetComponent<Button>();
            var lbl = go.GetComponentInChildren<TMP_Text>();

            int cd = character.GetCooldown(skill);
            bool canAfford = character.CurrentMP >= skill.mpCost;
            bool onCooldown = cd > 0;
            bool usable = canAfford && !onCooldown;

            // Label : "NomSkill (3 MP)" ou "NomSkill [CD:2]" ou "NomSkill [MP insuffisant]"
            string suffix = onCooldown  ? $" [CD:{cd}]"
                          : !canAfford  ? $" [{skill.mpCost}MP requis]"
                          : $" ({skill.mpCost}MP)";
            if (lbl != null) lbl.text = skill.skillName + suffix;

            btn.interactable = usable;

            var capturedSkill = skill;
            btn.onClick.AddListener(() => UseSkill(capturedSkill));
        }

        // Si aucune compétence disponible
        if (character.Skills.Count == 0)
        {
            var go = Instantiate(skillButtonPrefab, skillListContainer);
            _spawnedButtons.Add(go);
            var lbl = go.GetComponentInChildren<TMP_Text>();
            if (lbl != null) lbl.text = "Aucune compétence";
            go.GetComponent<Button>().interactable = false;
        }
    }

    private void UseSkill(SkillSO skill)
    {
        var bm = BattleManager.Instance;
        if (bm == null) { Hide(); return; }

        CharacterData target = ResolveTarget(skill);
        if (target == null) { Hide(); return; }

        Hide();
        bm.ExecuteAction(target, skill);
    }

    private CharacterData ResolveTarget(SkillSO skill)
    {
        var bm = BattleManager.Instance;
        switch (skill.targetType)
        {
            case SkillTargetType.SingleEnemy:
            case SkillTargetType.AllEnemies:
                var enemies = bm.GetAliveEnemies();
                return enemies.Count > 0 ? enemies[0] : null;

            case SkillTargetType.SingleAlly:
            case SkillTargetType.AllAllies:
                var allies = bm.GetAliveAllies();
                return allies.Count > 0 ? allies[0] : null;

            case SkillTargetType.Self:
                return bm.ActiveCharacter;

            default:
                return bm.GetAliveEnemies().Count > 0 ? bm.GetAliveEnemies()[0] : null;
        }
    }

    private void ClearSkillList()
    {
        foreach (var go in _spawnedButtons)
            if (go != null) Destroy(go);
        _spawnedButtons.Clear();
    }
}
