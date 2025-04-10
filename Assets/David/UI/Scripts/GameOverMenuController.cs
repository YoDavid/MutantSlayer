using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverMenuController : BaseMenuController
{
    [Header("Death Sequence Settings")]
    [SerializeField] private float timeSlowDuration = 1.5f; // New: Duration for time slowdown
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float delayBeforeTextAnimation = 0.5f;
    [SerializeField] private float textFadeInDuration = 1f;
    [SerializeField] private float textScaleDuration = 1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float delayAfterTextAnimation = 0.5f;
    [SerializeField] private float panelFadeDuration = 1f;

    [Header("UI References")]
    [SerializeField] private CanvasGroup youDiedTextCanvasGroup;
    [SerializeField] private RectTransform youDiedTextTransform;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private CanvasGroup fadeInPanel;

    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start();
        ResetMenuState();
    }

    private void ResetMenuState()
    {
        if (youDiedTextCanvasGroup != null) youDiedTextCanvasGroup.alpha = 0f;
        if (buttonsParent != null) buttonsParent.SetActive(false);
        if (fadeInPanel != null) fadeInPanel.alpha = 0f;
    }

    public void StartGameOverSequence()
    {
        // Don't pause immediately - start slowdown coroutine
        StartCoroutine(GradualTimeSlowdown());
    }

    private IEnumerator GradualTimeSlowdown()
    {
        float elapsed = 0f;
        while (elapsed < timeSlowDuration)
        {
            // Gradually reduce timescale from 1 to 0
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed / timeSlowDuration);
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Adjust fixedDeltaTime accordingly
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure timescale is exactly 0 when done
        Time.timeScale = 0f;

        // Start the UI sequence
        ResetMenuState();
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Make sure the fadeInPanel is active
        if (fadeInPanel != null)
        {
            fadeInPanel.gameObject.SetActive(true); // Make it active before fading
            yield return StartCoroutine(FadePanel(0f, 1f, panelFadeDuration));
        }

        yield return new WaitForSecondsRealtime(delayBeforeTextAnimation);

        // Animate "You Died" text
        if (youDiedTextCanvasGroup != null && youDiedTextTransform != null)
        {
            yield return StartCoroutine(AnimateYouDiedText());
        }

        yield return new WaitForSecondsRealtime(delayAfterTextAnimation);

        // Show buttons
        if (buttonsParent != null)
        {
            buttonsParent.SetActive(true);
        }

        SetVisible(true);
    }


    private IEnumerator FadePanel(float startAlpha, float endAlpha, float duration)
    {
        fadeInPanel.alpha = startAlpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            fadeInPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeInPanel.alpha = endAlpha;
    }

    private IEnumerator AnimateYouDiedText()
    {
        youDiedTextCanvasGroup.alpha = 0f;
        float elapsed = 0f;

        // Fade in
        while (elapsed < textFadeInDuration)
        {
            youDiedTextCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        youDiedTextCanvasGroup.alpha = 1f;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        if (visible) SelectButton(0);
    }

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // Reset fixedDeltaTime
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; // Reset fixedDeltaTime
        SceneLoader.Instance.LoadSceneWithFade("Scene_MainMenu");
    }
}