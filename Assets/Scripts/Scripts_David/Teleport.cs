using TMPro;
using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour
{
    // Standard teleport settings
    public Transform targetLocation;
    public string targetTag = "Player";
    public float fadeDuration = 0.5f;
    public float pauseDuration = 2f;
    public string autoTeleportLevelName = "to level 3";

    // Animation control settings
    [Header("Animation Control")]
    public bool playIdleAnimation = false;
    public bool playFallingAnimation = false;

    // Special platform settings
    [Header("Platform Special Settings")]
    public float platformFadeDuration = 0.3f;
    public float platformPauseBeforeEnable = 1.5f;
    public float platformPreFallDelay = 1f;

    // Debug visibility
    [Header("Debug States (Read Only)")]
    [SerializeField] private bool _isMovementEnabled = true;
    [SerializeField] private bool _isAttackEnabled = true;
    [SerializeField] private bool _isInFallingState = false;

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private PlayerAttackController attackController;
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private bool isPlayerInZone = false;
    [SerializeField] private bool isPlatformCollision = false;
    [SerializeField] private bool usePlatformTiming = false;

    [Header("UI Prompt")]
    public TMP_Text interactionPromptText;

    // Public properties for external access
    public bool IsMovementEnabled => _isMovementEnabled;
    public bool IsAttackEnabled => _isAttackEnabled;



    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(targetTag);
        InitializePlayerComponents();
        UpdateDebugStates(); // Initialize debug states
    }

    private void InitializePlayerComponents()
    {
        if (player != null)
        {
            movementController = player.GetComponent<PlayerMovementController>();
            attackController = player.GetComponent<PlayerAttackController>();
            playerAnimation = player.GetComponent<PlayerAnimationController>();
        }
    }

    private void UpdateDebugStates()
    {
        if (movementController != null) _isMovementEnabled = movementController.enabled;
        if (attackController != null) _isAttackEnabled = attackController.enabled;
    }

    private void Update()
    {
        bool isAutoTeleportLevel = transform.name == autoTeleportLevelName || gameObject.scene.name == autoTeleportLevelName;

        if (isPlayerInZone && (Input.GetKeyDown(KeyCode.F) || isAutoTeleportLevel))
        {
            if (!isPlatformCollision)
            {
                StartStandardTeleportSequence(isAutoTeleportLevel);
            }
        }

        UpdateDebugStates(); // Keep debug states updated
    }

    private void SetMovementEnabled(bool enabled)
    {
        if (movementController != null)
        {
            movementController.enabled = enabled;
            _isMovementEnabled = enabled;
        }
    }

    private void SetAttackEnabled(bool enabled)
    {
        if (attackController != null)
        {
            attackController.enabled = enabled;
            _isAttackEnabled = enabled;
        }
    }

    private void StartStandardTeleportSequence(bool isAuto)
    {
        movementController.StopStepSounds();
        playerAnimation.rb.velocity = Vector2.zero;

        // Set animation state based on inspector settings
        if (playIdleAnimation)
        {
            playerAnimation.SetIdleState(true);
        }
        else if (playFallingAnimation)
        {
            playerAnimation.SetFallingState(true);
            _isInFallingState = true;
        }

        isPlatformCollision = false;
        StartCoroutine(TeleportWithFade(usePlatformTiming: isAuto));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        isPlayerInZone = true;

        if (!playFallingAnimation && interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(true);

        if (other.gameObject.name == "PlatformDisappearAndReappear (14)")
        {
            isPlatformCollision = true;
            StartCoroutine(HandlePlatformCollision(other.gameObject));
        }
        else if (transform.name == autoTeleportLevelName || gameObject.scene.name == autoTeleportLevelName)
        {
            StartStandardTeleportSequence(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        isPlayerInZone = false;

        if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }



    private IEnumerator HandlePlatformCollision(GameObject playerObj)
    {
        if (movementController == null || playerAnimation == null)
        {
            InitializePlayerComponents();
        }

        yield return new WaitForSeconds(platformPreFallDelay);

        SetMovementEnabled(false);
        movementController.StopStepSounds();

        if (playerAnimation != null && playerAnimation.rb != null)
        {
            playerAnimation.rb.velocity = Vector2.zero;
            // Force falling animation for platform collision
            playerAnimation.SetFallingState(true);
            _isInFallingState = true;
        }

        yield return StartCoroutine(PlatformTeleportSequence());
    }

    private IEnumerator PlatformTeleportSequence()
    {
        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(
                SceneLoader.Instance.FadeWithOverlay(0, 1, platformFadeDuration));
        }

        player.transform.position = targetLocation.position;
        playerAnimation.rb.velocity = Vector2.zero;

        yield return new WaitForSecondsRealtime(platformPauseBeforeEnable);

        SetMovementEnabled(true);
        SetAttackEnabled(true);
        playerAnimation.SetFallingState(false);
        _isInFallingState = false;

        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(
                SceneLoader.Instance.FadeWithOverlay(1, 0, platformFadeDuration));
        }

        isPlatformCollision = false;
    }

    private IEnumerator TeleportWithFade(bool usePlatformTiming = false)
    {
        if (!isPlayerInZone) yield break;
        isPlayerInZone = false;

        playerAnimation.rb.velocity = Vector2.zero;
        playerAnimation.SetSpeed(0);
        SetMovementEnabled(false);
        SetAttackEnabled(false);

        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(
                SceneLoader.Instance.FadeWithOverlay(0, 1, fadeDuration));
        }

        player.transform.position = targetLocation.position;

        float waitDuration = usePlatformTiming ? platformPauseBeforeEnable : pauseDuration;
        yield return new WaitForSecondsRealtime(waitDuration);

        // Reset animation states after teleport
        if (playIdleAnimation)
        {
            playerAnimation.SetIdleState(false);
        }
        else if (playFallingAnimation)
        {
            playerAnimation.SetFallingState(false);
            _isInFallingState = false;
        }

        SetMovementEnabled(true);
        SetAttackEnabled(true);

        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(
                SceneLoader.Instance.FadeWithOverlay(1, 0, fadeDuration));
        }
    }
}