using UnityEngine;

public class SmallEnemyAttackCollider : MonoBehaviour
{
    public SmallEnemyConfig config; // Reference to config

    private Collider2D attackCollider;
    private float lastHitTime;
    private PlayerHealth playerHealth;
    private SpriteRenderer enemySprite;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        enemySprite = GetComponentInParent<SpriteRenderer>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

        GameObject playerHurtbox = GameObject.Find("PlayerHurtbox");
        if (playerHurtbox != null) playerHealth = playerHurtbox.GetComponentInParent<PlayerHealth>();
    }

    public void EnableAttackCollider()
    {
        attackCollider.enabled = true;
        UpdateColliderPosition();
    }

    public void DisableAttackCollider() => attackCollider.enabled = false;

    private void UpdateColliderPosition()
    {
        bool isFacingLeft = enemySprite.flipX;
        transform.localPosition = isFacingLeft ? config.leftFacingOffset : config.rightFacingOffset; // Now using config
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "PlayerHurtbox") return;
        if (playerHealth == null || Time.time < lastHitTime + config.hitCooldown) return; // Fixed
        DealDamage();
    }


    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (!playerHealth.IsPlayerInvulnerable())
            playerHealth.TakeDamage(config.attackDamage); // Now using config
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)config.rightFacingOffset, 0.1f); // Fixed
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)config.leftFacingOffset, 0.1f); // Fixed
    }
}