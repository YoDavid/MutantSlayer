using UnityEngine;

public class BossMovementHandlerTesting : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float desiredDistanceFromPlayer;

    private Rigidbody2D rb;
    private BossMovementTesting bossMovement;
    private Animator animator;
    private Transform player;

    public Rigidbody2D Rb => rb;  // Expose the Rigidbody2D

    void Start()
    {
        FindReferences();
    }

    void FindReferences()
    {
        bossMovement = GetComponent<BossMovementTesting>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void HandleMovement(float distanceToPlayer, bool isAttacking)
    {
        if (isAttacking) return;

        if (distanceToPlayer > desiredDistanceFromPlayer)
        {
            MoveTowardsPlayer(distanceToPlayer);
        }
        else
        {
            StopMovement();
        }
    }

    private void MoveTowardsPlayer(float distanceToPlayer)
    {
        float moveDirection = player.position.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);
        animator.SetBool("IsWalking", true);
    }

    private void StopMovement()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetBool("IsWalking", false);
    }
}
