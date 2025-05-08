using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    public EnemyConfig enemyConfig; // Reference to config
    public DamageConfig damageConfig; // For damage ranges

    private Collider2D attackCollider;
    private float lastHitTime;
    private PlayerHealth playerHealth;
    private SpriteRenderer enemySprite;
    [SerializeField] private bool isFirstAttackActive = false;
    private Rigidbody2D playerRb;
    private DamageDealer damageDealer; // Handles damage calculations
    [SerializeField] private PlayerLevelSystem playerLevelSystem;

    private AudioSource attackAudioSource; // NEW: AudioSource for attack sounds

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        enemySprite = GetComponentInParent<SpriteRenderer>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

        // Check if the EnemyDamageDealer is attached, if not, add it.
        damageDealer = GetComponent<EnemyDamageDealer>();
        if (damageDealer == null)
        {
            damageDealer = gameObject.AddComponent<EnemyDamageDealer>();
        }

        damageDealer.config = damageConfig; // Ensure the DamageConfig is assigned correctly

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

    public void DisableAttackCollider()
    {
        attackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "PlayerHurtbox")
        {
            return;
        }


        if (playerHealth == null || Time.time < lastHitTime + enemyConfig.hitCooldown)
        {
            return;
        }

        DealDamage();
    }

    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (playerHealth == null || playerHealth.IsPlayerInvulnerable())
        {
            return;
        }

        // Use the EnemyDamageDealer to calculate the damage
        var damageDealer = GetComponent<EnemyDamageDealer>();
        if (damageDealer != null)
        {
            // Pass the player's level to the damage dealer
            damageDealer.SetLevel(playerLevelSystem.progression.level);

            // Calculate base damage (and check for critical hit)
            var (baseDamage, isCritical) = damageDealer.CalculateDamage();

            // Apply random variation (±20%)
            float randomVariation = Random.Range(-0.2f, 0.2f);
            int finalDamage = Mathf.RoundToInt(baseDamage * (1f + randomVariation));


            // Play attack sound
            PlayAttackSound();

            // Apply damage to player health
            playerHealth.TakeDamage(finalDamage, isCritical);
        }


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

    private void PlayAttackSound()
    {
            AudioManager.Instance.PlaySmallEnemyAttack(); // Preserving original call for small enemy attacks
  
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
