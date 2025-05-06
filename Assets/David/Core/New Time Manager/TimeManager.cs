using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;


    [Header("Time Scales")]
    public float normalTimeScale = 1f;
    public float pauseTimeScale = 0f;

    private float currentTimeScale = 1f;
    private bool isPaused = false;

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
    }

    public void PauseGame()
    {
        Debug.Log("Step 4: Game paused");  // Log when game pauses.
        isPaused = true;
        SetTimeScale(pauseTimeScale);
        AudioManager.Instance?.SetPauseState(true);
    }

    public void ResumeGame()
    {
        Debug.Log("Step 5: Game resumed");  // Log when game resumes.
        isPaused = false;
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
            SetTimeScale(normalTimeScale);
    }

    public void ResetTimeScale()
    {
        if (!isPaused)
            SetTimeScale(normalTimeScale);
    }


    public bool IsPaused => isPaused;
}
