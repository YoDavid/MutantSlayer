using System.Collections;
using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float[] attackTimings = { 0.7f, 1.1f, 1.9f };
    private Collider2D attackCollider;

    [SerializeField] private PlayerHealth playerHealth;
    private Collider2D playerHurtBoxCollider;

    private bool isPlayerInRange = false;

    private Vector2 originalOffset;
    public float colliderShift = 3f;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider is not attached to the BossAttackHitbox object.");
        }
        attackCollider.enabled = false;

        GameObject player = GameObject.FindWithTag("Player");
        playerHurtBoxCollider = player.GetComponent<Collider2D>();
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (attackCollider is BoxCollider2D boxCollider)
        {
            originalOffset = boxCollider.offset;
        }
    }

    public void ActivateComboAttackCollider()
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

            attackCollider.enabled = true;

            yield return new WaitForSeconds(attackDuration);
            attackCollider.enabled = false;
        }
    }

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = true;
            ApplyDamage();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            isPlayerInRange = false;
            ApplyDamage();
        }
    }

    private void OnDrawGizmos()
    {
        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
    }

    public void FlipCollider(bool isFlipped)
    {
        if (attackCollider is BoxCollider2D boxCollider)
        {
            if (isFlipped) // If facing right (flipX is true)
            {
                boxCollider.offset = new Vector2(originalOffset.x + colliderShift, originalOffset.y);
            }
            else // If facing left (flipX is false)
            {
                boxCollider.offset = originalOffset;
            }
        }
    }
}