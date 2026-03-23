using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attaché à un PNJ. Quand le joueur entre dans le trigger et appuie sur E :
/// - Si shopData est assigné : ouvre la boutique via ShopOpenedEvent
/// - Sinon : affiche le dialogue (stub Yarn Spinner)
/// </summary>
public class NpcInteractor : MonoBehaviour
{
    [SerializeField] private string dialogueNode = "Villageois";
    [SerializeField] private ShopSO shopData;

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
        if (!_playerNearby) return;
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (shopData != null)
            EventBus.Publish(new ShopOpenedEvent { Shop = shopData });
        else
            Debug.Log($"[NPC] Dialogue : {dialogueNode} (Yarn Spinner sera câblé en Plan 7)");
    }
}
