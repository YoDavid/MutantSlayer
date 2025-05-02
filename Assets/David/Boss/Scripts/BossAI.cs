using System.Collections;
using UnityEngine;

public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }

public class BossAI : MonoBehaviour
{
    [Header("Boss State")]
    public BossState currentState;

    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float desiredDistanceFromPlayer;
    public bool isFacingLeft;

    [Header("Attack Range Definitions")]
    public float attackRange; 
    [SerializeField] private float rangedAttackRange;
    [SerializeField] private float aoeAttackRange; 
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
    private BossAttackManager attackManager;
    private BossComboAttackHitbox bossAttackHitbox;
    private BossHealth bossHealth;
    public Transform player;

    [Header("Debugging")]
    [SerializeField] private bool _isAttacking = false;
    [SerializeField] private bool showGizmos = false;

    [Header("Boss Scream Settings")]
    [SerializeField] private float minScreamDelay = 1f;
    [SerializeField] private float maxScreamDelay = 5f;
    private bool isScreamPlaying = false;
    [SerializeField] private float screamDuration = 1.5f;

    [SerializeField] private float nextScreamTimer;
    private bool isWalkingSoundPlaying = false;



    void Start()
    {
        InitializeValues();
        FindReferences();
        nextScreamTimer = Random.Range(minScreamDelay, maxScreamDelay);
    }

    void InitializeValues()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
        groundCheckRadius = 0.62f;
    }

   void FindReferences()
    {
        bossAttackHitbox = GetComponentInChildren<BossComboAttackHitbox>();
        bossMovement = GetComponent<BossMovement>();
        attackManager = GetComponent<BossAttackManager>();
        bossHealth = GetComponent<BossHealth>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        groundLayer = LayerMask.GetMask("Ground");
        groundCheck = transform.Find("GroundCheckPoint_Boss");
    }

    void Update()
    {
        HandleCooldowns();
        UpdatePlayerDistance();
        UpdateGroundedStatus();
        HandleFlipAndState();

        if (!isScreamPlaying)
        {
            nextScreamTimer -= Time.deltaTime;

            if (nextScreamTimer <= 0f && player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                if (IsPlayerInWalkingRange(distanceToPlayer))
                {
                    StartCoroutine(PlayBossScreamWithCooldown());
                }
                else
                {
                    ResetScreamTimer(); // Reset if player is out of range
                }
            }
        }

        UpdateWalkingSound();
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
        if(player == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
    }

    void UpdateGroundedStatus()
    {
        isGrounded = IsGrounded();
    }

    void HandleFlipAndState()
    {
        FlipTowardsPlayer();
        if(player == null) return;
        HandleState(Vector2.Distance(transform.position, player.position));
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
                    else if (IsPlayerInWalkingRange(distanceToPlayer))
                    {
                        currentState = BossState.Moving;
                    }
                }
                break;

            case BossState.Moving:
                if (!IsAttacking())
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
        }
    }

    void UpdateWalkingSound()
    {
        if (currentState == BossState.Moving && !isWalkingSoundPlaying)
        {
            AudioManager.Instance.PlayBossWalking();
            isWalkingSoundPlaying = true;
        }
        else if (currentState != BossState.Moving && isWalkingSoundPlaying)
        {
            AudioManager.Instance.StopCategory("Boss");  
            isWalkingSoundPlaying = false;
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
            if (bossHealth.currentHealth > 50)
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
        if (player == null || !IsGrounded()) return;

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

        attackManager.UpdateSpitPosition(isFacingLeft);
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

    private void ResetScreamTimer()
    {
        nextScreamTimer = Random.Range(minScreamDelay, maxScreamDelay);
    }

    IEnumerator PlayBossScreamWithCooldown()
    {
        isScreamPlaying = true;

        // Play random scream
        int index = Random.Range(0, 4);
        switch (index)
        {
            case 0: AudioManager.Instance.PlayBossScream_00(); break;
            case 1: AudioManager.Instance.PlayBossScream_01(); break;
            case 2: AudioManager.Instance.PlayBossScream_02(); break;
            case 3: AudioManager.Instance.PlayBossScream_03(); break;
        }

        yield return new WaitForSeconds(screamDuration);

        isScreamPlaying = false;
        ResetScreamTimer(); // Reset timer after scream finishes
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