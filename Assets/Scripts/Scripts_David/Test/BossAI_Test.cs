using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI_Test : MonoBehaviour
{
    public enum BossState { Idle, Moving, Jumping, AOEAttack, RangedAttack, ComboAttack }
    public BossState currentState;

    public Transform player;
    public float speed = 2f;
    [SerializeField] private float attackRange = 1.5f; // Melee attack range
    [SerializeField] private float rangedAttackRange = 5f; // Ranged attack range
    [SerializeField] private float walkingRange = 8f; // Range where the boss starts walking towards the player
    public float health = 100f;

    public Rigidbody2D rb;
    public Animator animator;

    private bool isAttacking = false;

    // Timer variables
    [Header("Attack Timer Settings")]
    public float minAttackTime = 1f; // Min time between attacks
    public float maxAttackTime = 3f; // Max time between attacks
    [SerializeField] private float attackCooldownTimer;

    void Start()
    {
        currentState = BossState.Idle;
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime); // Initialize the timer
    }

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime; // Reduce the cooldown timer

        // Handle walking behavior
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Always flip towards the player
        FlipTowardsPlayer();

        switch (currentState)
        {
            case BossState.Idle:
                IdleBehavior();
                if (distanceToPlayer < rangedAttackRange && attackCooldownTimer <= 0f) // Player is within range
                {
                    DecideAttack();
                }
                else if (distanceToPlayer < walkingRange && distanceToPlayer >= attackRange) // Start walking towards the player if within walking range and not in close attack range
                {
                    currentState = BossState.Moving;
                }
                break;

            case BossState.Moving:
                MoveBehavior(); // Walk towards the player

                if (attackCooldownTimer <= 0f) // Cooldown is over, decide next attack
                {
                    DecideAttack();
                }

                if (distanceToPlayer < rangedAttackRange && attackCooldownTimer <= 0f) // Player within attack range and cooldown is over
                {
                    DecideAttack();
                }
                break;

            case BossState.Jumping:
                JumpBehavior();
                break;

            case BossState.AOEAttack:
            case BossState.RangedAttack:
            case BossState.ComboAttack:
                // Already attacking, do nothing
                break;
        }
    }

    void IdleBehavior()
    {
        animator.SetBool("IsWalking", false);
    }

    void MoveBehavior()
    {
        if (!isAttacking) // Prevent moving while attacking
        {
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            // Flip the boss to face the player
            FlipTowardsPlayer();
        }
    }

    void JumpBehavior()
    {
        animator.SetBool("IsJumping", true);
    }

    void AOEAttackBehavior()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("AOEAttackTrigger");
            Invoke(nameof(ResetAttackState), 1.5f); // Adjust timing based on animation length
        }
    }

    void RangedAttackBehavior()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("RangedAttackTrigger");
            Invoke(nameof(ResetAttackState), 1.5f);
        }
    }

    void ComboAttackBehavior()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("ComboAttackTrigger");
            Invoke(nameof(ResetAttackState), 2.0f); // Adjust based on animation length
        }
    }


    void ResetAttackState()
    {
        isAttacking = false;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If the player is still within attack range, stay in idle state
        if (distanceToPlayer < attackRange)
        {
            currentState = BossState.Idle; // Stay in idle if the player is within melee range
        }
        else
        {
            currentState = BossState.Moving; // Otherwise, move towards the player
        }

        // Randomize cooldown time after each attack
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
    }

    void DecideAttack()
    {
        if (isAttacking) return; // Prevent spamming attacks

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (health > 50f) // Above 50% health
        {
            if (distanceToPlayer < attackRange) // Close range
            {
                currentState = BossState.ComboAttack; // Close-range combo attack
            }
            else if (distanceToPlayer < rangedAttackRange) // Ranged attack range
            {
                currentState = BossState.RangedAttack; // Ranged attack
            }
        }
        else // Below 50% health
        {
            if (distanceToPlayer < attackRange) // Close range
            {
                int attackChoice = Random.Range(0, 2); // Randomly choose between AOE or Combo
                if (attackChoice == 0)
                {
                    currentState = BossState.AOEAttack; // AOE attack
                }
                else
                {
                    currentState = BossState.ComboAttack; // Combo attack
                }
            }
            else if (distanceToPlayer < rangedAttackRange) // Ranged attack range
            {
                int attackChoice = Random.Range(0, 2); // Randomly choose between AOE or Ranged Attack
                if (attackChoice == 0)
                {
                    currentState = BossState.AOEAttack; // AOE attack
                }
                else
                {
                    currentState = BossState.RangedAttack; // Ranged attack
                }
            }
        }

        // Set the state based on the chosen attack
        UpdateAttackState();
    }

    void UpdateAttackState()
    {
        switch (currentState)
        {
            case BossState.AOEAttack:
                AOEAttackBehavior();
                break;
            case BossState.RangedAttack:
                RangedAttackBehavior();
                break;
            case BossState.ComboAttack:
                ComboAttackBehavior();
                break;
            default:
                break;
        }
    }

    // Flip the boss to face the player
    void FlipTowardsPlayer()
    {
        // Get the SpriteRenderer component of the boss
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // If the player is to the left of the boss, flip the sprite horizontally
        if (player.position.x < transform.position.x && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false; // Flip to face left
            Debug.Log("Flipping to Left");
        }
        // If the player is to the right of the boss, un-flip the sprite horizontally
        else if (player.position.x > transform.position.x && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true; // Flip to face right
            Debug.Log("Flipping to Right");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Melee attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Ranged attack range (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedAttackRange);

        // Walking range (green) - The range where the boss will walk towards the player
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, walkingRange);
    }
}

