using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button     btnStart;
    [SerializeField] private Button     btnLoad;
    [SerializeField] private Button     btnArena;
    [SerializeField] private Button     btnQuit;
    [SerializeField] private TMP_Text   btnStartLabel;

    private void Start()
    {
        bool hasSave = SaveSystem.HasSave();
        if (btnLoad != null) btnLoad.interactable = hasSave;
        if (btnStartLabel != null) btnStartLabel.text = "Nouvelle Partie";
        if (btnStart != null) btnStart.onClick.AddListener(OnNewGame);
        if (btnLoad != null) btnLoad.onClick.AddListener(OnContinue);
        if (btnArena != null) btnArena.onClick.AddListener(OnArena);
        if (btnQuit != null) btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnNewGame()
    {
        SaveSystem.Delete();
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("CharacterSelect");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect");
    }

    private void OnContinue()
    {
        if (GameSession.Instance != null) GameSession.Instance.Load();
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("WorldMap");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
    }

    private void OnArena()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("Arena");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("Arena");
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
