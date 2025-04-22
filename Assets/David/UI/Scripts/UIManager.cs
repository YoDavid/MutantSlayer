using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hud;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private GameOverMenuController gameOverMenu;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovementController playerMovement;
    [SerializeField] private PlayerAttackController playerAttackController;

    [Header("Boss UI")]
    [SerializeField] private GameObject bossHealthBarContainer;
    [SerializeField] private Transform bossTransform;
    [SerializeField] private float bossBarShowDistance = 15f;

    private Transform playerTransform;

    private void Awake()
    {
        if (!hud) hud = transform.Find("Canvas/HUD")?.gameObject;
        if (!pauseMenu) pauseMenu = transform.Find("Canvas/PauseMenu")?.GetComponent<PauseMenuController>();
        if (!gameOverMenu) gameOverMenu = transform.Find("Canvas/GameOverMenu")?.GetComponent<GameOverMenuController>();
        if (!playerHealth) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (!playerMovement) playerMovement = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerMovementController>();
        if (!playerAttackController) playerAttackController = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerAttackController>();

        SetHUDVisible(true);

        if (gameOverMenu != null) gameOverMenu.gameObject.SetActive(false);

        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (bossHealthBarContainer != null)
            bossHealthBarContainer.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)
            && (gameOverMenu == null || !gameOverMenu.IsVisible)
            && playerHealth != null
            && playerHealth.CurrentHealth > 0)
        {
            TogglePauseMenu();
        }

        HandleBossHealthBarVisibility();
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
        if (hud != null)
            hud.SetActive(visible);
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu == null) return;

        bool shouldPause = !pauseMenu.IsVisible;

        pauseMenu.SetVisible(shouldPause);
        SetHUDVisible(!shouldPause);

        // Safely pause/unpause game time
        Time.timeScale = shouldPause ? 0f : 1f;

        // Enable/disable player movement
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(!shouldPause);

        // Disable/enable the PlayerAttackController when pausing/unpausing
        if (playerAttackController != null)
            playerAttackController.enabled = !shouldPause;
    }


    private void HandleBossHealthBarVisibility()
    {
        if (playerTransform == null || bossTransform == null || bossHealthBarContainer == null)
            return;

        float distance = Vector2.Distance(playerTransform.position, bossTransform.position);
        bool shouldShow = distance <= bossBarShowDistance;

        if (bossHealthBarContainer.activeSelf != shouldShow)
            bossHealthBarContainer.SetActive(shouldShow);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 1f; // Resume time just in case

        SetHUDVisible(false);

        if (gameOverMenu != null)
        {
            gameOverMenu.gameObject.SetActive(true);
            gameOverMenu.StartGameOverSequence();
        }
    }
}
