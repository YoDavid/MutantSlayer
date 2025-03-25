using UnityEngine;

public class SmallEnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float walkingRange = 5f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 spawnPosition;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer

    public float WalkingRange => walkingRange;
    public Vector2 SpawnPosition => spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the renderer
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPosition = transform.position;
    }

    public void MoveToTarget(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * moveSpeed;

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
        return Vector2.Distance(transform.position, player.position) <= walkingRange;
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
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : transform.position, walkingRange);
    }
}