using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class OpenningSlideShow : MonoBehaviour
{
    [Header("Slide Content")]
    public Sprite[] images;
    public string[] texts;
    public Image imageComponent;
    public Text textComponent;
    public string nextSceneName = "GameScene"; // Default fallback

    [Header("Timing Settings")]
    public float fadeDuration = 0.5f;
    public float autoAdvanceDelay = 5f;
    public float spacePromptBlinkRate = 0.8f;

    [Header("Audio Settings")]
    public string slideshowMusic = "slideshow_music";
    public bool playTransitionSound = true;

    [Header("UI References")]
    public GameObject spacePrompt;
    public GameObject skipPrompt;

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
        // Stop music when leaving slideshow (optional)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }

    private void Update()
    {
        // Space advances slides
        if (Input.GetKeyDown(KeyCode.Space) && !isFading)
        {
            AdvanceSlide();
        }

        // Escape skips entire slideshow
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
            // Play transition sound if enabled
            if (playTransitionSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlideTransition();
            }

            StartCoroutine(FadeAndChangeSlide());

            // Reset auto-advance timer
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance());
        }
    }

    private void ShowSlide(int index)
    {
        imageComponent.sprite = images[index];
        textComponent.text = texts[index];

        // Reset alpha in case coming from fade
        var color = imageComponent.color;
        color.a = 1f;
        imageComponent.color = color;
        textComponent.color = color;
    }

    private void SkipSlideshow()
    {
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
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name not specified!");
        }
    }

    private IEnumerator FadeAndChangeSlide()
    {
        isFading = true;

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f));

        // Change content
        ShowSlide(currentIndex);

        // Fade in
        yield return StartCoroutine(Fade(0f, 1f));

        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color imageColor = imageComponent.color;
        Color textColor = textComponent.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);

            imageColor.a = alpha;
            textColor.a = alpha;
            imageComponent.color = imageColor;
            textComponent.color = textColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final alpha is set
        imageColor.a = endAlpha;
        textColor.a = endAlpha;
        imageComponent.color = imageColor;
        textComponent.color = textColor;
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
            if (spacePrompt != null)
            {
                spacePrompt.SetActive(!spacePrompt.activeSelf);
            }
            yield return new WaitForSeconds(spacePromptBlinkRate);
        }
    }
}