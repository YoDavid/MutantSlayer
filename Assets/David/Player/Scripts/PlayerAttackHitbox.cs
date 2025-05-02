using UnityEngine;
using System;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private BloodSplashParticlesPool bloodSplashPool;

    private float lastHitTime;

    private DamageDealer damageDealer;
    private AudioManager audioManager;
    private CameraShake camerShake;

    // Events for time control
    public event Action<bool, bool> OnHit; // bool isCritical, bool isBoss

    private void Awake()
    {
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

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

        // Handle both small enemies and bosses
        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            var (damage, critical) = damageDealer.CalculateDamage();
            enemyHealth.TakeDamage(damage, critical);
            isCritical = critical;
            hitSuccess = true;
            hitPosition = other.transform.position;
        }
        else if (other.TryGetComponent<BossHealth>(out var bossHealth))
        {
            var (damage, critical) = damageDealer.CalculateDamage();
            bossHealth.TakeDamage(damage, critical);
            isCritical = critical;
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

        int rand = UnityEngine.Random.Range(0, 4); // 1 to 4
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
