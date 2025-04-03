using UnityEngine;
using System;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private float hitCooldown = 0.3f;
    private float lastHitTime;

    private DamageDealer damageDealer;

    // Events for time control
    public event Action<bool, bool> OnHit; // bool isCritical, bool isBoss

    private void Awake()
    {
        damageDealer = gameObject.AddComponent<DamageDealer>();
        damageDealer.config = damageConfig;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown) return;

        bool isBoss = false;
        bool hitSuccess = false;
        bool isCritical = false;

        // Handle both small enemies and bosses
        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            var (damage, critical) = damageDealer.CalculateDamage();
            enemyHealth.TakeDamage(damage, critical);
            isCritical = critical;
            hitSuccess = true;
        }
        else if (other.TryGetComponent<BossHealth>(out var bossHealth))
        {
            var (damage, critical) = damageDealer.CalculateDamage();
            bossHealth.TakeDamage(damage, critical);
            isCritical = critical;
            isBoss = true;
            hitSuccess = true;
        }

        if (hitSuccess)
        {
            lastHitTime = Time.time;
            OnHit?.Invoke(isCritical, isBoss);
        }
    }
}