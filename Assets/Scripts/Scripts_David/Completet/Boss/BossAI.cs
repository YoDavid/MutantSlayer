using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }
    public BossState currentState;

    public Transform player;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float desiredDistanceFromPlayer = 1f;
    public bool isFacingLeft; // Regular public flag

    [Header("Attack Timer Settings")]
    public float minAttackTime = 1f;
    public float maxAttackTime = 3f;
    public float attackCooldownTimer;

    [Header("Attack Range Definitions")]
    public float attackRange = 2f; // Melee attack range
    public float rangedAttackRange = 5f; // Ranged attack range
    public float aoeAttackRange = 3f; // AOE attack range
    public float walkingRange = 4f;

    [Header("Timers For Each Attack Type")]
    public float comboAttackDuration = 2.3f; // Combo Attack Duration
    public float rangedAttackDuration = 1.5f; // Ranged Attack Duration
    public float aoeAttackDuration = 3.0f;  // AOE Attack Duration


    public Rigidbody2D rb;
    public Animator animator;
    private BossMovement movement;
    private BossAttackManager attackManager;
    private BossAttackHitbox bossAttackHitbox;
    private BossHealth bossHealth;

    private bool _isAttacking = false;

    void Start()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
        bossAttackHitbox = GetComponentInChildren<BossAttackHitbox>();
        movement = GetComponent<BossMovement>();
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
                    movement.HandleIdleState();
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
                    movement.HandleMovingState(distanceToPlayer, desiredDistanceFromPlayer, IsAttacking());
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

        if (bossHealth.health > 50)
        {
            // Above 50% health
            if (distanceToPlayer < attackRange)  // Red Gizmo: Close range
            {
                currentState = Random.Range(0, 2) == 0 ? BossState.ComboAttack : BossState.RangedAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)  // Blue Gizmo: Mid range
            {
                currentState = BossState.RangedAttack;
            }
        }
        else
        {
            // Below 50% health
            if (distanceToPlayer < attackRange)  // Red Gizmo: Close range
            {
                currentState = (Random.Range(0, 3) == 0) ? BossState.ComboAttack : (Random.Range(0, 2) == 0 ? BossState.RangedAttack : BossState.AOEAttack);
            }
            else if (distanceToPlayer < aoeAttackRange)  // Yellow Gizmo: Close-mid range
            {
                currentState = (Random.Range(0, 2) == 0) ? BossState.AOEAttack : BossState.RangedAttack;
            }
            else if (distanceToPlayer < rangedAttackRange)  // Blue Gizmo: Mid range
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
                StartCoroutine(WaitForAttack(aoeAttackDuration));  // Wait for AOE attack to complete
                break;
            case BossState.RangedAttack:
                attackManager.RangedAttackBehavior();
                StartCoroutine(WaitForAttack(rangedAttackDuration));  // Wait for Ranged attack to complete
                break;
            case BossState.ComboAttack:
                attackManager.ComboAttackBehavior();
                StartCoroutine(WaitForAttack(comboAttackDuration));  // Wait for Combo attack to complete
                break;
        }
    }

    IEnumerator WaitForAttack(float duration)
    {
        yield return new WaitForSeconds(duration);  // Wait for the attack animation to finish
        SetAttacking(false);
        ResumeMovement(); // Allow movement again after attack
        currentState = BossState.Moving;  // Transition to Moving or Idle depending on the situation
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);  // Reset the attack cooldown
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        movement.enabled = false;
    }

    public void ResumeMovement()
    {
        movement.enabled = true;
    }

    public bool IsAttacking()
    {
        return _isAttacking;
    }

    public void SetAttacking(bool value)
    {
        _isAttacking = value;
    }

    // **Restored FlipTowardsPlayer() Function**
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

    // **Restored OnDrawGizmos() Function**
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkingRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aoeAttackRange);
    }
}
