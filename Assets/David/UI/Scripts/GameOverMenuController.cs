using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverMenuController : BaseMenuController
{
    [Header("Death Sequence Settings")]
    [SerializeField] private float timeSlowDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float delayBeforeTextAnimation = 0.5f;
    [SerializeField] private float textFadeInDuration = 1f;
    [SerializeField] private float textScaleDuration = 1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float delayAfterTextAnimation = 0.5f;
    [SerializeField] private float panelFadeDuration = 1f;
    [SerializeField] private float buttonFadeDuration = 1f;

    [Header("UI References")]
    [SerializeField] private CanvasGroup youDiedTextCanvasGroup;
    [SerializeField] private RectTransform youDiedTextTransform;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private CanvasGroup fadeInPanel;

    private PlayerHealth playerHealth;

    public bool IsVisible { get; private set; }


    protected override void Start()
    {
        base.Start();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        ResetMenuState();
    }

    private void ResetMenuState()
    {
        if (youDiedTextCanvasGroup != null) youDiedTextCanvasGroup.alpha = 0f;
        if (buttonsParent != null) buttonsParent.SetActive(false);
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;
        }
        if (fadeInPanel != null) fadeInPanel.alpha = 0f;
    }

    public void StartGameOverSequence()
    {
        StartCoroutine(GradualTimeSlowdown());
    }

    private IEnumerator GradualTimeSlowdown()
    {
        float elapsed = 0f;
        while (elapsed < timeSlowDuration)
        {
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed / timeSlowDuration);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 0f;

        ResetMenuState();
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        if (fadeInPanel != null)
        {
            fadeInPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(fadeInPanel, 0f, 1f, panelFadeDuration));
        }

        yield return new WaitForSecondsRealtime(delayBeforeTextAnimation);

        if (youDiedTextCanvasGroup != null && youDiedTextTransform != null)
        {
            yield return StartCoroutine(AnimateYouDiedText());
        }

        yield return new WaitForSecondsRealtime(delayAfterTextAnimation);

        if (buttonsParent != null && buttonsCanvasGroup != null)
        {
            buttonsParent.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(buttonsCanvasGroup, 0f, 1f, buttonFadeDuration));
            buttonsCanvasGroup.interactable = true;
            buttonsCanvasGroup.blocksRaycasts = true;
        }

        SetVisible(true);
        canUseButtons = true;
    }

    private IEnumerator AnimateYouDiedText()
    {
        youDiedTextCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        AudioManager.Instance.PlayDeathScreen();

        while (elapsed < textFadeInDuration)
        {
            youDiedTextCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        youDiedTextCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        group.alpha = startAlpha;

        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        group.alpha = endAlpha;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;

        if (visible)
        {
            SelectButton(0);
            FindObjectOfType<ForceMouseHidden>()?.UnlockAndShowMouse();
        }
        else
        {
            FindObjectOfType<ForceMouseHidden>()?.LockAndHideMouse();
        }
    }


    public void OnRestartAtCheckpointPressed()
    {
        if (CheckpointManager.Instance.HasCheckpoint())
        {
            SceneLoader.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneLoader.Instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
    }

    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        SceneLoader.Instance.LoadSceneWithFade("Scene_MainMenu");
    }


}
