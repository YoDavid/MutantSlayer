using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float speed = 2f;
    public Transform player;
    public Animator animator;
    private Rigidbody2D rb;
    public float jumpForce = 5f;
    private BossAI bossAI; // Reference to BossAI

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Animation Timers")]
    public float anticipationTime = 0.7f;
    public float jumpUpwardTime = 1f;
    public float JumpMidair = 0.1f;
    public float landingTime = 0.5f;
    public float groundSmashTime = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossAI = GetComponent<BossAI>(); // Get reference to BossAI
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.P) && isGrounded)
        {
            StartCoroutine(JumpTowardsPlayer());
        }
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
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        }
        else
        {
            animator.SetBool("IsWalking", false);
            rb.velocity = Vector2.zero;
        }
    }

    public void TriggerJump()
    {
        StartCoroutine(JumpTowardsPlayer());
    }

    public IEnumerator JumpTowardsPlayer()
    {
        if (bossAI != null)
        {
            bossAI.currentState = BossState.Jumping;
        }

        animator.SetTrigger("JumpAnticipation");
        yield return new WaitForSeconds(anticipationTime);

        Vector2 targetPosition = player.position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, jumpForce);

        animator.SetTrigger("JumpUpwardMovement");
        bool isFalling = false;
        bool hasLanded = false;

        while (!hasLanded)
        {
            if (rb.velocity.y > 0)
            {
                animator.SetTrigger("JumpUpwardMovement");
            }
            else if (!isFalling && rb.velocity.y < 0)
            {
                animator.SetTrigger("JumpMidair");
                isFalling = true;
            }

            if (!hasLanded && IsGrounded())
            {
                animator.SetTrigger("JumpLanding");
                hasLanded = true;
            }

            yield return null;
        }

        animator.SetTrigger("JumpGroundSmash");
        rb.velocity = new Vector2(0, rb.velocity.y);
        yield return new WaitForSeconds(landingTime);

        if (bossAI != null)
        {
            bossAI.currentState = BossState.Moving;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
}