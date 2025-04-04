using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UIElements;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] protected float fadeDuration = 0.5f;
    [SerializeField] protected float inputRepeatDelay = 0.2f;
    [SerializeField] protected KeyCode upKey = KeyCode.W;
    [SerializeField] protected KeyCode downKey = KeyCode.S;
    [SerializeField] protected KeyCode selectKey = KeyCode.Return;

    [Header("References")]
    [SerializeField] protected MenuButton[] menuButtons;

    protected int _selectedIndex = 0;
    protected bool _isTransitioning = false;
    protected float _lastInputTime;

    protected virtual void Start()
    {
        InitializeButtons();
        SelectButton(0);
    }

    protected virtual void InitializeButtons()
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

    protected virtual void Update()
    {
        if (_isTransitioning) return;
        HandleKeyboardNavigation();
    }

    protected virtual void HandleKeyboardNavigation()
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

    protected virtual void Navigate(int direction)
    {
        AudioManager.Instance.PlayButtonHover();
        StartCoroutine(TransitionToButton(_selectedIndex + direction));
        _lastInputTime = Time.time;
    }

    protected virtual IEnumerator TransitionToButton(int newIndex)
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

    protected virtual IEnumerator FadeButton(MenuButton button, bool fadeIn)
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

    protected virtual void SelectButton(int index)
    {
        if (menuButtons == null || menuButtons.Length == 0) return;
        index = Mathf.Clamp(index, 0, menuButtons.Length - 1);
        menuButtons[index].Select();
        _selectedIndex = index;
    }
}