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

    private float currentTimeScale = 1f;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetTimeScale(normalTimeScale);
        Debug.Log("[TimeManager] Initialized with normal time scale: " + normalTimeScale);
    }

    public void SetTimeScale(float scale)
    {
        currentTimeScale = scale;
        Time.timeScale = scale;
    }

    public void PauseGame()
    {
        isPaused = true;
        Physics2D.SyncTransforms(); // Force physics update
        ResetPlayerVelocity();      // Now reset velocity
        SetTimeScale(pauseTimeScale);
        AudioManager.Instance?.SetPauseState(true);
    }

    private void ResetPlayerVelocity()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.isKinematic = true; // Disable physics forces
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false; // Re-enable physics
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