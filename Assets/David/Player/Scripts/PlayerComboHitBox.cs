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
    private AudioManager audioManager;
    private CameraShake cameraShake;
    private PlayerHealth playerHealth;
    private PlayerMovementController playerMovement;
    private PlayerAnimationController playerAnimation;

    // State
    private float comboStartTime;
    private float lastHitTime;
    private List<DetectedHit> detectedHits = new List<DetectedHit>();
    private bool externalComboActiveState = false;
    private bool attackPhaseActive = false;

    // Public accessors
    public bool IsComboActive => externalComboActiveState;
    public bool IsComboAttacking => attackPhaseActive;

    private bool isComboSoundPlaying = false;
    [SerializeField][Range(0f, 1f)] private float timeSlowFactor = 0.2f; // 0 = freeze, 1 = normal speed

    private class DetectedHit
    {
        public Vector3 position;
        public bool isBoss;
        public IDamageable damageable; // Interface reference
    }

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        hitCollider.enabled = false;

        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

        cameraShake = FindObjectOfType<CameraShake>();
        audioManager = AudioManager.Instance;

        playerHealth = GetComponentInParent<PlayerHealth>();
        playerMovement = GetComponentInParent<PlayerMovementController>();
        playerAnimation = GetComponentInParent<PlayerAnimationController>();
    }

    private void Update()
    {

        if (!externalComboActiveState) return;

        float timeSinceComboStart = Time.time - comboStartTime;

        if (playerMovement != null && !playerMovement.isGrounded)
        {
            playerAnimation.ResetAirborneActions();
            return;
        }

        // Check if we should enter attack phase (after windup)
        if (!attackPhaseActive && timeSinceComboStart >= windupDuration)
        {
            attackPhaseActive = true;
            lastHitTime = Time.time; // Reset for first attack

            // Start combo slash loop sound after windup
            if (!isComboSoundPlaying && audioManager != null)
            {
                playerHealth.isInvulnerable = true;
                audioManager.PlayComboSlashLoop();
                isComboSoundPlaying = true;
            }
        }

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
        // Only start combo if player is grounded
        if (playerMovement != null && !playerMovement.isGrounded)
        {
            return; // Exit if not grounded
        }

        comboStartTime = Time.time;
        lastHitTime = comboStartTime;
        externalComboActiveState = true;
        attackPhaseActive = false;
        detectedHits.Clear();
    }

    public void OnComboEnded()
    {
        // Only freeze time if we actually hit something
        if (detectedHits.Count > 0)
        {
            StartCoroutine(TimeStopEffect());
        }

        // Deactivate combo and attack phase
        externalComboActiveState = false;
        attackPhaseActive = false;
        hitCollider.enabled = false;

        // Stop combo slash sound
        StopComboSlashSound();

        // Process all the hits detected during the combo
        ProcessAllHits();
        detectedHits.Clear();

        // Play the final big slash hit sound (only if we hit something)
        if (detectedHits.Count > 0 && audioManager != null)
        {
            audioManager.PlayComboFinalHit();
        }

        // Reset combo sound flag
        isComboSoundPlaying = false;
        playerHealth.isInvulnerable = false;
    }

    private void StopComboSlashSound()
    {
        if (audioManager != null)
        {
            // Stop the combo slash loop sound
            audioManager.StopSound("PlayerOthers", "sfx_player_combo_slash_loop");
        }
    }

    private IEnumerator TimeStopEffect()
    {
        Time.timeScale = timeSlowFactor;
        yield return new WaitForSecondsRealtime(timeStopDuration); // Wait for the real-time duration
        Time.timeScale = 1f;
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
            var (damage, isCritical) = damageDealer.CalculateDamage();

            hit.damageable.TakeDamage(damage, isCritical, true, hitCount);

            CreateComboDamagePopUp(damage, hit.position, hitCount, isCritical, hit.isBoss);

            if (spawnCount < 3) // Only instantiate up to 3 times
            {
                SpawnHitParticles(hit.position);
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
        if (audioManager == null) return;
        int rand = Random.Range(0, 4);
        audioManager.PlaySFX("Player", $"sfx_player_attack_hit_0{rand}");
    }

    private void SpawnHitParticles(Vector3 position)
    {
        if (hitParticlePrefab == null) return;

        // Random small offset
        float randomX = Random.Range(-0.3f, 0.3f);
        float randomY = Random.Range(-0.1f, 0.1f);

        Vector3 spawnPosition = position + new Vector3(randomX, randomY - 2f, 0f);

        Instantiate(hitParticlePrefab, spawnPosition, Quaternion.identity);
    }
}