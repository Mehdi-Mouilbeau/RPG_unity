using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Génère deux panneaux UI en runtime :
///   - Panneau joueurs  : ancré en bas-gauche de l'écran
///   - Panneau ennemis  : ancré en haut-droite de l'écran
/// Aucune dépendance aux objets de scène sauf activeTurnIndicator.
/// </summary>
public class BattleHUD : MonoBehaviour
{
    [Header("Tour actif")]
    [SerializeField] private TMP_Text activeTurnIndicator;

    // Références aux textes générés
    private readonly List<TMP_Text> _playerNames = new();
    private readonly List<TMP_Text> _playerHPs   = new();
    private readonly List<TMP_Text> _playerMPs   = new();

    private readonly List<TMP_Text> _enemyNames = new();
    private readonly List<TMP_Text> _enemyHPs   = new();

    private bool _initialized = false;

    private void OnEnable()
    {
        EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Subscribe<ActionResolvedEvent>(OnActionResolved);
        EventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        EventBus.Unsubscribe<ActionResolvedEvent>(OnActionResolved);
        EventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
    }

    public void Initialize(BattleManager battle)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        BuildPlayerPanel(canvas, battle.GetPlayerTeam());
        BuildEnemyPanel(canvas, battle.GetEnemyTeam());
        _initialized = true;
    }

    public void RefreshAll(BattleManager battle)
    {
        var players = battle.GetPlayerTeam();
        var enemies = battle.GetEnemyTeam();

        for (int i = 0; i < players.Count && i < _playerNames.Count; i++)
        {
            bool dead = players[i].IsDead;
            _playerNames[i].text  = dead ? $"<s>{players[i].CharacterName}</s>" : players[i].CharacterName;
            _playerNames[i].color = dead ? Color.grey : Color.white;
            SetHPText(_playerHPs[i], players[i].CurrentHP, players[i].MaxHP);
            SetMPText(_playerMPs[i], players[i].CurrentMP, players[i].MaxMP);
        }

        for (int i = 0; i < enemies.Count && i < _enemyNames.Count; i++)
        {
            _enemyNames[i].text = enemies[i].IsDead
                ? $"<s>{enemies[i].CharacterName}</s>"
                : enemies[i].CharacterName;
            SetHPText(_enemyHPs[i], enemies[i].CurrentHP, enemies[i].MaxHP);
        }
    }

    // ── Construction des panneaux ─────────────────────────────────────────

    private void BuildPlayerPanel(Canvas canvas, List<CharacterData> players)
    {
        _playerNames.Clear(); _playerHPs.Clear(); _playerMPs.Clear();

        // Panneau en bas-gauche
        var panel = CreatePanel(canvas, "PlayerPanel",
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(0f, 0f),
            pivot:     new Vector2(0f, 0f),
            offset:    new Vector2(10f, 10f),
            size:      new Vector2(320f, players.Count * 56f + 8f));

        AddBackground(panel, new Color(0f, 0f, 0f, 0.55f));

        for (int i = 0; i < players.Count; i++)
        {
            var p    = players[i];
            float y  = panel.sizeDelta.y - 8f - i * 56f - 28f;

            var name = AddText(panel, $"PName{i}", new Vector2(8f, y), new Vector2(120f, 22f), p.CharacterName, 13f, TextAlignmentOptions.MidlineLeft, Color.white);
            var hp   = AddText(panel, $"PHP{i}",   new Vector2(130f, y + 12f), new Vector2(180f, 20f), "", 12f, TextAlignmentOptions.MidlineLeft, Color.white);
            var mp   = AddText(panel, $"PMP{i}",   new Vector2(130f, y - 10f), new Vector2(180f, 18f), "", 11f, TextAlignmentOptions.MidlineLeft, new Color(0.3f, 0.7f, 1f));

            SetHPText(hp, p.CurrentHP, p.MaxHP);
            SetMPText(mp, p.CurrentMP, p.MaxMP);

            _playerNames.Add(name);
            _playerHPs.Add(hp);
            _playerMPs.Add(mp);
        }
    }

    private void BuildEnemyPanel(Canvas canvas, List<CharacterData> enemies)
    {
        _enemyNames.Clear(); _enemyHPs.Clear();

        // Panneau en haut-droite
        var panel = CreatePanel(canvas, "EnemyPanel",
            anchorMin: new Vector2(1f, 1f),
            anchorMax: new Vector2(1f, 1f),
            pivot:     new Vector2(1f, 1f),
            offset:    new Vector2(-10f, -10f),
            size:      new Vector2(260f, enemies.Count * 48f + 8f));

        AddBackground(panel, new Color(0f, 0f, 0f, 0.55f));

        for (int i = 0; i < enemies.Count; i++)
        {
            var e    = enemies[i];
            float y  = panel.sizeDelta.y - 8f - i * 48f - 24f;

            var name = AddText(panel, $"EName{i}", new Vector2(8f, y), new Vector2(140f, 22f), e.CharacterName, 13f, TextAlignmentOptions.MidlineLeft, Color.white);
            var hp   = AddText(panel, $"EHP{i}",   new Vector2(150f, y), new Vector2(102f, 22f), "", 12f, TextAlignmentOptions.MidlineRight, Color.white);

            SetHPText(hp, e.CurrentHP, e.MaxHP);

            _enemyNames.Add(name);
            _enemyHPs.Add(hp);
        }
    }

    // ── Helpers UI ────────────────────────────────────────────────────────

    private static RectTransform CreatePanel(Canvas canvas, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 offset, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        go.layer = 5;

        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.anchoredPosition = offset;
        rt.sizeDelta = size;
        return rt;
    }

    private static void AddBackground(RectTransform panel, Color color)
    {
        var img         = panel.gameObject.AddComponent<Image>();
        img.color       = color;
        img.raycastTarget = false;
    }

    private static TMP_Text AddText(RectTransform panel, string name,
        Vector2 localPos, Vector2 size, string text, float fontSize,
        TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(panel, false);
        go.layer = 5;

        var rt              = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 0f);
        rt.anchorMax        = new Vector2(0f, 0f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = localPos;
        rt.sizeDelta        = size;

        var tmp         = go.AddComponent<TextMeshProUGUI>();
        tmp.text        = text;
        tmp.fontSize    = fontSize;
        tmp.alignment   = alignment;
        tmp.color       = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ── Events ────────────────────────────────────────────────────────────

    private void OnTurnStarted(TurnStartedEvent e)
    {
        if (activeTurnIndicator != null)
            activeTurnIndicator.text = $"Tour de : {e.Character.CharacterName}";

        if (BattleManager.Instance == null) return;
        if (!_initialized) Initialize(BattleManager.Instance);
        RefreshAll(BattleManager.Instance);
    }

    private void OnActionResolved(ActionResolvedEvent e)
    {
        if (BattleManager.Instance != null)
            RefreshAll(BattleManager.Instance);
    }

    private void OnBattleEnded(BattleEndedEvent e)
    {
        if (activeTurnIndicator != null)
            activeTurnIndicator.gameObject.SetActive(false);
    }

    // ── Couleurs ──────────────────────────────────────────────────────────

    private static void SetHPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text  = $"HP {current}/{max}";
        label.color = GetHealthColor(max > 0 ? (float)current / max : 0f);
    }

    private static void SetMPText(TMP_Text label, int current, int max)
    {
        if (label == null) return;
        label.text  = $"MP {current}/{max}";
        label.color = GetMPColor(max > 0 ? (float)current / max : 0f);
    }

    private static Color GetHealthColor(float ratio)
    {
        if (ratio > 0.5f) return Color.white;
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }

    private static Color GetMPColor(float ratio)
    {
        if (ratio > 0.5f) return new Color(0.3f, 0.7f, 1f);
        if (ratio > 0.25f) return Color.yellow;
        return Color.red;
    }
}
