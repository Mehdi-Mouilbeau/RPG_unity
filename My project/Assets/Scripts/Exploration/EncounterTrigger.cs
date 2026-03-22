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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session == null) return;

        if (Random.value > encounterChance) return;

        var enemy = zone.GetRandomEnemy();
        if (enemy == null) return;

        session.PendingEncounter = new CampaignEncounter(
            new List<EnemySO> { enemy }, zone, isBoss: false);

        SceneLoader.Instance.LoadScene("Battle");
    }
}
