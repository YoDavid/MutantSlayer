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
    }

    public void HandleIdleState()
    {
        animator.SetBool("IsWalking", false);
    }

    public void HandleMovingState(float distanceToPlayer, float stopDistance, bool isAttacking)
    {
        if (isAttacking) return;

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