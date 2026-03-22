using UnityEngine;
using Yarn.Unity;

public class YarnCommands : MonoBehaviour
{
    [YarnCommand("save")]
    public void SaveGame()
    {
        GameSession.Instance?.Save();
        Debug.Log("[Yarn] Partie sauvegardée.");
    }
}
