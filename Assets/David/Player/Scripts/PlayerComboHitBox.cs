using UnityEngine;
using System.Collections.Generic;

public class PlayerComboHitbox : MonoBehaviour
{
    [Header("Combo Timing")]
    [SerializeField] private float windupDuration = 2.3f; // Combo windup animation length
    [SerializeField] private float hitInterval = 0.18f;   // Time between hits (0.9s/5 hits)

    [Header("Combat Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private GameObject hitParticlePrefab;

    // Components
    private Collider2D hitCollider;
    private DamageDealer damageDealer;
    private AudioManager audioManager;
    private CameraShake cameraShake;

    // State
    private float comboStartTime;
    private float lastHitTime;
    private List<DetectedHit> detectedHits = new List<DetectedHit>();
    private bool externalComboActiveState = false;
    private bool attackPhaseActive = false;

    // Public accessors
    public bool IsComboActive => externalComboActiveState;
    public bool IsComboAttacking => attackPhaseActive;

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
    }

    // Call this when ComboAttackStart becomes true
    public void OnComboStarted()
    {
        comboStartTime = Time.time;
        lastHitTime = comboStartTime;
        externalComboActiveState = true;
        attackPhaseActive = false;
        detectedHits.Clear();
    }

    // Call this when ComboAttackStart becomes false
    public void OnComboEnded()
    {
        externalComboActiveState = false;
        attackPhaseActive = false;
        hitCollider.enabled = false;

        // Process all hits at once
        ProcessAllHits();
        detectedHits.Clear();
    }

    private void Update()
    {
        if (!externalComboActiveState) return;

        float timeSinceComboStart = Time.time - comboStartTime;

        // Check if we should enter attack phase
        if (!attackPhaseActive && timeSinceComboStart >= windupDuration)
        {
            attackPhaseActive = true;
            lastHitTime = Time.time; // Reset for first attack
        }

        // Attack phase logic
        if (attackPhaseActive)
        {
            // Check if time for next hit
            if (Time.time >= lastHitTime + hitInterval)
            {
                AttemptHit();
            }
        }
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

        foreach (var hit in detectedHits)
        {
            // Calculate damage just before applying
            var (damage, isCritical) = damageDealer.CalculateDamage();

            // Apply damage with combo information
            hit.damageable.TakeDamage(damage, isCritical, true, hitCount);

            // Show effects with combo positioning
            CreateComboDamagePopUp(damage, hit.position, hitCount, isCritical, hit.isBoss);

            SpawnHitParticles(hit.position);
            PlayRandomHitSound();

            // Camera shake for criticals
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
        position.y -= 2f;
        Instantiate(hitParticlePrefab, position, Quaternion.identity);
    }
}