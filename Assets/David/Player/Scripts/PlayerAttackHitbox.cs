using System;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig; // The configuration for damage
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private BloodSplashParticlesPool bloodSplashPool;

    private float lastHitTime;

    private PlayerLevelSystem playerLevelSystem; // Reference to PlayerLevelSystem
    private AudioManager audioManager;
    private CameraShake camerShake;

    // Events for time control
    public event Action<bool, bool> OnHit; // bool isCritical, bool isBoss

    private void Awake()
    {
        playerLevelSystem = GetComponentInParent<PlayerLevelSystem>(); // Assuming it's on the parent GameObject

        if (playerLevelSystem == null)
        {
            Debug.LogError("PlayerLevelSystem not found!");
        }

        camerShake = GameObject.FindObjectOfType<CameraShake>();

        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown) return;

        bool isBoss = false;
        bool hitSuccess = false;
        bool isCritical = false;
        Vector3 hitPosition = transform.position; // Default fallback

        // Get scaled damage from PlayerLevelSystem
        int damage = playerLevelSystem.GetScaledDamage("normal");
        float critChance = playerLevelSystem.GetCritChance();
        float critMultiplier = playerLevelSystem.GetCritMultiplier();

        // Check if we hit an enemy
        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            isCritical = UnityEngine.Random.value <= critChance; // Explicitly use UnityEngine.Random

            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * critMultiplier);
            }

            enemyHealth.TakeDamage(damage, isCritical);
            hitSuccess = true;
            hitPosition = other.transform.position;
        }
        // Check if we hit a boss
        else if (other.TryGetComponent<BossHealth>(out var bossHealth))
        {
            isCritical = UnityEngine.Random.value <= critChance; // Explicitly use UnityEngine.Random

            if (isCritical)
            {
                damage = Mathf.RoundToInt(damage * critMultiplier);
            }

            bossHealth.TakeDamage(damage, isCritical);
            isBoss = true;
            hitSuccess = true;
            hitPosition = other.transform.position;
        }

        if (hitSuccess)
        {
            lastHitTime = Time.time;
            PlayRandomHitSound();
            SpawnHitParticles(hitPosition);
            OnHit?.Invoke(isCritical, isBoss);

            if (camerShake != null)
            {
                if (isCritical)
                {
                    camerShake.CriticalHitShakeCamera(); // Stronger shake for crits
                }
                else
                {
                    camerShake.NormalHitShakeCamera(); // Normal shake for regular hits
                }
            }
        }
    }

    private void PlayRandomHitSound()
    {
        if (audioManager == null) return;

        int rand = UnityEngine.Random.Range(0, 4); // Explicitly use UnityEngine.Random
        string clipName = $"sfx_player_attack_hit_0{rand}";
        audioManager.PlaySFX("Player", clipName);
    }

    private void SpawnHitParticles(Vector3 position)
    {
        if (bloodSplashPool != null)
        {
            position.y -= 2f;
            bloodSplashPool.PlayHitSplash(position);
        }
    }
}
