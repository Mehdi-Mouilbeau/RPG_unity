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
        btnLoad.interactable = hasSave;
        if (btnStartLabel != null)
            btnStartLabel.text = "Nouvelle Partie";

        btnStart.onClick.AddListener(OnNewGame);
        btnLoad.onClick.AddListener(OnContinue);
        btnArena.onClick.AddListener(OnArena);
        btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnNewGame()
    {
        SaveSystem.Delete();
        SceneLoader.Instance.LoadScene("CharacterSelect");
    }

    private void OnContinue()
    {
        GameSession.Instance.Load();
        SceneLoader.Instance.LoadScene("WorldMap");
    }

    private void OnArena()
    {
        SceneLoader.Instance.LoadScene("Arena");
    }

    private void OnQuit()
    {
        Application.Quit();
    }
}
