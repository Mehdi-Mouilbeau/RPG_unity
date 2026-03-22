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
        panel.SetActive(false);
        btnSave.onClick.AddListener(OnSave);
        btnClose.onClick.AddListener(OnClose);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            panel.SetActive(!panel.activeSelf);
    }

    private void OnSave()
    {
        GameSession.Instance.Save();
        if (statusText) statusText.text = "Partie sauvegardée !";
    }

    private void OnClose()
    {
        panel.SetActive(false);
    }

    public void Show() => panel.SetActive(true);
}
