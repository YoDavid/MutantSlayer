using UnityEngine;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }
    public BossState currentState;

    public Transform player;
    public float speed = 2f;

    // Made public so that BossAttackManager can read them.
    public float attackRange = 1.5f;       // Melee attack range
    public float rangedAttackRange = 5f;   // Ranged attack range
    public float walkingRange = 8f;        // Range where the boss starts walking towards the player
    public float health = 100f;

    public Rigidbody2D rb;
    public Animator animator;

    // Timer variables
    [Header("Attack Timer Settings")]
    public float minAttackTime = 1f;
    public float maxAttackTime = 3f;
    public float attackCooldownTimer;

    // Reference to separated behavior scripts
    private BossMovement movement;
    private BossAttackManager attackManager;

    // The isAttacking flag (with helper methods) to prevent overlapping attacks.
    private bool _isAttacking = false;

    void Start()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);

        movement = GetComponent<BossMovement>();
        attackManager = GetComponent<BossAttackManager>();

        Debug.Log("BossAI Initialized. Current State: " + currentState);
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Always flip towards the player
        FlipTowardsPlayer();

        HandleState(distanceToPlayer);
    }

    void HandleState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case BossState.Idle:
                movement.HandleIdleState();
                if (distanceToPlayer < rangedAttackRange && attackCooldownTimer <= 0f)
                {
                    DecideAttack();
                }
                else if (distanceToPlayer < walkingRange && distanceToPlayer >= attackRange)
                {
                    currentState = BossState.Moving;
                }
                break;

            case BossState.Moving:
                movement.HandleMovingState(distanceToPlayer, IsAttacking());
                if (attackCooldownTimer <= 0f)
                {
                    DecideAttack();
                }
                break;

            case BossState.Jumping:
                movement.HandleJumpingState();
                break;

            // In attack states, we do nothing—attack has already been triggered.
            case BossState.AOEAttack:
            case BossState.RangedAttack:
            case BossState.ComboAttack:
                // Already attacking; wait for ResetAttackState to update the state.
                break;
        }
    }

    // DecideAttack determines which attack to perform based on distance and health.
    public void DecideAttack()
    {
        if (IsAttacking()) return; // Prevent spamming attacks

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (health > 50f) // Above 50% health
        {
            if (distanceToPlayer < attackRange)
            {
                currentState = BossState.ComboAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)
            {
                currentState = BossState.RangedAttack;
            }
        }
        else // Below 50% health
        {
            if (distanceToPlayer < attackRange)
            {
                int attackChoice = Random.Range(0, 2);
                currentState = (attackChoice == 0) ? BossState.AOEAttack : BossState.ComboAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)
            {
                int attackChoice = Random.Range(0, 2);
                currentState = (attackChoice == 0) ? BossState.AOEAttack : BossState.RangedAttack;
            }
        }

        UpdateAttackState();
    }

    // UpdateAttackState triggers the proper attack behavior.
    void UpdateAttackState()
    {
        switch (currentState)
        {
            case BossState.AOEAttack:
                attackManager.AOEAttackBehavior();
                break;
            case BossState.RangedAttack:
                attackManager.RangedAttackBehavior();
                break;
            case BossState.ComboAttack:
                attackManager.ComboAttackBehavior();
                break;
            default:
                break;
        }
    }

    void FlipTowardsPlayer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (player.position.x < transform.position.x && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
            Debug.Log("Flipping to Left");
        }
        else if (player.position.x > transform.position.x && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
            Debug.Log("Flipping to Right");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkingRange);
    }

    // Helper methods for the isAttacking flag.
    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public void SetAttacking(bool value)
    {
        _isAttacking = value;
    }
}
