using System.Collections;
using UnityEngine;

public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }

public class BossAI : MonoBehaviour
{
    
    public BossState currentState;

    public Transform player;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float desiredDistanceFromPlayer = 1f;
    public bool isFacingLeft;

    [Header("Attack Timer Settings")]
    public float minAttackTime = 1f;
    public float maxAttackTime = 3f;
    public float attackCooldownTimer;

    [Header("Attack Range Definitions")]
    public float attackRange = 2f; // Melee attack range
    public float rangedAttackRange = 5f; // Ranged attack range
    public float aoeAttackRange = 3f; // AOE attack range
    public float walkingRange = 4f;
    public float jumpingAttackRange = 4f;

    [Header("Timers For Each Attack Type")]
    public float comboAttackDuration = 2.3f;
    public float rangedAttackDuration = 1.5f;
    public float aoeAttackDuration = 3.0f;
    public float jumpiAttackDuration = 2.0f; // New timer for jumping attack

    public Rigidbody2D rb;
    public Animator animator;
    public BossMovement bossMovement;
    private BossAttackManager attackManager;
    private BossAttackHitbox bossAttackHitbox;
    private BossHealth bossHealth;

    private bool _isAttacking = false;

    public bool showGizmos = false;

    void Start()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
        bossAttackHitbox = GetComponentInChildren<BossAttackHitbox>();
        bossMovement = GetComponent<BossMovement>();
        attackManager = GetComponent<BossAttackManager>();
        bossHealth = GetComponent<BossHealth>();
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        FlipTowardsPlayer();
        HandleState(distanceToPlayer);
    }

    void HandleState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case BossState.Idle:
                if (!IsAttacking())
                {
                    bossMovement.HandleIdleState();
                    if (distanceToPlayer < rangedAttackRange && attackCooldownTimer <= 0f)
                    {
                        DecideAttack();
                    }
                    else if (distanceToPlayer < walkingRange && distanceToPlayer > desiredDistanceFromPlayer)
                    {
                        currentState = BossState.Moving;
                    }
                }
                break;

            case BossState.Moving:
                if (!IsAttacking()) // Prevent movement while attacking
                {
                    bossMovement.HandleMovingState(distanceToPlayer, desiredDistanceFromPlayer, IsAttacking());
                    if (distanceToPlayer <= desiredDistanceFromPlayer)
                    {
                        currentState = BossState.Idle;
                    }
                    else if (attackCooldownTimer <= 0f)
                    {
                        DecideAttack();
                    }
                }
                break;

            case BossState.AOEAttack:
            case BossState.RangedAttack:
            case BossState.ComboAttack:
            case BossState.Jumping:  // Added Jumping to stop movement
                StopMovement(); // Ensures boss doesn't move while attacking
                break;
        }
    }

    void DecideAttack()
    {
        if (IsAttacking()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (bossHealth.health > 50)
        {
            // Above 50% health
            if (distanceToPlayer < attackRange)
            {
                currentState = Random.Range(0, 2) == 0 ? BossState.ComboAttack : BossState.RangedAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)
            {
                currentState = BossState.RangedAttack;
            }
            else if (distanceToPlayer < jumpingAttackRange)  // Add a condition for the jumping attack
            {
                currentState = BossState.Jumping;  // Trigger the Jumping attack state
            }
        }
        else
        {
            // Below 50% health
            if (distanceToPlayer < attackRange)
            {
                currentState = (Random.Range(0, 3) == 0) ? BossState.ComboAttack : (Random.Range(0, 2) == 0 ? BossState.RangedAttack : BossState.AOEAttack);
            }
            else if (distanceToPlayer < aoeAttackRange)
            {
                currentState = (Random.Range(0, 2) == 0) ? BossState.AOEAttack : BossState.RangedAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)
            {
                currentState = BossState.RangedAttack;
            }
        }

        UpdateAttackState();
    }

    void UpdateAttackState()
    {
        switch (currentState)
        {
            case BossState.AOEAttack:
                attackManager.AOEAttackBehavior();
                StartCoroutine(WaitForAttack(aoeAttackDuration));
                break;
            case BossState.RangedAttack:
                attackManager.RangedAttackBehavior();
                StartCoroutine(WaitForAttack(rangedAttackDuration));
                break;
            case BossState.ComboAttack:
                attackManager.ComboAttackBehavior();
                StartCoroutine(WaitForAttack(comboAttackDuration));
                break;
            case BossState.Jumping:
                bossMovement.TriggerJump();
                StartCoroutine(WaitForAttack(jumpiAttackDuration));
                break;
        }
    }

    IEnumerator WaitForAttack(float duration)
    {
        SetAttacking(true); // Start the attack state
        yield return new WaitForSeconds(duration); // Wait for attack to complete
        SetAttacking(false); // End the attack state
        ResumeMovement();
        currentState = BossState.Moving; // Transition to moving state
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime); // Reset cooldown timer
    }

    public void StopMovement()
    {
        // Stop horizontal movement only, but allow vertical velocity (jumping) to continue if the boss is not jumping
        if (currentState != BossState.Jumping)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal velocity only
        }
        animator.SetBool("IsWalking", false);
    }

    public void ResumeMovement()
    {
        bossMovement.enabled = true;
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public void SetAttacking(bool value)
    {
        _isAttacking = value;
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false;
            isFacingLeft = true;
            bossAttackHitbox.FlipCollider(false);
        }
        else
        {
            spriteRenderer.flipX = true;
            isFacingLeft = false;
            bossAttackHitbox.FlipCollider(true);
        }
    }

    // Modified OnDrawGizmos to check for showGizmos flag
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            // Draw attack range gizmos
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, walkingRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeAttackRange);

            // New Gizmo for Jumping Attack Distance
            Gizmos.color = Color.cyan;  // You can change the color here
            Gizmos.DrawWireSphere(transform.position, jumpingAttackRange); // Jumping attack distance
        }
    }
}
