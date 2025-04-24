using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float minLoadTime = 1f;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isTransitioningScene = false; 

    private void Awake()
    {
        // Singleton pattern with proper persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize fade overlay
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0;
                fadeOverlay.gameObject.SetActive(false);
            }

            // Initialize loading screen
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioningScene) return;

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isTransitioningScene = true;  // Mark that a transition is in progress

        // Fade out
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0, 1, fadeDuration));
        }

        // Load scene
        yield return StartCoroutine(LoadSceneAsync(sceneName));

        // Fade in (optional)
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(Fade(1, 0, fadeDuration));
            fadeOverlay.gameObject.SetActive(false);
        }

        isTransitioningScene = false; 
    }

    public IEnumerator FadeWithOverlay(float from, float to, float duration)
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(from, to, duration));

        // Only deactivate overlay if we're fading out (to transparent)
        if (to == 0f)
            fadeOverlay.gameObject.SetActive(false);
    }

    public IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        fadeOverlay.alpha = from;

        while (elapsed < duration)
        {
            fadeOverlay.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeOverlay.alpha = to;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        foreach (var menu in FindObjectsOfType<BaseMenuController>())
        {
            menu.gameObject.SetActive(false);
        }

        loadingScreen.SetActive(true);
        float elapsedTime = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            elapsedTime += Time.unscaledDeltaTime;

            if (operation.progress >= 0.9f && elapsedTime >= minLoadTime)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        loadingScreen.SetActive(false);
    }

    private bool DoesSceneExist(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(scenePath) == sceneName)
                return true;
        }
        return false;
    }
}
