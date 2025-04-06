using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public abstract class BaseMenuController : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] protected float inputRepeatDelay = 0.2f;
    [SerializeField] protected KeyCode upKey = KeyCode.W;
    [SerializeField] protected KeyCode downKey = KeyCode.S;
    [SerializeField] protected KeyCode selectKey = KeyCode.Return;
    [SerializeField] protected MenuButton[] menuButtons;

    protected int currentIndex = 0;
    protected bool isTransitioning = false;  // Prevent fast repeated inputs
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
        if (SceneLoader.Instance != null && !isTransitioning)  // Ensure no transition is active
        {
            SceneLoader.Instance.LoadSceneWithFade(sceneName);
            isTransitioning = true;  // Mark as transitioning when the scene load starts
        }
        else
        {
            Debug.LogError("SceneLoader instance is null or transition is active!");
            SceneManager.LoadScene(sceneName);
        }
    }

    protected virtual void Update()
    {
        if (isTransitioning) return;  // Ignore input during transitions
        HandleKeyboardNavigation();
    }

    protected virtual void HandleKeyboardNavigation()
    {
        if (Time.unscaledTime < lastInputTime + inputRepeatDelay) return;

        if (Input.GetKeyDown(upKey) || Input.GetKeyDown(downKey))
        {
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

    protected virtual IEnumerator TransitionToButton(int newIndex)
    {
        isTransitioning = true;  // Prevent any other transitions while one is ongoing

        newIndex = Mathf.Clamp(newIndex, 0, menuButtons.Length - 1);

        menuButtons[currentIndex].Deselect();
        menuButtons[newIndex].Select();
        currentIndex = newIndex;

        isTransitioning = false;  // Enable transition again after it is finished

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
