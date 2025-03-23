using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;

    [Header("References")]
    public Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private BossAI bossAI;
    public Transform player;

    [Header("Debug Settings")]
    public bool showGroundCheckGizmo = true;

    public bool isReturningToStart = false;

    void Start()
    {
        AssignReferences();
    }

    void AssignReferences()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossAI>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (rb == null) Debug.LogWarning("Rigidbody2D not found!");
        if (animator == null) Debug.LogWarning("Animator not found!");
        if (bossAI == null) Debug.LogWarning("BossAI not found!");
        if (player == null) Debug.LogWarning("Player not found! Make sure the Player has the correct tag.");
    }

    public void HandleIdleState()
    {
        animator.SetBool("IsWalking", false);
    }

    public void HandleMovingState(float distanceToPlayer, float stopDistance, bool isAttacking)
    {
        if (isAttacking) return;

        if (isReturningToStart)
        {
            // Move toward the starting position
            Vector2 direction = (bossAI.startingPosition - (Vector2)transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            animator.SetBool("IsWalking", true);

            // If close to the starting position, stop moving
            if (Vector2.Distance(transform.position, bossAI.startingPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero;
                animator.SetBool("IsWalking", false);
                isReturningToStart = false; // Reset the flag
                bossAI.currentState = BossState.Idle; // Transition to Idle state
            }
        }
        else
        {
            // Move toward the player
            if (distanceToPlayer > stopDistance)
            {
                animator.SetBool("IsWalking", true);
                Vector2 direction = (player.position - transform.position).normalized;
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            }
            else
            {
                animator.SetBool("IsWalking", false);
                rb.velocity = Vector2.zero;
            }
        }
    }

    public void StartReturningToStart()
    {
        isReturningToStart = true; // Set the flag to start returning to the starting position
    }
}
