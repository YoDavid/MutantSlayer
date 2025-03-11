using System.Collections;
using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackDuration = 0.2f; // Active time per hit
    [SerializeField] private float[] attackTimings = { 0.7f, 1.1f, 1.9f }; // Attack moments (fixed values)
    private Collider2D attackCollider;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider is not attached to the BossAttackHitbox object.");
        }
        attackCollider.enabled = false; // Ensure the collider is initially disabled
    }

    public void ActivateComboAttackCollider()
    {
        StartCoroutine(ActivateComboWithIntervals());
    }

    private IEnumerator ActivateComboWithIntervals()
    {
        float startTime = Time.time; // Capture the exact start time

        foreach (float attackTime in attackTimings)
        {
            float waitTime = attackTime - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime); // Wait until the attack moment

            attackCollider.enabled = true; // Enable hitbox
            ApplyDamage(); // Apply damage to player
            yield return new WaitForSeconds(attackDuration); // Keep active for attack duration
            attackCollider.enabled = false; // Disable hitbox
        }
    }

    private void ApplyDamage()
    {
        PlayerHealth playerHealth = GameObject.FindWithTag("Player")?.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
