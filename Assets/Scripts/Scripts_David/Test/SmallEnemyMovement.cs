using UnityEngine;

public class SmallEnemyMovement : MonoBehaviour
{
    public SmallEnemyConfig config; // Reference to config
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 spawnPosition;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public float WalkingRange => config.walkingRange; // Now using config
    public Vector2 SpawnPosition => spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPosition = transform.position;
    }

    public void MoveToTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * config.moveSpeed; // Now using config
        UpdateSpriteFacing(direction);
        animator.SetBool("IsMoving", true);
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("IsMoving", false);
    }


    public bool IsPlayerInDetectionRange()
    {
        return Vector2.Distance(transform.position, player.position) <= config.walkingRange; // Fixed
    }

    public bool HasReachedPosition(Vector2 targetPosition, float arrivalThreshold = 0.1f)
    {
        return Vector2.Distance(transform.position, targetPosition) < arrivalThreshold;
    }

    private void UpdateSpriteFacing(Vector2 movementDirection)
    {
        if (movementDirection.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.2f);
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : transform.position, config.walkingRange); // Fixed
    }
}