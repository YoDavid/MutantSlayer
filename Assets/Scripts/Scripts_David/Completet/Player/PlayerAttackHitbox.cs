using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private float lastHitTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < lastHitTime + hitCooldown) return;

        // Handle both small enemies and bosses
        if (other.TryGetComponent<SmallEnemyHealth>(out var enemyHealth))
        {
            enemyHealth.TakeDamage(baseDamage);
            lastHitTime = Time.time;
        }
        else if (other.TryGetComponent<BossHealth>(out var bossHealth))
        {
            bossHealth.TakeDamage(baseDamage);
            lastHitTime = Time.time;
        }
    }
}