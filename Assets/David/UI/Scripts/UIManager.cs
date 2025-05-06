using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class TutorialContent
{
    public Sprite image;
    [Header("TextMeshPro Fields")]
    public string title;
    [TextArea(3, 10)]
    public string description;
    public bool pauseGameDuringThisPage = true;

    [Header("Image Position Settings")]
    [Tooltip("Enable to set custom position for this page")]
    public bool overridePosition = false;
    [Tooltip("Custom position when override is enabled")]
    public Vector2 customImagePosition = Vector2.zero;

    [Header("Image Size Settings")]
    [Tooltip("Enable to set custom size for this page")]
    public bool overrideSize = false;
    [Tooltip("Custom size when override is enabled")]
    public Vector2 customImageSize = Vector2.one * 100f; // Default size

}


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialSystem;
    [SerializeField] private TutorialSystemController TutorialScript;

    private bool wasHUDVisibleBeforeTutorial = true; // Track HUD visibility state before tutorial
    private Transform playerTransform;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

        if (tutorialSystem != null)
        {
            tutorialSystem.SetActive(false); // Start disabled
        }
    }

    private void Update()
    {
        if (pauseMenu == null) return;
        bool shouldPause = !pauseMenu.IsVisible;

        if ((Input.GetKeyDown(KeyCode.Escape))
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

        // If tutorial is showing, hide it instead of pausing
        if (TutorialScript != null && TutorialScript.IsShowing)
        {
            HideTutorial();
            return;
        }

        bool shouldPause = !pauseMenu.IsVisible;
        pauseMenu.SetVisible(shouldPause);
        SetHUDVisible(!shouldPause);

        // Handle pause game using TimeManager
        if (shouldPause)
            TimeManager.Instance?.PauseGame();
        else
            TimeManager.Instance?.ResumeGame();

        SetPlayerInputEnabled(!shouldPause);
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
        TimeManager.Instance?.ResumeGame();

        SetHUDVisible(false);

        if (gameOverMenu != null)
        {
            gameOverMenu.gameObject.SetActive(true);
            gameOverMenu.StartGameOverSequence();
        }

    }

    public void ShowTutorial(List<TutorialContent> pages)
    {
        if (TutorialScript == null || pages == null || pages.Count == 0) return;

        // Save current HUD visibility state before showing tutorial
        wasHUDVisibleBeforeTutorial = hud != null && hud.activeSelf;

        // Hide HUD when showing tutorial
        SetHUDVisible(false);

        if (!TimeManager.Instance.IsPaused)
        {
            TimeManager.Instance.PauseGame();
        }

        // Disable player input
        SetPlayerInputEnabled(false);

        // Show tutorial content
        TutorialScript.ShowTutorial(pages);

    }

    public void HideTutorial()
    {
        if (TutorialScript == null) return;

        TutorialScript.HideTutorial();

        // Restore HUD visibility to its previous state before tutorial was shown
        if (hud != null)
        {
            SetHUDVisible(wasHUDVisibleBeforeTutorial);
        }

        // Only resume if not in other paused states
        if (!pauseMenu.IsVisible)
        {
            TimeManager.Instance.ResumeGame();
        }

        // Restore player input if game isn't paused
        if (!TimeManager.Instance.IsPaused)
        {
            SetPlayerInputEnabled(true);
        }
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(enabled);
        if (playerAttackController != null) playerAttackController.enabled = enabled;
    }
}
