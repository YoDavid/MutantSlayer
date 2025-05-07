using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Damage Settings")]
    public DamageConfig damageConfig; // Public for Inspector assignment
    private BossDamageDealer damageDealer; // Changed to BossDamageDealer for consistency

    [Header("Collider References")]
    [SerializeField] private Collider2D spikeCollider;
    [SerializeField] private Collider2D playerHurtBoxCollider;

    [Header("Player Reference")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Damage Scaling")]
    [SerializeField] private BossLevelScaling bossLevelScaling; // NEW
    private int currentBaseDamage;
    private float currentPhaseMultiplier;

    private void Awake()
    {
        damageDealer = gameObject.AddComponent<BossDamageDealer>(); // Create like other attacks

        // Identical scaling logic to BossComboAttackHitbox
        if (bossLevelScaling != null)
        {
            UpdateScaledDamage();
        }
        else
        {
            damageDealer.config = damageConfig;
            Debug.LogWarning("BossLevelScaling not found - using base damage values");
        }
    }

    // Same scaling method as other attacks
    private void UpdateScaledDamage()
    {
        DamageConfig scaledConfig = Instantiate(damageConfig);
        currentPhaseMultiplier = bossLevelScaling.GetPhaseDamageMultiplier();

        scaledConfig.normalDamageRange = new Vector2Int(
            Mathf.RoundToInt(damageConfig.normalDamageRange.x * currentPhaseMultiplier),
            Mathf.RoundToInt(damageConfig.normalDamageRange.y * currentPhaseMultiplier)
        );

        scaledConfig.criticalDamageRange = new Vector2Int(
            Mathf.RoundToInt(damageConfig.criticalDamageRange.x * currentPhaseMultiplier),
            Mathf.RoundToInt(damageConfig.criticalDamageRange.y * currentPhaseMultiplier)
        );

        damageDealer.config = scaledConfig;
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        // Only search if not assigned in Inspector
        if (playerHurtBoxCollider == null)
        {
            GameObject playerHurtbox = GameObject.FindWithTag("PlayerHurtBox");
            if (playerHurtbox != null)
                playerHurtBoxCollider = playerHurtbox.GetComponent<Collider2D>();
        }

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Debug errors if still missing
        if (playerHurtBoxCollider == null)
            Debug.LogError("PlayerHurtbox not found in scene.", this);

        if (playerHealth == null)
            Debug.LogError("PlayerHealth component not found.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerHurtBoxCollider)
        {
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        if (playerHealth != null && !playerHealth.IsPlayerInvulnerable())
        {
            var (damage, isCritical) = damageDealer.CalculateDamage();
            playerHealth.TakeDamage(damage, isCritical);

        }
    }

    public void SetColliderEnabled(bool enabled)
    {
        if (spikeCollider != null)
        {
            spikeCollider.enabled = enabled;
        }
        else
        {
            Debug.LogError("Spike collider not assigned.", this);
        }
    }
}