using UnityEngine;

public class PauseMenuController : BaseMenuController
{
    [Header("Pause Settings")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private float pauseTimeScale = 0f;

    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start();
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        gameObject.SetActive(visible);
        Time.timeScale = visible ? pauseTimeScale : 1f;

        if (visible)
        {
            SelectButton(0);
            AudioManager.Instance.PlayMenuOpen();
            if (optionsMenu) optionsMenu.SetActive(false);
        }
        else
        {
            AudioManager.Instance.PlayMenuClose();
        }
    }

    public void OnResumePressed() => SetVisible(false);
    public void OnOptionsPressed() => optionsMenu.SetActive(true);

    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        SetVisible(false);

        SceneLoader.Instance.LoadScene("Scene_MainMenu");

        Debug.Log("MainMenu button pressed - Scene load initiated");
    }
}