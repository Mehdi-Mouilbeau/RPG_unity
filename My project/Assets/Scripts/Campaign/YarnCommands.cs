using UnityEngine;

/// <summary>
/// Stub Plan 6 — la commande Yarn <<save>> sera câblée en Plan 7 avec Yarn Spinner.
/// </summary>
public class YarnCommands : MonoBehaviour
{
    public void SaveGame()
    {
        GameSession.Instance?.Save();
        Debug.Log("[YarnCommands] Partie sauvegardée.");
    }
}
