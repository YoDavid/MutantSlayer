using UnityEngine;

public class SmallEnemyAttackCollider : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float hitCooldown = 0.5f;

    [Header("Collider Positioning")]
    [SerializeField] private Vector2 rightFacingOffset = new Vector2(0.5f, 0);
    [SerializeField] private Vector2 leftFacingOffset = new Vector2(-0.5f, 0);
    [SerializeField] private bool autoFlipWithEnemy = true;

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
        if (!autoFlipWithEnemy) return;

        bool isFacingLeft = enemySprite.flipX;
        transform.localPosition = isFacingLeft ? leftFacingOffset : rightFacingOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "PlayerHurtbox") return;
        if (playerHealth == null || Time.time < lastHitTime + hitCooldown) return;

        DealDamage();
    }

    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (!playerHealth.IsPlayerInvulnerable()) playerHealth.TakeDamage(attackDamage);
    }

    // For visual debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)rightFacingOffset, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.parent.position + (Vector3)leftFacingOffset, 0.1f);
    }
}