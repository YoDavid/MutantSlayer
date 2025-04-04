using UnityEngine;
using System.Collections;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] protected float inputRepeatDelay = 0.2f;
    [SerializeField] protected KeyCode upKey = KeyCode.W;
    [SerializeField] protected KeyCode downKey = KeyCode.S;
    [SerializeField] protected KeyCode selectKey = KeyCode.Return;
    [SerializeField] protected MenuButton[] menuButtons;

    [Header("Transition Settings")]
    [SerializeField] protected float buttonClickDelay = 0.3f;
    [SerializeField] protected CanvasGroup fadeOverlay;
    [SerializeField] protected float fadeDuration = 0.5f;

    protected int currentIndex = 0;
    protected bool isTransitioning = false;
    protected float lastInputTime;

    protected virtual void Awake()
    {
        if (menuButtons == null || menuButtons.Length == 0)
        {
            menuButtons = GetComponentsInChildren<MenuButton>();
        }
    }

    protected virtual void Start()
    {
        InitializeButtons();
        SelectButton(0);
    }

    protected virtual void InitializeButtons()
    {
        foreach (var button in menuButtons)
        {
            if (button?.button != null)
            {
                button.button.onClick.AddListener(() => AudioManager.Instance.PlayButtonClick());
            }
        }
    }

    protected void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(1f));
        }
        yield return new WaitForSeconds(buttonClickDelay);
        SceneLoader.Instance.LoadScene(sceneName);
    }

    protected IEnumerator FadeScreen(float targetAlpha)
    {
        float startAlpha = fadeOverlay.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }

    protected virtual void Update()
    {
        if (isTransitioning) return;
        HandleKeyboardNavigation();
    }

    protected virtual void HandleKeyboardNavigation()
    {
        // Use unscaledTime for pause menu compatibility
        if (Time.unscaledTime < lastInputTime + inputRepeatDelay) return;

        if (Input.GetKeyDown(upKey) || Input.GetKeyDown(downKey))
        {
            // Force sound play even if repeating
            AudioManager.Instance.PlayButtonHover();
            lastInputTime = Time.unscaledTime;

            int direction = Input.GetKeyDown(upKey) ? -1 : 1;
            StartCoroutine(TransitionToButton(currentIndex + direction));
        }
        else if (Input.GetKeyDown(selectKey))
        {
            menuButtons[currentIndex].button.onClick.Invoke();
            lastInputTime = Time.unscaledTime;
        }
    }

    protected virtual void Navigate(int direction)
    {
        AudioManager.Instance.PlayButtonHover();
        StartCoroutine(TransitionToButton(currentIndex + direction));
        lastInputTime = Time.time;
    }

    protected virtual IEnumerator TransitionToButton(int newIndex)
    {
        isTransitioning = true;
        newIndex = Mathf.Clamp(newIndex, 0, menuButtons.Length - 1);

        // Deselect current button
        menuButtons[currentIndex].Deselect();

        // Select new button
        menuButtons[newIndex].Select();

        currentIndex = newIndex;
        isTransitioning = false;

        yield return null;
    }

    protected virtual void SelectButton(int index)
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        index = Mathf.Clamp(index, 0, menuButtons.Length - 1);
        menuButtons[index].Select();
        currentIndex = index;
    }
}