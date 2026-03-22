using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/// <summary>
/// Attaché à un PNJ. Quand le joueur entre dans le trigger et appuie sur E,
/// démarre le dialogue Yarn Spinner correspondant.
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private string        dialogueNode = "Villageois";
    [SerializeField] private DialogueRunner dialogueRunner;

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
        if (_playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogueRunner == null) return;
            if (!dialogueRunner.IsDialogueRunning)
                dialogueRunner.StartDialogue(dialogueNode);
        }
    }
}
