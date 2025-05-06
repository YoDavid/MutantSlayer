using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class TutorialSystemController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text navigationPrompt;

    [Header("Settings")]
    [SerializeField] private float blinkSpeed = 1f;

    private List<TutorialContent> tutorialPages;
    private int currentPageIndex = 0;
    private float blinkTimer = 0f;
    private bool promptVisible = true;
    public bool IsShowing { get; private set; }

    private void Start()
    {
        // Force the default position on start
        if (tutorialImage != null)
        {
            RectTransform imageRect = tutorialImage.GetComponent<RectTransform>();
            if (imageRect != null)
            {
                imageRect.anchoredPosition = new Vector2(-650f, 100f);
                Debug.Log($"Start(): Set image to {imageRect.anchoredPosition}");
            }
        }
    }

    private void Update()
    {
        if (!IsShowing) return;

        HandleBlinkingPrompt();
        HandleNavigationInput();
    }

    public void ShowTutorial(List<TutorialContent> pages)
    {
        tutorialPages = pages;
        currentPageIndex = 0;
        UpdateContent();
        gameObject.SetActive(true);
        IsShowing = true;
    }

    public void HideTutorial()
    {
        gameObject.SetActive(false);
        IsShowing = false;
    }

    private void HandleBlinkingPrompt()
    {
        if (navigationPrompt == null) return;

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer >= blinkSpeed)
        {
            blinkTimer = 0f;
            promptVisible = !promptVisible;
            navigationPrompt.gameObject.SetActive(promptVisible);
        }
    }

    private void HandleNavigationInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click - Next
        {
            if (currentPageIndex < tutorialPages.Count - 1)
            {
                currentPageIndex++;
                UpdateContent();
            }
            else
            {
                UIManager.Instance.HideTutorial();
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right click - Previous
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdateContent();
            }
        }
    }

    private void UpdateContent()
    {
        var currentContent = tutorialPages[currentPageIndex];

        if (tutorialImage != null)
        {
            tutorialImage.sprite = currentContent.image;

            RectTransform imageRect = tutorialImage.GetComponent<RectTransform>();
            if (imageRect != null)
            {
                // Handle Position
                if (currentContent.overridePosition)
                {
                    imageRect.anchoredPosition = currentContent.customImagePosition;
                    Debug.Log($"Position overridden to {imageRect.anchoredPosition}");
                }
                else
                {
                    imageRect.anchoredPosition = new Vector2(-650f, 100f);
                }

                // Handle Size
                if (currentContent.overrideSize)
                {
                    imageRect.sizeDelta = currentContent.customImageSize;
                    Debug.Log($"Size overridden to {imageRect.sizeDelta}");
                }
                else
                {
                    // Reset to original sprite size (or default if no sprite)
                    if (currentContent.image != null)
                    {
                        imageRect.sizeDelta = currentContent.image.rect.size;
                    }
                    else
                    {
                        imageRect.sizeDelta = Vector2.one * 100f; // Fallback size
                    }
                }

                // Live editing in Play Mode
                if (Application.isPlaying)
                {
                    if (currentContent.overridePosition)
                        currentContent.customImagePosition = imageRect.anchoredPosition;

                    if (currentContent.overrideSize)
                        currentContent.customImageSize = imageRect.sizeDelta;
                }
            }
        }

        // Rest of your UpdateContent method...
        if (titleText != null) titleText.text = currentContent.title;
        if (descriptionText != null) descriptionText.text = currentContent.description;

        UpdateNavigationText();

        if (currentContent.pauseGameDuringThisPage && !TimeManager.Instance.IsPaused)
        {
            TimeManager.Instance.PauseGame();
        }
        else if (!currentContent.pauseGameDuringThisPage && TimeManager.Instance.IsPaused)
        {
            TimeManager.Instance.ResumeGame();
        }
    }

    private void UpdateNavigationText()
    {
        if (navigationPrompt == null) return;

        string promptText = "";

        if (currentPageIndex > 0)
            promptText += "Previous: RMB\n";

        if (currentPageIndex < tutorialPages.Count - 1)
            promptText += "Next: LMB";
        else
            promptText += "Close: LMB";

        navigationPrompt.text = promptText;
        promptVisible = true;
        blinkTimer = 0f;
        navigationPrompt.gameObject.SetActive(true);
    }
}


