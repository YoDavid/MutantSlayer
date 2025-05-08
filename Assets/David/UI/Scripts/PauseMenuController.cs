using UnityEngine;

public class PauseMenuController : BaseMenuController
{
    [Header("Pause Settings")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private UIManager uiManager;

    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start();
        canUseButtons = true;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        gameObject.SetActive(visible);

        if (visible)
        {
            SelectButton(0);
            AudioManager.Instance.PlayMenuOpen();
            TimeManager.Instance?.PauseGame();
            FindObjectOfType<ForceMouseHidden>()?.UnlockAndShowMouse(); 
            if (optionsMenu) optionsMenu.SetActive(false);
        }
        else
        {
            TimeManager.Instance?.ResumeGame();
            AudioManager.Instance.PlayMenuClose();
            uiManager.SetHUDVisible(true);
            FindObjectOfType<ForceMouseHidden>()?.LockAndHideMouse();
        }

    }

    public void OnResumePressed()
    {
        SetVisible(false);
        TimeManager.Instance?.ResumeGame();
        AudioManager.Instance.PlayMenuClose();
        uiManager.SetHUDVisible(true);
        uiManager.SetPlayerInputEnabled(enabled);
        canUseButtons = false;
    }

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
