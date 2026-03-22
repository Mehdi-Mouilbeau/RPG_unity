using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zone de collision qui déclenche un combat aléatoire quand le joueur entre dedans.
/// </summary>
public class EncounterTrigger : MonoBehaviour
{
    [SerializeField] private CampaignZoneSO zone;
    [Range(0f, 1f)]
    [SerializeField] private float encounterChance = 0.3f;

    private bool _triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session == null) return;

        if (Random.value > encounterChance) return;

        var enemy = zone.GetRandomEnemy();
        if (enemy == null) return;

        if (SceneLoader.Instance == null) { Debug.LogError("[EncounterTrigger] SceneLoader.Instance est null"); return; }

        session.PendingEncounter = new CampaignEncounter(
            new List<EnemySO> { enemy }, zone, isBoss: false);

        _triggered = true;
        SceneLoader.Instance.LoadScene("Battle");
    }
}
