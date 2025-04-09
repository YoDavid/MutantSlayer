using System.Collections;
using UnityEngine;

public class BossComboAttackTimerTesting : MonoBehaviour
{
    private float[] attackTimings;
    private float attackDuration;
    private BossColliderHandlerTesting colliderHandler;
    private BossDamageHandlerTesting damageHandler;
    private CameraShake cameraShake;

    public void Initialize(float[] timings, float duration, BossColliderHandlerTesting collider, BossDamageHandlerTesting damage)
    {
        attackTimings = timings;
        attackDuration = duration;
        colliderHandler = collider;
        damageHandler = damage;
        cameraShake = FindAnyObjectByType<CameraShake>();
    }

    public void StartComboAttack()
    {
        StartCoroutine(ActivateComboWithIntervals());
    }

    private IEnumerator ActivateComboWithIntervals()
    {
        float startTime = Time.time;

        foreach (float attackTime in attackTimings)
        {
            float waitTime = attackTime - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            colliderHandler.EnableCollider();
            cameraShake?.ShakeCameraComboAttack();
            yield return new WaitForSeconds(attackDuration);
            colliderHandler.DisableCollider();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        damageHandler?.HandleCollision(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        damageHandler?.EndCollision(other);
    }

    private void OnDrawGizmos()
    {
        if (colliderHandler?.GetCollider() != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(colliderHandler.GetCollider().bounds.center, colliderHandler.GetCollider().bounds.size);
        }
    }
}