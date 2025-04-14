using UnityEngine;
using System;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private GameObject hitParticlePrefab; // <-- Assign this in the Inspector

    private float lastHitTime;

    private DamageDealer damageDealer;
    private AudioManager audioManager;

    // Events for time control
    public event Action<bool, bool> OnHit; // bool isCritical, bool isBoss

    private void Awake()
    {
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;

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
        }
    }

    private void PlayRandomHitSound()
    {
        if (audioManager == null) return;

        int rand = UnityEngine.Random.Range(1, 5); // 1 to 4
        string clipName = $"slash_hit_0{rand}";
        audioManager.PlaySFX("Player", clipName);
        Debug.Log($"Played: {clipName}");
    }

    private void SpawnHitParticles(Vector3 position)
    {
        if (hitParticlePrefab != null)
        {
            // Adjust the Y position to be 2 units lower
            position.y -= 2f;
            Instantiate(hitParticlePrefab, position, hitParticlePrefab.transform.rotation);
        }
    }

}
