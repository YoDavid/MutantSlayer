using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEditor;

public class OpenningSlideShow : MonoBehaviour
{
    [System.Serializable]
    public class SlideGroup
    {
        public string name;
        public Sprite[] images;
        [TextArea] public string text;
        public bool isSlashSequence = false;
        public float slideDuration = 1f;
        public float slashSpeed = 0.1f;
        public float whiteFlashDuration = 0.05f;

        [Header("Text Positioning")]
        public Vector2 textPosition;
        public Vector2 textSize = new Vector2(500, 200);
        public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        public bool overrideTextStyle = false;
        public float fontSize = 0f;

    }

    [Header("Slide Groups")]
    public SlideGroup[] slideGroups;

    [Header("UI References")]
    public Image imageComponent;
    public TMP_Text storyTextComponent;
    public Image whiteFlashPanel;
    public TMP_Text spacePromptText;
    public TMP_Text skipPromptText;
    public string nextSceneName = "Scene_Game";
    [SerializeField] private GameObject textPanel;

    [Header("Timing Settings")]
    public float fadeDuration = 0.5f;
    public float spacePromptBlinkRate = 0.8f;
    private bool isShowingAllText = false;

    private int currentGroupIndex = 0;
    private int currentSlideInGroup = 0;
    private bool isFading = false;
    private bool isPlayingSlashSequence = false;
    private Coroutine autoAdvanceCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine slashSequenceCoroutine;
    private Coroutine textAnimationCoroutine;

    [Header("Fade Settings")]
    public Image blackFadePanel; // Assign a full-screen black UI Image
    public float fadeInDuration = 3f; // Configurable fade time
    private bool isFadingIn = true;

    
    [Header("Debug Settings")]
    [SerializeField] private bool overrideAllSlideDurations = false;
    [SerializeField] private float debugSlideDuration = 2f; // Default fast duration for testing

    [Header("Cover Panel Settings")]
    [SerializeField] private Image CoverPanelForImages;
    [SerializeField] private float CoverPanelForImagesFadeDuration = 10f;
    [SerializeField] private float CoverPanelFadeDelay = 2f;
    private Coroutine coverPanelFadeCoroutine; // Track the fade coroutine

    private float typingSoundVolume = 1f;
    private float typingSoundFadeDuration = 15f;
    private float timeSinceTypingStarted = 0f;

    private float groupStartTime;
    private float textFinishTime;
    private bool isTrackingGroupTime = false;

    private void Start()
    {
        Cursor.visible = false;
        // Initialize fade panel
        blackFadePanel.gameObject.SetActive(true);
        blackFadePanel.color = Color.black;

        // Start fade-in coroutine
        StartCoroutine(FadeFromBlack());
        StartCoroutine(FadeImagesCoverPanel());

        // Rest of your initialization...
        AudioManager.Instance.PlayMusic("slideshow_theme");
        whiteFlashPanel.gameObject.SetActive(false);

        if (slideGroups.Length > 0)
        {
            ShowCurrentSlide();
            blinkCoroutine = StartCoroutine(BlinkSpacePrompt());
        }

        coverPanelFadeCoroutine = StartCoroutine(FadeImagesCoverPanel());
    }

    private IEnumerator FadeImagesCoverPanel()
    {
        // Wait for the delay period before starting
        if (CoverPanelFadeDelay > 0)
        {
            yield return new WaitForSeconds(CoverPanelFadeDelay);
        }

        // Set initial alpha to 100% (fully opaque)
        Color startColor = CoverPanelForImages.color;
        startColor.a = 1f;
        CoverPanelForImages.color = startColor;

        float elapsedTime = 0f;

        while (elapsedTime < CoverPanelForImagesFadeDuration)
        {
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / CoverPanelForImagesFadeDuration);

            // Apply new alpha
            Color newColor = CoverPanelForImages.color;
            newColor.a = newAlpha;
            CoverPanelForImages.color = newColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final alpha is exactly 0.5
        Color finalColor = CoverPanelForImages.color;
        finalColor.a = 0f;
        CoverPanelForImages.color = finalColor;
    }

    private bool ShouldShowTextPanel()
    {
        if (currentGroupIndex >= slideGroups.Length) return false;

        string groupName = slideGroups[currentGroupIndex].name;
        return !(groupName.Contains("Part 3") &&
               !groupName.Contains("Slashes") &&
               !slideGroups[currentGroupIndex].isSlashSequence);
    }

    private IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        Color color = blackFadePanel.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            blackFadePanel.color = color;
            yield return null;
        }

        color.a = 0;
        blackFadePanel.color = color;
        isFadingIn = false;

        blackFadePanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
    }

    private void Update()
    {
        if (isFadingIn || isFading || isPlayingSlashSequence) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isShowingAllText)
            {
                // First press: show all text immediately
                ShowAllTextInstantly();
                isShowingAllText = true;
            }
            else
            {
                // Second press: advance to next slide
                AdvanceSlide();
                isShowingAllText = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipSlideshow();
        }
    }

    private void ShowAllTextInstantly()
    {
        // Stop any running text animation
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }

        // Show complete text immediately
        var currentGroup = slideGroups[currentGroupIndex];
        storyTextComponent.text = currentGroup.text;
        storyTextComponent.color = new Color(storyTextComponent.color.r, storyTextComponent.color.g, storyTextComponent.color.b, 1f);

        // Immediately set cover panel to transparent
        if (coverPanelFadeCoroutine != null)
        {
            StopCoroutine(coverPanelFadeCoroutine);
        }
        Color transparentColor = CoverPanelForImages.color;
        transparentColor.a = 0f;
        CoverPanelForImages.color = transparentColor;
    }

    private void ApplyTextStyle(SlideGroup group)
    {
        if (!group.overrideTextStyle) return;

        if (group.fontSize > 0)
        {
            storyTextComponent.fontSize = group.fontSize;
        }

        RectTransform textRT = storyTextComponent.GetComponent<RectTransform>();

        textRT.anchoredPosition = group.textPosition;

        textRT.sizeDelta = group.textSize;

        storyTextComponent.alignment = group.alignment;
    }

    private float GetAdjustedSlideDuration(SlideGroup group)
    {
        // Use debug duration if override is enabled, otherwise use group's duration
        return overrideAllSlideDurations ? debugSlideDuration : group.slideDuration;
    }

    private void ShowCurrentSlide()
    {
        var currentGroup = slideGroups[currentGroupIndex];
        float currentSlideDuration = GetAdjustedSlideDuration(currentGroup);

        // Handle TextPanel visibility
        if (textPanel != null)
        {
            textPanel.SetActive(ShouldShowTextPanel());
        }

        // Rest of your existing ShowCurrentSlide code...
        // Debug when a new group starts
        if (currentSlideInGroup == 0)
        {
            groupStartTime = Time.time;
            isTrackingGroupTime = true;
            float totalDuration = currentGroup.images.Length * currentSlideDuration;
            Debug.Log($"Starting group {currentGroupIndex} ('{currentGroup.name}') at {groupStartTime:F2}s. " +
                     $"{currentGroup.images.Length} images ª {currentSlideDuration}s = {totalDuration}s total");
        }

        isShowingAllText = false;
        imageComponent.sprite = currentGroup.images[currentSlideInGroup];

        // Only animate text on first slide of multi-image groups or all single-image slides
        if (currentSlideInGroup == 0 || currentGroup.images.Length == 1)
        {
            if (textAnimationCoroutine != null)
            {
                StopCoroutine(textAnimationCoroutine);
            }

            float totalGroupDuration = currentGroup.images.Length * currentSlideDuration;
            float textDuration = Mathf.Max(0.1f, totalGroupDuration - 3f); // Always finish 3s before group ends

            Debug.Log($"Starting text animation (duration: {textDuration:F2}s)");
            textAnimationCoroutine = StartCoroutine(AnimateText(currentGroup.text, textDuration));
            ApplyTextStyle(currentGroup);
        }
        else
        {
            storyTextComponent.color = new Color(storyTextComponent.color.r,
                                               storyTextComponent.color.g,
                                               storyTextComponent.color.b, 1f);
        }

        if (!currentGroup.isSlashSequence && autoAdvanceCoroutine == null)
        {
            // For multi-image groups, each slide gets slideDuration seconds
            float slideDisplayTime = currentSlideDuration;
            autoAdvanceCoroutine = StartCoroutine(AutoAdvance(slideDisplayTime));
        }
    }

    private void AdvanceSlide()
    {
        if (isPlayingSlashSequence) return;

        var currentGroup = slideGroups[currentGroupIndex];
        isShowingAllText = false;

        // Debug group completion
        if (currentSlideInGroup == currentGroup.images.Length - 1 && isTrackingGroupTime)
        {
            float groupEndTime = Time.time;
            float actualDuration = groupEndTime - groupStartTime;
            float expectedDuration = currentGroup.images.Length * currentGroup.slideDuration;

            Debug.Log($"Group {currentGroupIndex} completed at {groupEndTime:F2}s\n" +
                     $"Actual duration: {actualDuration:F2}s | Expected: {expectedDuration:F2}s");
            isTrackingGroupTime = false;
        }

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        currentSlideInGroup++;

        // Check if we're moving to a new group
        bool isChangingGroups = currentSlideInGroup >= currentGroup.images.Length;

        if (isChangingGroups)
        {
            currentGroupIndex++;
            currentSlideInGroup = 0;

            // Check if we've reached the end of all groups
            if (currentGroupIndex >= slideGroups.Length)
            {
                LoadNextScene();
                return;
            }

            // Only play transition sound when changing groups AND new group isn't slash sequence
            if (!slideGroups[currentGroupIndex].isSlashSequence && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlideTransition();
            }
        }

        // Only proceed if we haven't reached the end
        if (currentGroupIndex < slideGroups.Length)
        {
            if (slideGroups[currentGroupIndex].isSlashSequence)
            {
                StartSlashSequence();
            }
            else
            {
                StartCoroutine(FadeAndChangeSlide());
            }
        }
    }


    private void StartSlashSequence()
    {
        isPlayingSlashSequence = true;
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }
        slashSequenceCoroutine = StartCoroutine(PlaySlashSequence());
    }

    private IEnumerator PlaySlashSequence()
    {
        var currentGroup = slideGroups[currentGroupIndex];

        // Hide text during slash sequence
        storyTextComponent.text = "";

        for (int i = 0; i < currentGroup.images.Length; i++)
        {
            // Show current slash image
            imageComponent.sprite = currentGroup.images[i];

            // Play slash sound
            switch (i % 3)
            {
                case 0: AudioManager.Instance.PlaySwing_00(); break;
                case 1: AudioManager.Instance.PlaySwing_01(); break;
                case 2: AudioManager.Instance.PlaySwing_02(); break;
            }

            // White flash effect
            yield return StartCoroutine(WhiteFlashEffect(currentGroup.whiteFlashDuration));

            // Wait before next slash
            yield return new WaitForSeconds(currentGroup.slashSpeed);
        }

        // End of slash sequence
        isPlayingSlashSequence = false;
        currentGroupIndex++;
        currentSlideInGroup = 0;

        if (currentGroupIndex >= slideGroups.Length)
        {
            LoadNextScene();
        }
        else
        {
            StartCoroutine(FadeAndChangeSlide());
        }
    }

    private IEnumerator WhiteFlashEffect(float duration)
    {
        whiteFlashPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        whiteFlashPanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeAndChangeSlide()
    {
        isFading = true;

        yield return StartCoroutine(FadeCurrentSlide(1f, 0f)); // Fade out
        ShowCurrentSlide(); // This will handle TextPanel visibility
        yield return StartCoroutine(FadeCurrentSlide(0f, 1f)); // Fade in

        isFading = false;
    }

    private IEnumerator AutoAdvance(float delay)
    {
        float adjustedDelay = overrideAllSlideDurations ? debugSlideDuration : delay;
        float startTime = Time.time;
        yield return new WaitForSeconds(adjustedDelay);

        if (!isFading && !isPlayingSlashSequence)
        {
            Debug.Log($"Advancing after {Time.time - startTime:F2}s (expected: {adjustedDelay:F2}s)");
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

    private void SkipSlideshow()
    {
        if (slashSequenceCoroutine != null)
            StopCoroutine(slashSequenceCoroutine);
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

    private IEnumerator FadeCurrentSlide(float from, float to)
    {
        float elapsed = 0f;

        Color imageColor = imageComponent.color;
        Color textColor = storyTextComponent.color;

        // Only fade text if it's a single-image group or if we're fading in the first slide of a multi-image group
        bool shouldFadeText = slideGroups[currentGroupIndex].images.Length <= 1 ||
                             (currentSlideInGroup == 0 && to == 1f);

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            imageColor.a = Mathf.Lerp(from, to, t);
            if (shouldFadeText)
                textColor.a = Mathf.Lerp(from, to, t);

            imageComponent.color = imageColor;
            if (shouldFadeText)
                storyTextComponent.color = textColor;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        imageColor.a = to;
        if (shouldFadeText)
            textColor.a = to;

        imageComponent.color = imageColor;
        if (shouldFadeText)
            storyTextComponent.color = textColor;
    }

    private IEnumerator AnimateText(string fullText, float duration)
    {
        storyTextComponent.text = "";
        int totalLetters = fullText.Length;
        float delayPerChar = totalLetters > 0 ? duration / totalLetters : 0f;

        typingSoundVolume = 1f;
        timeSinceTypingStarted = 0f;
        StartCoroutine(FadeOutTypingSound());

        for (int i = 0; i <= totalLetters; i++)
        {
            storyTextComponent.text = fullText.Substring(0, i);

            if (i > 0 && fullText[i - 1] != ' ')
            {
                timeSinceTypingStarted += delayPerChar;
                AudioManager.Instance.PlayKeyboardSound(typingSoundVolume);
            }

            if (delayPerChar > 0) yield return new WaitForSeconds(delayPerChar);
        }

        textFinishTime = Time.time;
        Debug.Log($"Text completed at {textFinishTime:F2}s " +
                 $"(took {textFinishTime - groupStartTime:F2}s of expected {duration:F2}s)");

        storyTextComponent.color = new Color(storyTextComponent.color.r,
                                           storyTextComponent.color.g,
                                           storyTextComponent.color.b, 1f);
    }

    
    private IEnumerator FadeOutTypingSound()
    {
        float elapsed = 0f;
        while (elapsed < typingSoundFadeDuration)
        {
            elapsed += Time.deltaTime;
            typingSoundVolume = Mathf.Clamp01(1 - (elapsed / typingSoundFadeDuration));
            yield return null;
        }
        typingSoundVolume = 0f;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
        UpdateTextLive();
#endif
    }

    private void UpdateTextLive()
    {
#if UNITY_EDITOR
        if (storyTextComponent == null || slideGroups == null ||
            currentGroupIndex < 0 || currentGroupIndex >= slideGroups.Length)
        {
            return;
        }

        var currentGroup = slideGroups[currentGroupIndex];

        // Always update text content
        storyTextComponent.text = currentGroup.text;

        // Only apply styling if enabled
        if (currentGroup.overrideTextStyle)
        {
            storyTextComponent.fontSize = currentGroup.fontSize > 0 ?
                currentGroup.fontSize : storyTextComponent.fontSize;

            RectTransform textRT = storyTextComponent.GetComponent<RectTransform>();
            textRT.anchoredPosition = currentGroup.textPosition;
            textRT.sizeDelta = currentGroup.textSize;
            storyTextComponent.alignment = currentGroup.alignment;
        }

        // Force UI update
        EditorApplication.QueuePlayerLoopUpdate();
#endif
    }
}