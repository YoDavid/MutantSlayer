using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float speed = 2f;
    public Transform player;
    public Animator animator;
    private Rigidbody2D rb;
    private BossAI bossAI;

    [Header("Ground Check Gizmo")]
    public bool showGroundCheckGizmo = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossAI = GetComponent<BossAI>(); // Get reference to BossAI
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


}
