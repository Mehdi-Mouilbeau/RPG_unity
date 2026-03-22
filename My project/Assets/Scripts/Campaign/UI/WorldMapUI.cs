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
        foreach (var z in zones)
        {
            var zone = z;
            bool locked    = !string.IsNullOrEmpty(z.requiredFlag)
                             && !GameSession.Instance.Flags.IsSet(z.requiredFlag);
            bool completed = !string.IsNullOrEmpty(z.completedFlag)
                             && GameSession.Instance.Flags.IsSet(z.completedFlag);

            z.button.interactable = !locked;
            if (z.label != null)
            {
                string suffix = locked ? " [Verrouillé]" : (completed ? " [Complété]" : "");
                z.label.text = z.zone.zoneName + suffix;
            }

            if (!locked)
                z.button.onClick.AddListener(() => EnterZone(zone.zone));
        }
    }

    private void EnterZone(CampaignZoneSO zone)
    {
        EventBus.Publish(new ZoneEnteredEvent { Zone = zone });
        SceneLoader.Instance.LoadScene(zone.sceneKey);
    }
}
