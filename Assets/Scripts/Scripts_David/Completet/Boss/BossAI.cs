using System.Collections;
using UnityEngine;

public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }

public class BossAI : MonoBehaviour
{
    [Header("Boss State")]
    public BossState currentState;
    public Vector2 startingPosition;

    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] public float desiredDistanceFromPlayer;
    public bool isFacingLeft;

    [Header("Attack Range Definitions")]
    public float attackRange; // Melee attack range
    [SerializeField] private float rangedAttackRange; // Ranged attack range
    [SerializeField] private float aoeAttackRange; // AOE attack range
    [SerializeField] private float walkingRange;
    [SerializeField] private float jumpingAttackRange;

    [Header("Attack Timer Settings")]
    public float minAttackTime;
    public float maxAttackTime;
    public float attackCooldownTimer;

    [Header("Timers For Each Attack Type")]
    public float comboAttackDuration;
    public float rangedAttackDuration;
    public float aoeAttackDuration;

    [Header("Jump Cooldown")]
    [SerializeField] private float jumpCooldownTimer;
    [SerializeField] private float maxJumpCooldown;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    public bool isGrounded;

    [Header("Other References")]
    public Rigidbody2D rb;
    private Animator animator;
    private BossMovement bossMovement;
    private BossComboAttackHitbox bossAttackHitbox;
    private BossHealth bossHealth;
    public Transform player;
    private BossAttackCoordinator attackCoordinator;
    private BossAttackManager bossAttackManager;

    [Header("Debugging")]
    [SerializeField] private bool _isAttacking = false;
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private bool isDebugMode = false;

    void Start()
    {
        InitializeValues();
        FindReferences();
    }

    void InitializeValues()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
        groundCheckRadius = 0.62f;
        startingPosition = transform.position;
    }

    void FindReferences()
    {
        bossAttackHitbox = GetComponentInChildren<BossComboAttackHitbox>();
        bossMovement = GetComponent<BossMovement>();
        bossHealth = GetComponent<BossHealth>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackCoordinator = GetComponent<BossAttackCoordinator>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        groundLayer = LayerMask.GetMask("Ground");
        groundCheck = transform.Find("GroundCheckPoint_Boss");

        if (bossAttackHitbox == null) Debug.LogWarning("BossAttackHitbox not found!");
        if (bossMovement == null) Debug.LogWarning("BossMovement not found!");
        if (attackManager == null) Debug.LogWarning("BossAttackManager not found!");
        if (bossHealth == null) Debug.LogWarning("BossHealth not found!");
        if (animator == null) Debug.LogWarning("Animator not found!");
        if (player == null) Debug.LogWarning("Player not found! Make sure the Player has the correct tag.");
        if (groundCheck == null) Debug.LogWarning("GroundCheckPoint_Boss not found! Make sure it exists in the hierarchy.");
    }

    void Update()
    {
        HandleCooldowns();
        UpdatePlayerDistance();
        UpdateGroundedStatus();
        HandleFlipAndState();

<<<<<<< HEAD
<<<<<<< HEAD
=======
        // Debug key to test attacks without range checks
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
=======
        // Debug key to test attacks without range checks
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isDebugMode)
            {
                DebugAttackBehavior();
            }
        }
    }

    private void DebugAttackBehavior()
    {
<<<<<<< HEAD
<<<<<<< HEAD
        if (!IsAttacking() && isDebugMode)
        {
            attackCoordinator.ExecuteAttack(BossState.AOEAttack);
=======
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
        if (!IsAttacking())
        {
            // Trigger any attack for testing (e.g., AOE attack)
            attackManager.AOEAttackBehavior();
<<<<<<< HEAD
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
        }
    }

    private bool IsPlayerInWalkingRange(float distanceToPlayer)
    {
        return distanceToPlayer < walkingRange && distanceToPlayer > desiredDistanceFromPlayer;
    }

    void HandleCooldowns()
    {
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    void UpdatePlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
    }

    void UpdateGroundedStatus()
    {
        isGrounded = IsGrounded();
    }

    void HandleFlipAndState()
    {
        FlipTowardsPlayer();
        HandleState(Vector2.Distance(transform.position, player.position));
    }

    void HandleState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case BossState.Jumping:
                if (!IsAttacking())
                {
                    attackCoordinator.ExecuteAttack(BossState.Jumping);
                }
                break;

            case BossState.Idle:
                if (!IsAttacking())
                {
                    animator.SetBool("IsWalking", false);
                    if (distanceToPlayer < rangedAttackRange && attackCooldownTimer <= 0f)
                    {
                        DecideAttack();
                    }
<<<<<<< HEAD
<<<<<<< HEAD
                    else if (distanceToPlayer > desiredDistanceFromPlayer)
=======
                    else if (IsPlayerInWalkingRange(distanceToPlayer))
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
=======
                    else if (IsPlayerInWalkingRange(distanceToPlayer))
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
                    {
                        currentState = BossState.Moving;
                    }
                    else
                    {
                        // Player is out of walking range; return to starting position
                        ReturnToStartingPosition();
                    }
                }
                break;

            case BossState.Moving:
                if (!IsAttacking()) // Prevent movement while attacking
                {
                    Vector2 moveDirection = (player.position - transform.position).normalized;
                    rb.velocity = new Vector2(moveDirection.x * speed, rb.velocity.y);
                    animator.SetBool("IsWalking", true);

                    if (distanceToPlayer <= desiredDistanceFromPlayer)
                    {
                        currentState = BossState.Idle;
                    }
<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
                    else if (!IsPlayerInWalkingRange(distanceToPlayer))
                    {
                        // Player is out of walking range; return to starting position
                        ReturnToStartingPosition();
                    }
                    else if (attackCooldownTimer <= 0f)
                    {
                        DecideAttack();
                    }
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
                }
                break;

            case BossState.AOEAttack:
            case BossState.RangedAttack:
            case BossState.ComboAttack:
                StopMovement();
                break;
        }
    }

    private void ReturnToStartingPosition()
    {
        currentState = BossState.Moving;
        bossMovement.StartReturningToStart();
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
                else if (distanceToPlayer < aoeAttackRange && distanceToPlayer < rangedAttackRange)
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
                attackCoordinator.ExecuteAttack(BossState.AOEAttack); // Updated
                StartCoroutine(WaitForAttack(aoeAttackDuration));
                break;
            case BossState.RangedAttack:
                attackCoordinator.ExecuteAttack(BossState.RangedAttack); // Updated
                StartCoroutine(WaitForAttack(rangedAttackDuration));
                break;
            case BossState.ComboAttack:
                attackCoordinator.ExecuteAttack(BossState.ComboAttack); // Updated
                StartCoroutine(WaitForAttack(comboAttackDuration));
                break;
            case BossState.Jumping:
                attackCoordinator.ExecuteAttack(BossState.Jumping); // Updated
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
        if (currentState == BossState.Moving)
        {
            animator.SetBool("IsWalking", true);
        }
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

        if (currentState == BossState.Moving && bossMovement.isReturningToStart)
        {
            // Flip toward the starting position
            if (startingPosition.x < transform.position.x)
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
        else
        {
            // Flip toward the player
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
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius, groundLayer);

        if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, walkingRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeAttackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, jumpingAttackRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, groundCheck.position);

        }
    }
}