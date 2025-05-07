using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerComboHitbox : MonoBehaviour
{
    [Header("Combo Timing")]
    [SerializeField] public float windupDuration = 2.3f; // Combo windup animation length
    [SerializeField] private float hitInterval = 0.18f;   // Time between hits (0.9s/5 hits)

    [Header("Time Stop Effect")]
    [SerializeField] private float timeStopDuration = 0.3f; // Duration for time stop after combo ends

    [Header("Combat Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private GameObject hitParticlePrefab;

    // Components
    private Collider2D hitCollider;
    private DamageDealer damageDealer;
    private CameraShake cameraShake;
    private PlayerHealth playerHealth;
    private PlayerMovementController playerMovement;
    private PlayerAnimationController playerAnimation;
    private BloodSplashParticlesPool bloodSplashParticlesPool;

    [SerializeField] private PlayerLevelSystem playerLevelSystem;

    // State
    private float comboStartTime;
    private float lastHitTime;
    [SerializeField] private List<DetectedHit> detectedHits = new List<DetectedHit>();
    private bool externalComboActiveState = false;
    [SerializeField] private bool attackPhaseActive = false;

    // Public accessors
    public bool IsComboActive => externalComboActiveState;
    public bool IsComboAttacking => attackPhaseActive;

    private bool isComboSoundPlaying = false;
    [SerializeField][Range(0f, 1f)] private float timeSlowFactor = 0.2f; // 0 = freeze, 1 = normal speed

    [System.Serializable] // Add this attribute
    public class DetectedHit
    {
        public Vector3 position;
        public bool isBoss;
        // Note: IDamageable won't show in inspector as it's an interface
        // You might want to add a GameObject reference for visualization
        [System.NonSerialized] public IDamageable damageable; // Mark as non-serialized
    }

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        hitCollider.enabled = false;

        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

        cameraShake = FindObjectOfType<CameraShake>();
        bloodSplashParticlesPool = FindObjectOfType<BloodSplashParticlesPool>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        playerMovement = GetComponentInParent<PlayerMovementController>();
        playerAnimation = GetComponentInParent<PlayerAnimationController>();
    }

    private void Update()
    {
        // Skip if combo isn't active
        if (!externalComboActiveState) return;

        float timeSinceComboStart = Time.time - comboStartTime;

        // Reset actions if the player isn't grounded
        if (playerMovement != null && !playerMovement.isGrounded)
        {
            playerAnimation.ResetAirborneActions();
            return;
        }

        // Step 4: Check for the attack phase after windup
        if (!attackPhaseActive && timeSinceComboStart >= windupDuration)
        {
            attackPhaseActive = true;
            lastHitTime = Time.time;

            // Debug for attack phase

            if (!isComboSoundPlaying)
            {
                playerHealth.SetComboInvulnerability(true);
                AudioManager.Instance.PlayComboSlashLoop();
                isComboSoundPlaying = true;
            }
        }

        // Step 5: Handle attacks in the active phase
        if (attackPhaseActive)
        {
            if (Time.time >= lastHitTime + hitInterval)
            {
                AttemptHit();
            }
        }
    }


    public void OnComboStarted()
    {
        if (playerMovement != null && !playerMovement.isGrounded)
            return;

        comboStartTime = Time.time;
        lastHitTime = comboStartTime;
        externalComboActiveState = true;
        attackPhaseActive = false;
        detectedHits.Clear();

    }


    public void OnComboEnded()
    {
        if (detectedHits.Count > 0)
            StartCoroutine(TimeStopEffect());

        externalComboActiveState = false;
        attackPhaseActive = false;
        hitCollider.enabled = false;

        StopComboSlashSound();
        ProcessAllHits();
        detectedHits.Clear();


        isComboSoundPlaying = false;

        playerHealth.SetComboInvulnerability(false);
    }


    private void StopComboSlashSound()
    {
        if (detectedHits.Count > 0)
        {
            AudioManager.Instance.PlayComboFinalHit();
        }
        AudioManager.Instance.StopSound("PlayerOthers", "sfx_player_combo_slash_loop");

    }

    private IEnumerator TimeStopEffect()
    {
        TimeManager.Instance?.SetTimeScale(timeSlowFactor);
        yield return new WaitForSecondsRealtime(timeStopDuration);
        TimeManager.Instance?.ResetTimeScale();
    }

    private void AttemptHit()
    {
        lastHitTime = Time.time;
        hitCollider.enabled = true;
        Invoke(nameof(DisableCollider), Time.fixedDeltaTime);
    }

    private void DisableCollider()
    {
        hitCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!attackPhaseActive) return;

        bool isBoss = other.CompareTag("BossEnemy");
        if (!other.CompareTag("Enemy") && !isBoss) return;

        // Only store references - no damage calculation yet
        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            detectedHits.Add(new DetectedHit
            {
                position = other.transform.position,
                isBoss = isBoss,
                damageable = damageable
            });

            // Immediate visual feedback only
            if (cameraShake != null)
            {
                cameraShake.NormalHitShakeCamera();
            }
        }
    }


    private void ProcessAllHits()
    {
        int hitCount = 0;
        int spawnCount = 0; // Track how many prefabs spawned

        foreach (var hit in detectedHits)
        {
            // Retrieve scaled damage from PlayerLevelSystem
            int damage = playerLevelSystem.GetScaledDamage("normal");  // Use appropriate attack type (normal/charged/etc.)

            var (calculatedDamage, isCritical) = damageDealer.CalculateDamage();

            hit.damageable.TakeDamage(damage, isCritical, true, hitCount);

            CreateComboDamagePopUp(damage, hit.position, hitCount, isCritical, hit.isBoss);

            if (spawnCount < 3) // Only instantiate up to 3 times
            {
                bloodSplashParticlesPool.PlayHitSplash(hit.position);
                spawnCount++;
            }

            if (isCritical && cameraShake != null)
            {
                cameraShake.CriticalHitShakeCamera();
            }

            hitCount++;
        }
    }


    private void CreateComboDamagePopUp(int damage, Vector3 position, int hitIndex, bool isCritical, bool isBoss)
    {
        if (DamagePopUp.Instance == null) return;

        // Calculate offset based on hit index
        float xOffset = hitIndex * 0.5f; // Each number will be spaced 0.5 units apart
        float yOffset = hitIndex * 0.3f;  // Each number will be slightly higher

        Vector3 popUpPosition = position + new Vector3(xOffset, yOffset, 0);

        DamagePopUp.Instance.CreateDamageText(
            damage,
            popUpPosition,
            isPlayer: true,
            isBoss: isBoss,
            isCritical,
            isCombo: true,
            comboIndex: hitIndex);
    }

    private void PlayRandomHitSound()
    {
        int rand = Random.Range(0, 4);
        AudioManager.Instance.PlaySFX("Player", $"sfx_player_attack_hit_0{rand}");
    }


}