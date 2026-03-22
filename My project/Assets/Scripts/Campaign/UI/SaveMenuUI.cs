using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button     btnSave;
    [SerializeField] private Button     btnClose;
    [SerializeField] private TMP_Text   statusText;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (btnSave != null) btnSave.onClick.AddListener(OnSave);
        if (btnClose != null) btnClose.onClick.AddListener(OnClose);
    }

    private void Update()
    {
        if (panel != null && Input.GetKeyDown(KeyCode.Escape))
            panel.SetActive(!panel.activeSelf);
    }

    private void OnSave()
    {
        GameSession.Instance?.Save();
        if (statusText) statusText.text = "Partie sauvegardée !";
    }

    private void OnClose()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Show() { if (panel != null) panel.SetActive(true); }
}
