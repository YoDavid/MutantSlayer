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

    // Idle simply stops walking
    public void HandleIdleState()
    {
        animator.SetBool("IsWalking", false);
    }

    // Only move if not attacking (matching the test script’s MoveBehavior)
    public void HandleMovingState(float distanceToPlayer, bool isAttacking)
    {
        if (!isAttacking)
        {
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            FlipTowardsPlayer();
        }
    }

    public void HandleJumpingState()
    {
        animator.SetBool("IsJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void FlipTowardsPlayer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (player.position.x < transform.position.x && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
        else if (player.position.x > transform.position.x && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
    }
}
