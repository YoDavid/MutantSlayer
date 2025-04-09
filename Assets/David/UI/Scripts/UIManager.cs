using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hud;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private GameOverMenuController gameOverMenu;
    [SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        if (!hud) hud = transform.Find("Canvas/HUD")?.gameObject;
        if (!pauseMenu) pauseMenu = transform.Find("Canvas/PauseMenu")?.GetComponent<PauseMenuController>();
        if (!gameOverMenu) gameOverMenu = transform.Find("Canvas/GameOverMenu")?.GetComponent<GameOverMenuController>();

        SetHUDVisible(true);
        if (gameOverMenu != null) gameOverMenu.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (gameOverMenu == null || !gameOverMenu.IsVisible))
        {
            TogglePauseMenu();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        ShowGameOver();
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
        if (gameOverMenu != null)
        {
            gameOverMenu.gameObject.SetActive(true);
            gameOverMenu.StartGameOverSequence();
        }
    }
}