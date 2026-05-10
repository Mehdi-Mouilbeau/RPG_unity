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
        // Force le panel à une position/taille fixe au centre-gauche de l'écran
        var panelRT = panel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin = new Vector2(0f, 0.5f);
            panelRT.anchorMax = new Vector2(0f, 0.5f);
            panelRT.pivot     = new Vector2(0f, 0.5f);
            panelRT.anchoredPosition = new Vector2(10f, 0f);
            panelRT.sizeDelta = new Vector2(280f, 300f);
        }

        // Force SkillListContainer à remplir tout le Panel
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
        ClearSkillList();
    }

    private const float ButtonHeight = 45f;
    private const float ButtonSpacing = 5f;

    private void BuildSkillList()
    {
        ClearSkillList();

        var character = BattleManager.Instance?.ActiveCharacter;
        if (character == null) return;

        Debug.Log($"[SkillMenuUI] {character.CharacterName} — MP: {character.CurrentMP}/{character.MaxMP} — Skills: {character.Skills.Count}");

        if (character.Skills.Count == 0)
        {
            SpawnButton("Aucune compétence", usable: false, onClick: null);
        }
        else
        {
            foreach (var skill in character.Skills)
            {
                int cd = character.GetCooldown(skill);
                bool canAfford = character.CurrentMP >= skill.mpCost;
                bool onCooldown = cd > 0;

                Debug.Log($"  Skill: {skill.skillName} | mpCost={skill.mpCost} | canAfford={canAfford} | cd={cd}");

                string suffix = onCooldown ? $" [CD:{cd}]"
                              : !canAfford ? $" [{skill.mpCost}MP requis]"
                              : $" ({skill.mpCost}MP)";

                var capturedSkill = skill;
                // Toujours cliquable — ActionResolver gère le rejet si MP insuffisant
                SpawnButton(skill.skillName + suffix, usable: true, onClick: () => UseSkill(capturedSkill));
            }
        }

        LayoutButtons();
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

    /// <summary>Positionne les boutons en colonne de haut en bas avec taille fixe.</summary>
    private void LayoutButtons()
    {
        float yOffset = 10f; // marge du haut
        foreach (var go in _spawnedButtons)
        {
            var rt = go.GetComponent<RectTransform>();
            // Ancrage coin haut-gauche, taille fixe
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -yOffset);
            rt.sizeDelta = new Vector2(260f, ButtonHeight);

            yOffset += ButtonHeight + ButtonSpacing;
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
