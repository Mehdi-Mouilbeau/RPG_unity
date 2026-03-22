using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger unique qui déclenche le combat contre le boss de la zone.
/// Se désactive si le boss est déjà vaincu.
/// </summary>
public class BossTrigger : MonoBehaviour
{
    [SerializeField] private CampaignZoneSO zone;

    private void Start()
    {
        if (zone == null) return;
        if (GameSession.Instance != null
            && !string.IsNullOrEmpty(zone.bossDefeatedFlagKey)
            && GameSession.Instance.Flags.IsSet(zone.bossDefeatedFlagKey))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var session = GameSession.Instance;
        if (session == null || zone == null || zone.boss == null) return;

        session.PendingEncounter = new CampaignEncounter(
            new List<EnemySO> { zone.boss }, zone, isBoss: true);

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("Battle");
        else
            Debug.LogError("[BossTrigger] SceneLoader.Instance est null");
    }
}
