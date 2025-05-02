using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour
{
    public Transform targetLocation;
    public string targetTag = "Player";
    public float fadeDuration = 0.5f;
    public float pauseDuration = 2f;
    public string autoTeleportLevelName = "to level 3"; // Name of the level where auto-teleport should occur

    [SerializeField] private GameObject player;
    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private PlayerAttackController attackController;
    [SerializeField] private bool isPlayerInZone = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(targetTag);

        if (player != null)
        {
            movementController = player.GetComponent<PlayerMovementController>();
            attackController = player.GetComponent<PlayerAttackController>();
        }
        else
        {
            Debug.LogWarning("Teleport: Player not found at Start!");
        }
    }

    private void Update()
    {
        bool isAutoTeleportLevel = transform.name == autoTeleportLevelName || gameObject.scene.name == autoTeleportLevelName;

        if (isPlayerInZone && (Input.GetKeyDown(KeyCode.F) || isAutoTeleportLevel))
        {
            Debug.Log("Teleport: Starting teleport..." + (isAutoTeleportLevel ? " (Auto-triggered)" : " (F pressed)"));
            StartCoroutine(TeleportWithFade());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInZone = true;
            Debug.Log("Teleport: Player entered teleport zone.");

            // Immediate teleport if this is the auto-teleport level
            bool isAutoTeleportLevel = transform.name == autoTeleportLevelName || gameObject.scene.name == autoTeleportLevelName;
            if (isAutoTeleportLevel)
            {
                Debug.Log("Teleport: Auto-teleport level detected. Triggering teleport.");
                StartCoroutine(TeleportWithFade());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            isPlayerInZone = false;
            Debug.Log("Teleport: Player exited teleport zone.");
        }
    }

    private IEnumerator TeleportWithFade()
    {
        // Prevent multiple triggers
        if (!isPlayerInZone) yield break;

        isPlayerInZone = false;

        if (movementController != null) movementController.enabled = false;
        if (attackController != null) attackController.enabled = false;

        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.FadeWithOverlay(0, 1, fadeDuration));
        }

        Vector3 fromPosition = player.transform.position;
        player.transform.position = targetLocation.position;
        Debug.Log($"Player teleported from {fromPosition} to {targetLocation.position}");

        yield return new WaitForSecondsRealtime(pauseDuration);

        if (SceneLoader.Instance != null)
        {
            yield return SceneLoader.Instance.StartCoroutine(SceneLoader.Instance.FadeWithOverlay(1, 0, fadeDuration));
        }

        if (movementController != null) movementController.enabled = true;
        if (attackController != null) attackController.enabled = true;

        Debug.Log("Teleport complete. Player movement and attack re-enabled.");
    }
}