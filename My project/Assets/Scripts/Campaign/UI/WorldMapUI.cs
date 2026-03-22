using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldMapUI : MonoBehaviour
{
    [System.Serializable]
    public struct ZoneButton
    {
        public Button         button;
        public TMP_Text       label;
        public CampaignZoneSO zone;
        public string         requiredFlag;
        public string         completedFlag;
    }

    [SerializeField] private ZoneButton[] zones;

    private void Start()
    {
        var session = GameSession.Instance;
        if (session == null) return;

        foreach (var z in zones)
        {
            var zone = z;
            if (z.zone == null) continue;

            bool locked    = !string.IsNullOrEmpty(z.requiredFlag)
                             && !session.Flags.IsSet(z.requiredFlag);
            bool completed = !string.IsNullOrEmpty(z.completedFlag)
                             && session.Flags.IsSet(z.completedFlag);

            if (z.button != null) z.button.interactable = !locked;
            if (z.label != null)
            {
                string suffix = locked ? " [Verrouillé]" : (completed ? " [Complété]" : "");
                z.label.text = z.zone.zoneName + suffix;
            }

            if (!locked && z.button != null)
                z.button.onClick.AddListener(() => EnterZone(zone.zone));
        }
    }

    private void EnterZone(CampaignZoneSO zone)
    {
        EventBus.Publish(new ZoneEnteredEvent { Zone = zone });
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(zone.sceneKey);
    }
}
