using System.Collections;
using UnityEngine;

public class HitTimeController : MonoBehaviour
{
    [Header("Time Control Settings")]
    [SerializeField] private float normalHitStopDuration = 0.1f;
    [SerializeField] private float critHitSlowDuration = 0.2f;
    [SerializeField][Range(0, 1)] private float critTimeScale = 0.5f;
    [SerializeField] private float bossDurationMultiplier = 1.5f;
    [SerializeField] private float bossSlowMultiplier = 0.7f;

    [Header("References")]
    [SerializeField] private PlayerAttackHitbox attackHitbox;

    private Coroutine currentTimeEffect;

    private void Awake()
    {
        if (attackHitbox == null)
        {
            attackHitbox = GetComponentInChildren<PlayerAttackHitbox>();
        }

        if (attackHitbox != null)
        {
            attackHitbox.OnHit += HandleHit;
        }
        else
        {
            Debug.LogWarning("PlayerAttackHitbox reference not found in children!");
        }
    }

    private void HandleHit(bool isCritical, bool isBoss)
    {
        if (currentTimeEffect != null)
        {
            StopCoroutine(currentTimeEffect);
            Time.timeScale = 1f; // Reset immediately
        }

        float duration = isCritical ? critHitSlowDuration : normalHitStopDuration;
        float timeScale = isCritical ? critTimeScale : 0f; // Full stop for normal hits

        if (isBoss)
        {
            duration *= bossDurationMultiplier;
            if (isCritical) timeScale *= bossSlowMultiplier;
        }

        currentTimeEffect = StartCoroutine(ExecuteTimeEffect(duration, timeScale));
    }

    private IEnumerator ExecuteTimeEffect(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        currentTimeEffect = null;
    }

    private void OnDestroy()
    {
        if (currentTimeEffect != null)
        {
            StopCoroutine(currentTimeEffect);
        }
        Time.timeScale = 1f;
    }
}