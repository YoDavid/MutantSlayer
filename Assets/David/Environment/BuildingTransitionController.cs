using UnityEngine;
using System.Collections;

public class BuildingTransitionController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject buildingOverlay; // The black overlay sprite
    public string playerTag = "Player";
    public float activationDelay = 0.1f;

    [SerializeField] private bool isExteriorTrigger; // Set in Inspector!

    private static bool isInExteriorSpace;
    private static bool isInTransitionArea;
    private static bool hasFullyExited; // New flag to track if player left completely
    private static Coroutine transitionRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (isExteriorTrigger)
        {
            HandleExteriorTriggerEnter();
        }
        else // Interior trigger
        {
            hasFullyExited = false; // Player is re-entering
            HandleInteriorTriggerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (isExteriorTrigger)
        {
            HandleExteriorTriggerExit();
            // Only set fully exited if not touching interior
            if (!isInTransitionArea) hasFullyExited = true;
        }
        else // Interior trigger
        {
            HandleInteriorTriggerExit();
        }
    }

    private void HandleExteriorTriggerEnter()
    {
        isInExteriorSpace = true;
        UpdateOverlayState();
    }

    private void HandleInteriorTriggerEnter()
    {
        isInTransitionArea = true;
        UpdateOverlayState();
    }

    private void HandleExteriorTriggerExit()
    {
        isInExteriorSpace = false;
        UpdateOverlayState();
    }

    private void HandleInteriorTriggerExit()
    {
        isInTransitionArea = false;
        UpdateOverlayState();
    }

    private void UpdateOverlayState()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(UpdateOverlayWithDelay());
    }

    private IEnumerator UpdateOverlayWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        if (buildingOverlay != null)
        {
            // Only show overlay if:
            // 1. Not in exterior space AND
            // 2. (Either in transition area OR hasn't fully exited)
            bool shouldShowOverlay = !isInExteriorSpace &&
                                   (isInTransitionArea || !hasFullyExited);

            buildingOverlay.SetActive(shouldShowOverlay);
        }
    }
}