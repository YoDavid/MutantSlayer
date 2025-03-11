using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float speed = 2f;
    public Transform player;
    public Animator animator;
    private Rigidbody2D rb;
    public float jumpForce = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HandleIdleState()
    {
        animator.SetBool("IsWalking", false);
    }

    public void HandleMovingState(float distanceToPlayer, float stopDistance, bool isAttacking)
    {
        if (isAttacking) return;

        if (distanceToPlayer > stopDistance)
        {
            animator.SetBool("IsWalking", true);
            // Move the boss with Rigidbody2D velocity
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y); // Only move horizontally
        }
        else
        {
            animator.SetBool("IsWalking", false);
            rb.velocity = Vector2.zero; // Stop moving when within stop distance
        }
    }


    public void HandleJumpingState()
    {
        animator.SetBool("IsJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
