using UnityEngine;

/// <summary>
/// Portail de transition entre scènes. Quand le joueur entre dans le trigger,
/// charge la scène cible et pose un flag de progression si configuré.
/// </summary>
public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string progressionFlag;
    [SerializeField] private string unlockZoneFlag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var session = GameSession.Instance;
        if (session != null)
        {
            if (!string.IsNullOrEmpty(progressionFlag))
                session.Flags.Set(progressionFlag);
            if (!string.IsNullOrEmpty(unlockZoneFlag))
                session.Flags.Set(unlockZoneFlag);

            session.Save();
        }

        if (string.IsNullOrEmpty(targetScene)) { Debug.LogWarning("[SceneEntrance] targetScene non configuré"); return; }
        if (SceneLoader.Instance == null)
        {
            // Fallback : chargement direct (mode test sans MainMenu)
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
            return;
        }

        SceneLoader.Instance.LoadScene(targetScene);
    }
}
