using UnityEngine;

[System.Serializable]
public class PatrolSettings
{
    public bool enablePatrol = true;
    [Tooltip("Left patrol bound (local X offset from spawn)")]
    public float patrolLeftBound = -3f;
    [Tooltip("Right patrol bound (local X offset from spawn)")]
    public float patrolRightBound = 3f;
    public float patrolSpeed = 1.5f;
    [Tooltip("Time to wait at each patrol point")]
    public float idleTimeAtEdges = 1f;
}

public class EnemyMovement : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyConfig config;

    [Header("Patrol Settings")]
    public PatrolSettings patrolSettings;

    [Header("Combat Chase Limits")]
    [Tooltip("How far beyond patrol bounds enemy can chase")]
    public float maxChaseRangeFromBounds = 2f;
    [Tooltip("Minimum distance to maintain from player")]
    public float minDistanceFromPlayer = 0.5f;
    [Tooltip("How often to check player position (seconds)")]
    public float playerPositionCheckInterval = 0.2f; // New: Interval for position checks

    // Private variables
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 spawnPosition;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float currentIdleTime;
    private bool isMovingRight = true;
    private bool isInCombat = false;
    private float nextPositionCheckTime; // New: Timer for position checks

    // World-space boundary properties
    private float WorldPatrolLeft => spawnPosition.x + patrolSettings.patrolLeftBound;
    private float WorldPatrolRight => spawnPosition.x + patrolSettings.patrolRightBound;
    private float WorldChaseLeft => WorldPatrolLeft - maxChaseRangeFromBounds;
    private float WorldChaseRight => WorldPatrolRight + maxChaseRangeFromBounds;

    public Vector2 SpawnPosition => spawnPosition;
    public bool MovementLocked { get; private set; }

    [SerializeField] private bool isFacingLeft = false;
    private BoxCollider2D mainCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPosition = transform.position;
        mainCollider = GetComponent<BoxCollider2D>();
        isFacingLeft = true;
    }


    private void Update()
    {
        if (MovementLocked) return;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (Time.time >= nextPositionCheckTime)
        {
            nextPositionCheckTime = Time.time + playerPositionCheckInterval;
            UpdateCombatState();
        }

        if (isInCombat)
        {
            MoveToTarget(player.position);
        }
        else if (patrolSettings.enablePatrol)
        {
            PatrolBehavior();
        }
        else
        {
            StopMovement();
        }
    }

    private void UpdateCombatState()
    {
        bool wasInCombat = isInCombat;
        isInCombat = IsPlayerInDetectionRange() && IsPlayerWithinChaseBounds();

        if (isInCombat && !wasInCombat)
        {
            UpdateFacingTowardsPlayer();
        }
        else if (!isInCombat && wasInCombat && patrolSettings.enablePatrol)
        {
            FaceInPatrolDirection();
        }
    }


    private void PatrolBehavior()
    {
        if (currentIdleTime > 0)
        {
            currentIdleTime -= Time.deltaTime;
            StopMovement();
            return;
        }

        float targetX = isMovingRight ? WorldPatrolRight : WorldPatrolLeft;
        Vector2 targetPosition = new Vector2(targetX, spawnPosition.y);

        if (HasReachedPosition(targetPosition, 0.1f))
        {
            isMovingRight = !isMovingRight;
            currentIdleTime = patrolSettings.idleTimeAtEdges;
        }
        else
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = direction * patrolSettings.patrolSpeed;

            FaceInPatrolDirection();

            animator.SetBool("IsMoving", true);

        }
    }

    public void MoveToTarget(Vector2 targetPosition)
    {
        if (MovementLocked) return;

        // Clamp target within chase boundaries
        float clampedX = Mathf.Clamp(targetPosition.x, WorldChaseLeft, WorldChaseRight);
        targetPosition = new Vector2(clampedX, spawnPosition.y);

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, targetPosition);

        // If we're too close to the player, stop moving but keep facing them
        if (distanceToPlayer <= minDistanceFromPlayer)
        {
            StopMovement();
            UpdateFacingTowardsPlayer(); // Keep facing player even when not moving
            return;
        }

        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * config.moveSpeed;
        UpdateSpriteFacing(direction);
        animator.SetBool("IsMoving", true);
    }

    private void UpdateFacingTowardsPlayer()
    {
        if (player == null) return;
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        UpdateSpriteFacing(directionToPlayer);
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("IsMoving", false);
    }

    public bool IsPlayerWithinChaseBounds()
    {
        if (player == null) return false;
        float playerX = player.position.x;
        return playerX >= WorldChaseLeft && playerX <= WorldChaseRight;
    }

    public bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= config.walkingRange;
    }

    public bool HasReachedPosition(Vector2 targetPosition, float arrivalThreshold = 0.1f)
    {
        return Vector2.Distance(transform.position, targetPosition) < arrivalThreshold;
    }

    private void UpdateSpriteFacing(Vector2 movementDirection)
    {
        bool shouldFaceLeft = movementDirection.x < 0;

        if (shouldFaceLeft != isFacingLeft)
        {
            isFacingLeft = shouldFaceLeft;
            spriteRenderer.flipX = shouldFaceLeft;
            UpdateColliderOffset(); // Ensures collider matches new facing
        }
    }

    public void LockMovement() => MovementLocked = true;
    public void UnlockMovement() => MovementLocked = false;

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Vector3 currentPos = Application.isPlaying ? spawnPosition : transform.position;

        // Draw patrol area (green)
        Gizmos.color = new Color(0, 1, 0, 0.15f);
        Vector3 patrolCenter = new Vector3(
            (WorldPatrolLeft + WorldPatrolRight) / 2,
            currentPos.y,
            0
        );
        float patrolWidth = WorldPatrolRight - WorldPatrolLeft;
        Gizmos.DrawCube(patrolCenter, new Vector3(patrolWidth, 0.5f, 0));

        // Draw chase boundaries (red)
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Vector3 chaseCenter = new Vector3(
            (WorldChaseLeft + WorldChaseRight) / 2,
            currentPos.y,
            0
        );
        Gizmos.DrawCube(chaseCenter, new Vector3(WorldChaseRight - WorldChaseLeft, 0.3f, 0));

        // Draw current position (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Draw spawn position (cyan)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPosition, 0.25f);
        }

        // Draw detection range (blue)
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawWireSphere(transform.position, config.walkingRange);

        // Draw minimum distance (magenta)
        Gizmos.color = new Color(1, 0, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
    }

    private void UpdateColliderOffset()
    {
        if (mainCollider == null || config == null) return;

        mainCollider.offset = isFacingLeft ? config.leftFacingColliderOffset : config.rightFacingColliderOffset;
    }

    private void FaceInPatrolDirection()
    {
        UpdateSpriteFacing(isMovingRight ? Vector2.right : Vector2.left);
    }

}