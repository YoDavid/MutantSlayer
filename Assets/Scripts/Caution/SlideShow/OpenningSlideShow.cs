using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class OpenningSlideShow : MonoBehaviour
{
    [Header("Slide Content")]
    public Sprite[] images;
    public string[] texts;
    public Image imageComponent;
    public TMP_Text storyTextComponent;
    public string nextSceneName = "GameScene";

    [Header("Timing Settings")]
    public float fadeDuration = 0.5f;
    public float autoAdvanceDelay = 5f;
    public float spacePromptBlinkRate = 0.8f;

    [Header("Audio Settings")]
    public string slideshowMusic = "slideshow_music";
    public bool playTransitionSound = true;

    [Header("UI References")]
    public TMP_Text spacePromptText; // SPACE Key prompt
    public TMP_Text skipPromptText;  // ESC Key prompt (not blinking)

    private int currentIndex = 0;
    private bool isFading = false;
    private Coroutine autoAdvanceCoroutine;
    private Coroutine blinkCoroutine;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("slideshow_theme");
        }

        // Initialize first slide
        if (images.Length > 0 && texts.Length > 0)
        {
            ShowSlide(0);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance());
            blinkCoroutine = StartCoroutine(BlinkSpacePrompt());
        }
        else
        {
            Debug.LogError("No slides configured!");
        }
    }

    void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }

    private void Update()
    {
        if (isFading) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceSlide();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipSlideshow();
        }
    }

    private void AdvanceSlide()
    {
        currentIndex++;

        if (currentIndex >= images.Length || currentIndex >= texts.Length)
        {
            LoadNextScene();
        }
        else
        {
            if (playTransitionSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlideTransition();
            }

            StartCoroutine(FadeAndChangeSlide());

            // Restart auto-advance if not at the end
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance());
        }
    }

    private void ShowSlide(int index)
    {
        imageComponent.sprite = images[index];
        storyTextComponent.text = texts[index];

        var color = imageComponent.color;
        color.a = 1f;
        imageComponent.color = color;

        var textColor = storyTextComponent.color;
        textColor.a = 1f;
        storyTextComponent.color = textColor;
    }

    private void SkipSlideshow()
    {
        if (autoAdvanceCoroutine != null)
            StopCoroutine(autoAdvanceCoroutine);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneLoader.Instance.LoadSceneWithFade(nextSceneName);
        }
    }

    private IEnumerator FadeAndChangeSlide()
    {
        isFading = true;

        yield return StartCoroutine(FadeCurrentSlide(1f, 0f)); // Fade out
        ShowSlide(currentIndex);
        yield return StartCoroutine(FadeCurrentSlide(0f, 1f)); // Fade in

        isFading = false;
    }

    private IEnumerator FadeCurrentSlide(float from, float to)
    {
        float elapsed = 0f;

        Color imageColor = imageComponent.color;
        Color textColor = storyTextComponent.color;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            imageColor.a = Mathf.Lerp(from, to, t);
            textColor.a = Mathf.Lerp(from, to, t);

            imageComponent.color = imageColor;
            storyTextComponent.color = textColor;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        imageColor.a = to;
        textColor.a = to;
        imageComponent.color = imageColor;
        storyTextComponent.color = textColor;
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        if (!isFading)
        {
            AdvanceSlide();
        }
    }

    private IEnumerator BlinkSpacePrompt()
    {
        while (true)
        {
            if (spacePromptText != null)
            {
                spacePromptText.enabled = !spacePromptText.enabled;
            }
            yield return new WaitForSeconds(spacePromptBlinkRate);
        }
    }
}
