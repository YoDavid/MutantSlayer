using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hud;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private GameOverMenuController gameOverMenu;
    [SerializeField] private PlayerHealth playerHealth;

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

        SetHUDVisible(true);
        if (gameOverMenu != null) gameOverMenu.gameObject.SetActive(false);
        playerHealth = FindAnyObjectByType<PlayerHealth>();
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }

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
        if (hud) hud.SetActive(visible);
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu == null) return;
        bool shouldPause = !pauseMenu.IsVisible;
        pauseMenu.SetVisible(shouldPause);
        SetHUDVisible(!shouldPause);
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
        SetHUDVisible(false);
        if (gameOverMenu != null)
        {
            gameOverMenu.gameObject.SetActive(true);
            gameOverMenu.StartGameOverSequence();
        }
    }
}