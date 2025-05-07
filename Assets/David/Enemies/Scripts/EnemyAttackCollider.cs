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
            Debug.Log("Added missing EnemyDamageDealer component.");
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
        Debug.Log("Attack Collider Enabled");
    }

    public void DisableAttackCollider()
    {
        attackCollider.enabled = false;
        Debug.Log("Attack Collider Disabled");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "PlayerHurtbox")
        {
            Debug.Log("Trigger Entered: Not PlayerHurtbox");
            return;
        }

        Debug.Log("Trigger Entered: PlayerHurtbox");

        if (playerHealth == null || Time.time < lastHitTime + enemyConfig.hitCooldown)
        {
            Debug.Log("PlayerHealth is null or hitCooldown not passed.");
            return;
        }

        DealDamage();
    }

    private void DealDamage()
    {
        lastHitTime = Time.time;
        if (playerHealth == null || playerHealth.IsPlayerInvulnerable())
        {
            Debug.Log("PlayerHealth is null or player is invulnerable.");
            return;
        }

        // Use the EnemyDamageDealer to calculate the damage
        var damageDealer = GetComponent<EnemyDamageDealer>();  // Ensure the enemy has an EnemyDamageDealer component
        if (damageDealer != null)
        {
            // Pass the player's level to the damage dealer
            damageDealer.SetLevel(playerLevelSystem.progression.level);

            // Calculate damage (and check for critical hit)
            var (damage, isCritical) = damageDealer.CalculateDamage();
            Debug.Log("Calculated damage: " + damage + " (Critical: " + isCritical + ")");

            // Play attack sound
           // PlayAttackSound();

            // Apply damage to player health
            playerHealth.TakeDamage(damage, isCritical);
            Debug.Log("Damage Applied to Player: " + damage + " (Critical: " + isCritical + ")");
        }
        else
        {
            Debug.LogError("DamageDealer component missing!");
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
            Debug.Log("Knockback applied with force: " + force);
        }
    }

    private void PlayAttackSound()
    {
        if (attackAudioSource != null)
        {
            // Play attack sound - you can modify this to select the correct sound based on attack phase or other criteria
            AudioManager.Instance.PlaySmallEnemyAttack(); // Preserving original call for small enemy attacks
            Debug.Log("Playing enemy attack sound.");
        }
        else
        {
            Debug.LogError("AudioSource missing! Cannot play attack sound.");
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
