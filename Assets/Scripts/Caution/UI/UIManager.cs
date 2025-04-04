using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hud;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private GameOverMenuController gameOverMenu;

    private void Awake()
    {
        if (!hud) hud = transform.Find("Canvas/HUD")?.gameObject;
        if (!pauseMenu) pauseMenu = transform.Find("Canvas/PauseMenu")?.GetComponent<PauseMenuController>();
        if (!gameOverMenu) gameOverMenu = transform.Find("Canvas/GameOverMenu")?.GetComponent<GameOverMenuController>();

        SetHUDVisible(true);
        if (gameOverMenu != null) gameOverMenu.SetVisible(false);
    }

    public void SetHUDVisible(bool visible)
    {
        if (hud) hud.SetActive(visible);
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu == null) return;
        bool shouldPause = !pauseMenu.IsVisible;
        pauseMenu.SetVisible(shouldPause);
        SetHUDVisible(!shouldPause);
    }

    public void ShowGameOver()
    {
        SetHUDVisible(false);
        if (gameOverMenu != null) gameOverMenu.SetVisible(true);
    }
}