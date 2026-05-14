using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Button startButton;
    public Button quitButton;
    public CanvasGroup fadePanel;

    [Header("씬 이름")]
    public string gameSceneName = "GameScene";

    [Header("페이드 시간")]
    public float fadeDuration = 0.8f;

    void Start()
    {
        startButton.onClick.AddListener(OnClickStart);
        quitButton.onClick.AddListener(OnClickQuit);
        StartCoroutine(FadeIn());
    }

    void OnClickStart()
    {
        startButton.interactable = false;
        quitButton.interactable = false;
        StartCoroutine(FadeOutThenLoad(gameSceneName));
    }

    void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FadeIn()
    {
        fadePanel.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 0f;
    }

    IEnumerator FadeOutThenLoad(string sceneName)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = elapsed / fadeDuration;
            yield return null;
        }
        fadePanel.alpha = 1f;
        SceneManager.LoadScene(sceneName);
    }
}