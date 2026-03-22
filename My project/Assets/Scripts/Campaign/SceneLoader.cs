using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private Image fadeImage; // Image UI noire couvrant tout l'écran
    [SerializeField] private float fadeDuration = 0.4f;

    private bool _isLoading;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (fadeImage == null) Debug.LogError("[SceneLoader] fadeImage non assigné dans l'Inspector!");
    }

    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        _isLoading = true;
        // Fondu au noir
        yield return StartCoroutine(Fade(0f, 1f));
        yield return SceneManager.LoadSceneAsync(sceneName);
        // Fondu depuis le noir
        yield return StartCoroutine(Fade(1f, 0f));
        _isLoading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;
    }
}
