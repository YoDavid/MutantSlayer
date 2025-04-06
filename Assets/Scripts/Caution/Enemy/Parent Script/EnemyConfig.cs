using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float walkingRange = 5f;

    [Header("Combat")]
    public float attackRange = 1.5f;

    [Tooltip("Minimum time between attacks.")]
    public float minAttackCooldown = 1f;

    [Tooltip("Maximum time between attacks.")]
    public float maxAttackCooldown = 3f;

    public float attackDelay = 0.3f;
    public float colliderActiveDuration = 0.15f;
    public int attackDamage = 10;
    public float hitCooldown = 0.5f;

    [Header("Damage Effects")]
    public float enemyBlinkDuration = 0.1f;
    public int enemyBlinkCount = 2;
    public Color enemyBlinkColor = new Color(1, 0, 0, 0.5f);

    [Header("Main Collider Offsets")]
    public Vector2 rightFacingColliderOffset = new Vector2(0.5f, 0);
    public Vector2 leftFacingColliderOffset = new Vector2(-0.5f, 0);

    [Header("Attack Collider Offsets")]
    public Vector2 rightFacingOffset = new Vector2(0.5f, 0);
    public Vector2 leftFacingOffset = new Vector2(-0.5f, 0);

    [Header("Enemy Type")]
    public bool isWormEnemy = false;

    [Header("Combat - Second Attack (Worm Only)")]
    public bool hasDualAttack = false;
    public float secondAttackDelay = 0.5f;
    public float secondColliderActiveDuration = 0.2f;
    public Vector2 secondRightFacingOffset = new Vector2(0.5f, -0.3f);
    public Vector2 secondLeftFacingOffset = new Vector2(-0.5f, -0.3f);
    public float knockbackForce = 5f;
    public Vector2 knockbackDirection = new Vector2(1f, 0.3f);

    // Call this when you want a new randomized cooldown
    public float GetRandomAttackCooldown()
    {
        return Random.Range(minAttackCooldown, maxAttackCooldown);
    }
}
