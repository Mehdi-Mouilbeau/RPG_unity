using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Coffre. Quand le joueur appuie sur E à proximité, distribue le loot
/// et persiste l'état "ouvert" dans ProgressionFlags.
/// </summary>
public class ChestInteractor : MonoBehaviour
{
    [SerializeField] private string        chestId;
    [SerializeField] private LootTableSO   lootTable;
    [SerializeField] private GameObject    openVisual;
    [SerializeField] private GameObject    closedVisual;

    private bool _playerNearby;

    private void Start()
    {
        bool alreadyOpened = GameSession.Instance != null
            && GameSession.Instance.Flags.IsSet(chestId);
        SetVisual(alreadyOpened);
    }

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
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        var session = GameSession.Instance;
        if (session == null || session.Flags.IsSet(chestId)) return;

        var item = lootTable?.Roll();
        if (item != null && session.ActiveCharacter != null)
        {
            session.ActiveCharacter.Inventory.Equip(item);
            Debug.Log($"Coffre : obtenu {item.itemName}");
        }

        session.Flags.Set(chestId);
        SetVisual(true);
    }

    private void SetVisual(bool opened)
    {
        if (openVisual)   openVisual.SetActive(opened);
        if (closedVisual) closedVisual.SetActive(!opened);
    }
}
