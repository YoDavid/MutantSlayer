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

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Attack Range Definitions")]
    public float attackRange = 2f; // Melee attack range
    public float rangedAttackRange = 5f; // Ranged attack range
    public float aoeAttackRange = 3f; // AOE attack range
    public float walkingRange = 4f;
    public float jumpingAttackRange = 4f;

    [Header("Attack Timer Settings")]
    public float minAttackTime = 1f;
    public float maxAttackTime = 3f;
    public float attackCooldownTimer;

    [Header("Timers For Each Attack Type")]
    public float comboAttackDuration = 2.3f;
    public float rangedAttackDuration = 1.5f;
    public float aoeAttackDuration = 3.0f;

    [Header("Other References")]
    public Rigidbody2D rb;
    public Animator animator;
    public BossMovement bossMovement;
    private BossAttackManager attackManager;
    private BossAttackHitbox bossAttackHitbox;
    private BossHealth bossHealth;

    [Header("Miscellaneous")]
    public bool _isAttacking = false;
    public bool showGizmos = false;

    [Header("Jump Cooldown")]
    public float jumpCooldownTimer = 0f;  // Timer for jump cooldown
    public float maxJumpCooldown = 10f;  // Max cooldown time for jumps

   

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
        isGrounded = IsGrounded();

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    void HandleState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case BossState.Jumping:
                if (!IsAttacking())
                {
                    attackManager.JumpAttackBehavior();
                }
                break;

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
                StopMovement(); // Ensures boss doesn't move while attacking
                break;
        }
    }

    void DecideAttack()
    {
        if (IsAttacking()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Randomly decide to jump if cooldown is ready
        if (jumpCooldownTimer <= 0 && Random.value < 0.3f)
        {
            currentState = BossState.Jumping;
            jumpCooldownTimer = maxJumpCooldown; // Reset cooldown
        }
        else
        {
            // Decide attack based on health and distance
            if (bossHealth.health > 50)
            {
                if (distanceToPlayer < attackRange)
                {
                    // Within melee range for combo attack
                    currentState = BossState.ComboAttack;
                }
                else if (distanceToPlayer < rangedAttackRange && distanceToPlayer >= attackRange)
                {
                    // Within ranged attack range, but outside melee range
                    currentState = BossState.RangedAttack;
                }
            }
            else  // Boss health is 50 or below
            {
                if (distanceToPlayer < attackRange)
                {
                    // Within melee range for combo attack
                    currentState = BossState.ComboAttack;
                }
                else if (distanceToPlayer < aoeAttackRange && distanceToPlayer >= rangedAttackRange)
                {
                    // Within AOE attack range, but outside ranged attack range
                    currentState = BossState.AOEAttack;
                }
                else if (distanceToPlayer < rangedAttackRange && distanceToPlayer >= aoeAttackRange)
                {
                    // Within ranged attack range
                    currentState = BossState.RangedAttack;
                }
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
                attackManager.JumpAttackBehavior();
                break;
        }
    }

    IEnumerator WaitForAttack(float duration)
    {
        SetAttacking(true);
        yield return new WaitForSeconds(duration);
        SetAttacking(false);
        ResumeMovement();
        currentState = BossState.Moving;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
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
        if (player == null || !IsGrounded()) return;  // Don't flip if not grounded or player is null

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

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);

        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!isGrounded) // Transitioning to grounded state
            {
                attackManager.isJumping = false;  // Stop jumping flag when grounded
            }
            return true;  // Return true when hitting the ground
        }
        else
        {
            if (isGrounded) // If no longer grounded
            {
                attackManager.isJumping = true; // Set jumping state
            }
            return false;  // Return false when not hitting the ground
        }
    }

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

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); // Draw ground check area
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, groundCheck.position); // Draw a line to visualize the check
        }
    }

 
}
