using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Scales")]
    public float normalTimeScale = 1f;
    public float pauseTimeScale = 0f;

    [Header("Player Reference")]
    public Rigidbody2D playerRigidbody; // Assign in Inspector
    public PlayerMovementController playerController; // Assign your player controller script here

    private float currentTimeScale = 1f;
    private bool isPaused = false;
    private Vector2 storedVelocity; // To store velocity before pause
    private float storedAngularVelocity; // For rotational velocity if needed

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetTimeScale(normalTimeScale);
    }

    public void SetTimeScale(float scale)
    {
        currentTimeScale = scale;
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Maintain physics consistency
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Store current velocity before pausing
        if (playerRigidbody != null)
        {
            storedVelocity = playerRigidbody.velocity;
            storedAngularVelocity = playerRigidbody.angularVelocity;
            playerRigidbody.isKinematic = true;
        }

        // Disable player controller input
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        SetTimeScale(pauseTimeScale);
        AudioManager.Instance?.SetPauseState(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Re-enable physics and restore velocity
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
            playerRigidbody.velocity = storedVelocity;
            playerRigidbody.angularVelocity = storedAngularVelocity;
        }

        // Re-enable player controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        SetTimeScale(normalTimeScale);
        AudioManager.Instance?.SetPauseState(false);
    }

    public void TemporarilySlowTime(float newTimeScale, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowTimeRoutine(newTimeScale, duration));
    }

    private IEnumerator SlowTimeRoutine(float newTimeScale, float duration)
    {
        SetTimeScale(newTimeScale);
        yield return new WaitForSecondsRealtime(duration);
        if (!isPaused)
        {
            SetTimeScale(normalTimeScale);
        }
    }

    public void ResetTimeScale()
    {
        if (!isPaused)
        {
            SetTimeScale(normalTimeScale);
        }
    }

    public bool IsPaused => isPaused;
}