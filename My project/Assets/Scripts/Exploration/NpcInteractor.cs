using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attaché à un PNJ. Quand le joueur entre dans le trigger et appuie sur E,
/// affiche le dialogue dans la console (stub Plan 6 — Yarn Spinner câblé en Plan 7).
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private string dialogueNode = "Villageois";

    private bool _playerNearby;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _playerNearby = false;
    }

    private void Update()
    {
        if (_playerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Debug.Log($"[NPC] Dialogue : {dialogueNode} (Yarn Spinner sera câblé en Plan 7)");
    }
}
