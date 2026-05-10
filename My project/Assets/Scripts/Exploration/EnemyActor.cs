using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// À poser sur un ennemi visible dans la scène (Sprite + Collider2D isTrigger).
/// Quand le joueur entre en contact, lance la scène Battle avec ce groupe d'ennemis.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyActor : MonoBehaviour
{
    [Header("Ennemi")]
    [SerializeField] private EnemySO enemySO;

    [Tooltip("Ennemis supplémentaires dans le groupe (laisser vide pour un groupe homogène)")]
    [SerializeField] private EnemySO[] extraEnemies;

    [Tooltip("Nb d'exemplaires de cet ennemi dans le groupe si extraEnemies est vide (1-3)")]
    [Range(1, 3)]
    [SerializeField] private int groupSize = 1;

    [Header("Zone (optionnel — pour le loot et les flags boss)")]
    [SerializeField] private CampaignZoneSO zone;

    [Header("Persistance")]
    [Tooltip("Clé unique pour mémoriser que cet ennemi a été vaincu. Laisser vide = réapparaît à chaque visite.")]
    [SerializeField] private string defeatedFlagKey;

    private bool _triggered;

    private void Start()
    {
        if (string.IsNullOrEmpty(defeatedFlagKey)) return;
        var session = GameSession.Instance;
        if (session != null && session.Flags.IsSet(defeatedFlagKey))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session == null)
        {
            Debug.LogError("[EnemyActor] GameSession.Instance est null — aucune session active.");
            return;
        }
        if (enemySO == null)
        {
            Debug.LogError("[EnemyActor] EnemySO non assigné sur " + gameObject.name);
            return;
        }
        if (SceneLoader.Instance == null) return;

        var enemies = BuildEnemyList();
        session.PendingEncounter = new CampaignEncounter(enemies, zone, isBoss: false)
        {
            DefeatedFlagKey = defeatedFlagKey
        };

        _triggered = true;
        SceneLoader.Instance.LoadScene("Battle");
    }

    private List<EnemySO> BuildEnemyList()
    {
        var list = new List<EnemySO>();

        if (extraEnemies != null && extraEnemies.Length > 0)
        {
            // Groupe mixte : l'ennemi principal + les extras
            list.Add(enemySO);
            foreach (var extra in extraEnemies)
                if (extra != null) list.Add(extra);
        }
        else
        {
            // Groupe homogène : N copies du même ennemi
            int count = Mathf.Clamp(groupSize, 1, 3);
            for (int i = 0; i < count; i++)
                list.Add(enemySO);
        }

        return list;
    }
}
