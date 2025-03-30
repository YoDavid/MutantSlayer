using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    public EnemyConfig config; // Reference to config

    private Collider2D attackCollider;
    private float lastHitTime;
    private PlayerHealth playerHealth;
    private SpriteRenderer enemySprite;
    [SerializeField] private bool isFirstAttackActive = false; // Tracks if this is the first hit
    private Rigidbody2D playerRb;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        enemySprite = GetComponentInParent<SpriteRenderer>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

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
        if (playerHealth == null || Time.time < lastHitTime + config.hitCooldown) return;
        DealDamage();
    }

    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (playerHealth == null || playerHealth.IsPlayerInvulnerable()) return;

        playerHealth.TakeDamage(config.attackDamage);

        // Worm-specific knockback (unchanged)
        if (config.hasDualAttack && !isFirstAttackActive && playerRb != null)
        {
            float direction = enemySprite.flipX ? -1f : 1f;
            Vector2 force = new Vector2(
                config.knockbackDirection.x * direction,
                config.knockbackDirection.y
            );
            playerRb.AddForce(force * config.knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void SetColliderOffset(bool isFirstAttack)
    {
        bool isFacingLeft = enemySprite.flipX;

        if (config.hasDualAttack)
        {
            // Worm logic: Use second offsets for the second attack
            transform.localPosition = isFirstAttack
                ? (isFacingLeft ? config.leftFacingOffset : config.rightFacingOffset)
                : (isFacingLeft ? config.secondLeftFacingOffset : config.secondRightFacingOffset);
        }
        else
        {
            // Small enemy logic: Always use primary offsets
            transform.localPosition = isFacingLeft ? config.leftFacingOffset : config.rightFacingOffset;
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

        if (config.hasDualAttack)
        {
            transform.localPosition = isFirstAttackActive
                ? (isFacingLeft ? config.leftFacingOffset : config.rightFacingOffset)
                : (isFacingLeft ? config.secondLeftFacingOffset : config.secondRightFacingOffset);
        }
        else
        {
            transform.localPosition = isFacingLeft ? config.leftFacingOffset : config.rightFacingOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)config.rightFacingOffset, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)config.leftFacingOffset, 0.1f);
    }
}