using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(TilemapCollider2D))]
public class DisappearingTilemapPlatform : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Time before the platform starts its disappear sequence")]
    public float delayToActivate = 3f;

    [Tooltip("Duration of each blink (on and off)")]
    public float blinkSpeed = 0.1f;

    [Tooltip("Number of times the platform will blink")]
    public int numberOfBlinks = 5;

    [Tooltip("Time the platform remains disappeared")]
    public float disappearTime = 5f;

    private TilemapRenderer platformRenderer;
    private TilemapCollider2D platformCollider;
    private float standingTimer = 0f;
    private bool isPlayerOn = false;
    private Coroutine disappearSequenceCoroutine;

    void Start()
    {
        // Get necessary components
        platformRenderer = GetComponent<TilemapRenderer>();
        platformCollider = GetComponent<TilemapCollider2D>();

        // Ensure components exist
        if (!platformRenderer || !platformCollider)
        {
            Debug.LogError("DisappearingTilemapPlatform requires TilemapRenderer and TilemapCollider2D!");
            enabled = false;
        }
    }

    void Update()
    {
        // If the player is on the platform, start the timer
        if (isPlayerOn)
        {
            standingTimer += Time.deltaTime;

            // If the timer exceeds the activation delay and the disappear sequence hasn't started
            if (standingTimer >= delayToActivate && disappearSequenceCoroutine == null)
            {
                disappearSequenceCoroutine = StartCoroutine(HandleDisappearance());
                standingTimer = 0f; // Reset the timer
            }
        }
        else
        {
            standingTimer = 0f; // Reset the timer if the player leaves
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = true;
            standingTimer = 0f; // Reset the timer when the player enters
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
            standingTimer = 0f;

            // If the disappear sequence is running, stop it and reset the platform
            if (disappearSequenceCoroutine != null)
            {
                StopCoroutine(disappearSequenceCoroutine);
                disappearSequenceCoroutine = null;
                SetPlatformActive(true); // Make platform visible and collidable again
            }
        }
    }

    IEnumerator HandleDisappearance()
    {
        // Blinking effect (only control the renderer)
        for (int i = 0; i < numberOfBlinks; i++)
        {
            SetPlatformVisible(false); // Turn off visibility
            yield return new WaitForSeconds(blinkSpeed);
            SetPlatformVisible(true);  // Turn on visibility
            yield return new WaitForSeconds(blinkSpeed);

            // Check if the player has left during the blinking phase
            if (!isPlayerOn)
            {
                yield break; // Exit the coroutine early
            }
        }

        // Final disappearance (turn off renderer and collider)
        SetPlatformActive(false);

        // Wait for the platform to reappear
        yield return new WaitForSeconds(disappearTime);

        // Reappear (turn on renderer and collider)
        SetPlatformActive(true);
        disappearSequenceCoroutine = null; // Allow the sequence to run again
    }

    // Helper function to set the active state (visibility and collision) of the platform
    void SetPlatformActive(bool active)
    {
        if (platformRenderer != null)
        {
            platformRenderer.enabled = active;
        }
        if (platformCollider != null)
        {
            platformCollider.enabled = active;
        }
    }

    // Helper function to set the visibility of the platform
    void SetPlatformVisible(bool visible)
    {
        if (platformRenderer != null)
        {
            platformRenderer.enabled = visible;
        }
    }
}