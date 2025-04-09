using UnityEngine;

public class BossMovementTesting : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;

    [Header("References")]
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void HandleIdle()
    {
        animator.SetBool("IsWalking", false);
    }

    public void HandleChase(float distanceToPlayer, float stopDistance, bool isAttacking)
    {
        if (isAttacking) return;

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
