using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuNavigationController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float inputRepeatDelay = 0.2f;
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode selectKey = KeyCode.Return;

    [Header("References")]
    [SerializeField] private MainMenuButton[] menuButtons;

    private int _selectedIndex = 0;
    private bool _isTransitioning = false;
    private float _lastInputTime;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        InitializeButtons();
        SelectButton(0);
    }

    private void InitializeButtons()
    {
        foreach (var button in menuButtons)
        {
            if (button != null && button.Button != null)
            {
                button.Button.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayButtonClick();
                });
            }
        }
    }

    private void Update()
    {
        if (_isTransitioning) return;
        HandleKeyboardNavigation();
    }

    private void HandleKeyboardNavigation()
    {
        if (Time.time < _lastInputTime + inputRepeatDelay) return;

        if (Input.GetKeyDown(upKey)) Navigate(-1);
        else if (Input.GetKeyDown(downKey)) Navigate(1);
        else if (Input.GetKeyDown(selectKey))
        {
            menuButtons[_selectedIndex].Button.onClick.Invoke();
            _lastInputTime = Time.time;
        }
    }

    private void Navigate(int direction)
    {
        AudioManager.Instance.PlayButtonHover(); // Changed from PlayMenuScroll()
        StartCoroutine(TransitionToButton(_selectedIndex + direction));
        _lastInputTime = Time.time;
    }

    private IEnumerator TransitionToButton(int newIndex)
    {
        _isTransitioning = true;
        newIndex = (newIndex + menuButtons.Length) % menuButtons.Length;

        yield return StartCoroutine(FadeButton(menuButtons[_selectedIndex], false));
        menuButtons[_selectedIndex].Deselect();

        menuButtons[newIndex].Select();
        yield return StartCoroutine(FadeButton(menuButtons[newIndex], true));

        _selectedIndex = newIndex;
        _isTransitioning = false;
    }

    private IEnumerator FadeButton(MainMenuButton button, bool fadeIn)
    {
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        while (elapsed < fadeDuration)
        {
            button.SetAlpha(Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.SetAlpha(endAlpha);
    }

    private void SelectButton(int index)
    {
        if (menuButtons == null || menuButtons.Length == 0) return;
        index = Mathf.Clamp(index, 0, menuButtons.Length - 1);
        menuButtons[index].Select();
        _selectedIndex = index;
    }
}