using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int slideshowSceneIndex = 1; // Renamed for clarity
    [SerializeField] private float buttonClickDelay = 0.3f;
    [SerializeField] private float sceneTransitionFadeDuration = 0.5f; // New: for fade effect

    [Header("References")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject loadGameMenu;
    [SerializeField] private CanvasGroup fadeOverlay; // New: for fade effect

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("menu_theme");
        }
    }

    public void OnStartPressed()
    {
        AudioManager.Instance.PlaySFX("button_click");
        StartCoroutine(LoadSlideshowAfterDelay());
    }

    private IEnumerator LoadSlideshowAfterDelay()
    {
        yield return new WaitForSeconds(buttonClickDelay);

        // Optional: Fade out effect
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(1f)); // Fade to black
        }

        if (SceneManager.sceneCountInBuildSettings > slideshowSceneIndex)
        {
            SceneManager.LoadScene(slideshowSceneIndex);
        }
        else
        {
            Debug.LogError($"Scene index {slideshowSceneIndex} not found in build settings!");
            // Fallback - load next scene in order
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
        }
    }

    // New: Smooth fade effect coroutine
    private IEnumerator FadeScreen(float targetAlpha)
    {
        if (fadeOverlay == null) yield break;

        float startAlpha = fadeOverlay.alpha;
        float elapsed = 0f;

        while (elapsed < sceneTransitionFadeDuration)
        {
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / sceneTransitionFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }

    public void OnLoadPressed()
    {
        AudioManager.Instance.PlaySFX("button_click");
        loadGameMenu.SetActive(true);
    }

    public void OnOptionsPressed()
    {
        AudioManager.Instance.PlaySFX("button_click");
        optionsMenu.SetActive(true);
    }

    public void OnQuitPressed()
    {
        AudioManager.Instance.PlaySFX("button_click");
        StartCoroutine(QuitAfterDelay(buttonClickDelay));
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}