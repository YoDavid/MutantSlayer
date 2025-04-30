using UnityEngine;

public class PauseMenuController : BaseMenuController
{
    [Header("Pause Settings")]
    [SerializeField] private GameObject optionsMenu;

    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start();
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        gameObject.SetActive(visible);

        if (visible)
        {
            TimeManager.Instance?.PauseGame();
            SelectButton(0);
            AudioManager.Instance.PlayMenuOpen();
            if (optionsMenu) optionsMenu.SetActive(false);
        }
        else
        {
            TimeManager.Instance?.ResumeGame();
            AudioManager.Instance.PlayMenuClose();
        }
    }

    public void OnResumePressed() => SetVisible(false);

    public void OnOptionsPressed() => optionsMenu.SetActive(true);

    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();

        TimeManager.Instance?.ResumeGame();

        SetVisible(false);
        SceneLoader.Instance.LoadSceneWithFade("Scene_MainMenu");
    }
}
