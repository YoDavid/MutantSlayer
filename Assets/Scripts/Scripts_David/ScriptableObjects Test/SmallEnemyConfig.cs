using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Enemy Config")]
public class SmallEnemyConfig : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float walkingRange = 5f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public float attackDelay = 0.3f;
    public float colliderActiveDuration = 0.15f;
    public int attackDamage = 10;
    public float hitCooldown = 0.5f;

    [Header("Damage Effects")]
    public float enemyBlinkDuration = 0.1f;
    public int enemyBlinkCount = 2;
    public Color enemyBlinkColor = new Color(1, 0, 0, 0.5f); // Red semi-transparent by default

    [Header("Collider Offsets")]
    public Vector2 rightFacingOffset = new Vector2(0.5f, 0);
    public Vector2 leftFacingOffset = new Vector2(-0.5f, 0);
}