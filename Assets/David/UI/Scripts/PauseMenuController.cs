using UnityEngine;

public class PauseMenuController : BaseMenuController
{
    [Header("Pause Settings")]
    [SerializeField] private GameObject optionsMenu;

    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start();
        canUseButtons = true;
        if (Input.GetKeyDown(KeyCode.Escape))
        {

        }
    }

    public void SetVisible(bool visible)
    {
        Debug.Log("Step 1: Set menu visible");  // Log when visibility is being toggled.
        IsVisible = visible;
        gameObject.SetActive(visible);

        if (visible)
        {
            Debug.Log("Step 2: Menu opened");  // Log when the menu opens.
            SelectButton(0);
            AudioManager.Instance.PlayMenuOpen();
            TimeManager.Instance?.PauseGame();
            if (optionsMenu) optionsMenu.SetActive(false);
        }
        else
        {
            Debug.Log("Step 3: Menu closed");  // Log when the menu closes.
            TimeManager.Instance?.ResumeGame();
            AudioManager.Instance.PlayMenuClose();
        }
    }

    public void OnResumePressed() => SetVisible(false);

    public void OnOptionsPressed()
    {
        if (!optionsMenu) return;
        optionsMenu.SetActive(true);
    }


    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();

        TimeManager.Instance?.ResumeGame();

        SetVisible(false);
        SceneLoader.Instance.LoadSceneWithFade("Scene_MainMenu");
    }
}
