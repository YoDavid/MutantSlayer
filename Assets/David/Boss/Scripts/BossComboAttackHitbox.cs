    using System.Collections;
using UnityEngine;


public class BossComboAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    public DamageConfig damageConfig;
    private BossDamageDealer damageDealer;
    [SerializeField] private float attackDuration;
    [SerializeField] private float[] attackTimings;

    [Header("Collider Settings")]
    [SerializeField] private float colliderShift;
    [SerializeField] private Collider2D attackCollider;
    private Vector2 originalOffset;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;
    private bool isPlayerInRange = false;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    [SerializeField] private GameObject rockParticlesPrefab;
    [SerializeField] private FallingSpikeSpawner fallingSpikeSpawner;

    [Header("Damage Scaling")]
    [SerializeField] private BossLevelScaling bossLevelScaling;
    private int currentBaseDamage;
    private float currentPhaseMultiplier;

    private void Awake()
    {
        InitializeComponents();
        damageDealer = gameObject.AddComponent<BossDamageDealer>();

        // Initialize damage config with scaling
        if (bossLevelScaling != null)
        {
            UpdateScaledDamage();
        }
        else
        {
            damageDealer.config = damageConfig; // Let BossDamageDealer handle scaling
            Debug.LogWarning("BossLevelScaling not found - using base damage values");
        }

        fallingSpikeSpawner = FindAnyObjectByType<FallingSpikeSpawner>();
    }

    private void UpdateScaledDamage()
    {
        // Create a new instance of DamageConfig to avoid modifying the original asset
        DamageConfig scaledConfig = Instantiate(damageConfig);

        currentBaseDamage = bossLevelScaling.GetBaseDamage();
        currentPhaseMultiplier = bossLevelScaling.GetPhaseDamageMultiplier();

        // Apply scaling to damage ranges (using Vector2Int as in your config)
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
        SetupCollider();
        FindPlayerReferences();
    }

    private void InitializeComponents()
    {
        attackCollider = GetComponent<Collider2D>();
        cameraShake = FindAnyObjectByType<CameraShake>();
    }

    private void SetupCollider()
    {
        attackCollider.enabled = false;

        if (attackCollider is BoxCollider2D boxCollider)
        {
            originalOffset = boxCollider.offset;
        }
    }

    private void FindPlayerReferences()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }

        // If not assigned in Inspector, try to find automatically
        if (playerHurtBoxCollider == null)
        {
            GameObject hurtbox = GameObject.FindWithTag("PlayerHurtBox"); // Give your hurtbox this tag
            if (hurtbox != null) playerHurtBoxCollider = hurtbox.GetComponent<Collider2D>();
        }

        // Final validation
        if (playerHurtBoxCollider == null)
        {
            Debug.LogError("Player hurtbox collider not assigned or found!");
        }
    }

    public void ActivateComboAttackCollider()
    {
        StartCoroutine(ActivateComboWithIntervals());
    }

    private IEnumerator ActivateComboWithIntervals()
    {
        float startTime = Time.time;

        for (int i = 0; i < attackTimings.Length; i++)
        {
            float attackTime = attackTimings[i];
            float waitTime = attackTime - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            EnableCollider();

            AudioManager.Instance.PlayComboAttackBoss();

            if (rockParticlesPrefab != null)
            {
                Vector2 spawnPos = new Vector2(attackCollider.bounds.center.x, attackCollider.bounds.min.y + 2f);
                Instantiate(rockParticlesPrefab, spawnPos, Quaternion.identity);
            }

            // Check if this is the last attack in the combo
            if (i == attackTimings.Length - 1 && fallingSpikeSpawner != null)
            {
                fallingSpikeSpawner.SpawnSpikes2(); // Spawn two spikes
            }

            cameraShake.ShakeCameraComboAttack();
            yield return new WaitForSeconds(attackDuration);
            DisableCollider();
        }
    }


    private void EnableCollider()
    {
        attackCollider.enabled = true;
    }

    private void DisableCollider()
    {
        attackCollider.enabled = false;
    }

    private void ApplyDamage()
    {
        if (isPlayerInRange && !playerHealth.IsPlayerInvulnerable())
        {
            // Damage is now automatically scaled through the DamageDealer
            var (damage, isCritical) = damageDealer.CalculateDamage();
            playerHealth.TakeDamage(damage, isCritical);

            // Apply critical effects if needed
            if (isCritical)
            {
                cameraShake?.CriticalHitShakeCamera(); // Stronger shake for crits
            }
            else
            {
                cameraShake?.ShakeCameraComboAttack();
            }
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
            boxCollider.offset = isFlipped
                ? new Vector2(originalOffset.x + colliderShift, originalOffset.y)
                : originalOffset;
        }
    }
}
