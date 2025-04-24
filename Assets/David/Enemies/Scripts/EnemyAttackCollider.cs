using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    public EnemyConfig enemyConfig; // Reference to config
    public DamageConfig damageConfig; // NEW: For damage ranges

    private Collider2D attackCollider;
    private float lastHitTime;
    private PlayerHealth playerHealth;
    private SpriteRenderer enemySprite;
    [SerializeField] private bool isFirstAttackActive = false;
    private Rigidbody2D playerRb;
    private DamageDealer damageDealer; // NEW: Handles damage calculations

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        enemySprite = GetComponentInParent<SpriteRenderer>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

        // NEW: Initialize damage dealer
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

        GameObject playerHurtbox = GameObject.Find("PlayerHurtbox");
        if (playerHurtbox != null)
        {
            playerHealth = playerHurtbox.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null) playerRb = playerHealth.GetComponent<Rigidbody2D>();
        }
    }

    public void EnableAttackCollider()
    {
        UpdateColliderPosition();
        attackCollider.enabled = true;
    }

    public void DisableAttackCollider() => attackCollider.enabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "PlayerHurtbox") return;
        if (playerHealth == null || Time.time < lastHitTime + enemyConfig.hitCooldown) return;
        DealDamage();
    }

    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (playerHealth == null || playerHealth.IsPlayerInvulnerable()) return;

        // NEW: Calculate damage with critical chance
        var (damage, isCritical) = damageDealer.CalculateDamage();
        AudioManager.Instance.PlaySmallEnemyAttack();
        playerHealth.TakeDamage(damage, isCritical);

        // Worm-specific knockback (unchanged)
        if (enemyConfig.hasDualAttack && !isFirstAttackActive && playerRb != null)
        {
            float direction = enemySprite.flipX ? -1f : 1f;
            Vector2 force = new Vector2(
                enemyConfig.knockbackDirection.x * direction,
                enemyConfig.knockbackDirection.y
            );
            playerRb.AddForce(force * enemyConfig.knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void SetColliderOffset(bool isFirstAttack)
    {
        bool isFacingLeft = enemySprite.flipX;

        if (enemyConfig.hasDualAttack)
        {
            // Worm logic: Use second offsets for the second attack
            transform.localPosition = isFirstAttack
                ? (isFacingLeft ? enemyConfig.leftFacingOffset : enemyConfig.rightFacingOffset)
                : (isFacingLeft ? enemyConfig.secondLeftFacingOffset : enemyConfig.secondRightFacingOffset);
        }
        else
        {
            // Small enemy logic: Always use primary offsets
            transform.localPosition = isFacingLeft ? enemyConfig.leftFacingOffset : enemyConfig.rightFacingOffset;
        }
    }

    public void SetAttackPhase(bool isFirstAttack)
    {
        isFirstAttackActive = isFirstAttack;
        UpdateColliderPosition();
    }

    private void UpdateColliderPosition()
    {
        bool isFacingLeft = enemySprite.flipX;

        if (enemyConfig.hasDualAttack)
        {
            transform.localPosition = isFirstAttackActive
                ? (isFacingLeft ? enemyConfig.leftFacingOffset : enemyConfig.rightFacingOffset)
                : (isFacingLeft ? enemyConfig.secondLeftFacingOffset : enemyConfig.secondRightFacingOffset);
        }
        else
        {
            transform.localPosition = isFacingLeft ? enemyConfig.leftFacingOffset : enemyConfig.rightFacingOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)enemyConfig.rightFacingOffset, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)enemyConfig.leftFacingOffset, 0.1f);
    }
}