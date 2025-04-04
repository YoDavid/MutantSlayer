using UnityEngine;

public class MainMenu : BaseMenuController
{
    [Header("Menu References")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject loadGameMenu;

    protected override void Start()
    {
        base.Start(); 
        AudioManager.Instance.PlayMusic("menu_theme");
    }


    public void OnStartPressed() => LoadScene("Scene_SlideShow");

    public void OnOptionsPressed()
    {
        optionsMenu.SetActive(true);
        AudioManager.Instance.PlayButtonClick();
    }

    public void OnLoadPressed()
    {
        loadGameMenu.SetActive(true);
        AudioManager.Instance.PlayButtonClick();
    }

    public void OnQuitPressed()
    {
        AudioManager.Instance.PlayButtonClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}