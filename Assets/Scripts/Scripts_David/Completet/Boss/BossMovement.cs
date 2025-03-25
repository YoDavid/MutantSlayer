using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _stoppingDistance = 0.5f;
    [SerializeField] private float _returnToStartThreshold = 0.1f;
    [SerializeField] private float _acceleration = 10f;

    [Header("References")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private BossAI _bossAI;
    [SerializeField] private Transform _player;

    private Vector2 _currentVelocity;
    public bool isReturningToStart = false;
    private const string WALK_ANIM_PARAM = "IsWalking";

    private void Awake()
    {
        InitializeReferences();
        ValidateComponents();
    }

    private void InitializeReferences()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_bossAI == null) _bossAI = GetComponent<BossAI>();
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void ValidateComponents()
    {
        if (_rb == null) Debug.LogError("Rigidbody2D missing on BossMovement", this);
        if (_animator == null) Debug.LogError("Animator missing on BossMovement", this);
        if (_bossAI == null) Debug.LogError("BossAI reference missing on BossMovement", this);
        if (_player == null) Debug.LogError("Player transform not assigned", this);
    }

<<<<<<< HEAD
    private void Update()
    {
        if (ShouldMove())
        {
            HandleMovement();
        }
    }

    private bool ShouldMove()
    {
        return _bossAI != null &&
               _bossAI.currentState == BossState.Moving &&
               !_bossAI.IsAttacking();
    }

    public void HandleMovement()
    {
        Vector2 targetPosition = GetTargetPosition();
        MoveToTarget(targetPosition, GetStoppingDistance());
        UpdateAnimation(targetPosition);
    }

    private Vector2 GetTargetPosition()
    {
        return isReturningToStart ? _bossAI.startingPosition : (Vector2)_player.position;
    }

    private float GetStoppingDistance()
    {
        return isReturningToStart ? _returnToStartThreshold : _bossAI.desiredDistanceFromPlayer;
    }

    public void MoveToTarget(Vector2 targetPosition, float stopDistance)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = direction * _moveSpeed;

        // Smooth acceleration
        _rb.velocity = Vector2.SmoothDamp(
            _rb.velocity,
            new Vector2(targetVelocity.x, _rb.velocity.y),
            ref _currentVelocity,
            _acceleration * Time.deltaTime
        );

        CheckArrival(targetPosition, stopDistance);
    }

    private void CheckArrival(Vector2 targetPosition, float stopDistance)
    {
        if (Vector2.Distance(transform.position, targetPosition) <= stopDistance)
        {
            ArrivedAtTarget();
        }
    }

    private void UpdateAnimation(Vector2 targetPosition)
    {
        bool shouldWalk = Vector2.Distance(transform.position, targetPosition) > _stoppingDistance;
        _animator.SetBool(WALK_ANIM_PARAM, shouldWalk);
    }

    private void ArrivedAtTarget()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _animator.SetBool(WALK_ANIM_PARAM, false);

        if (isReturningToStart)
        {
            isReturningToStart = false;
            _bossAI.currentState = BossState.Idle;
        }
    }

    public void StartReturningToStart()
    {
        isReturningToStart = true;
        _bossAI.currentState = BossState.Moving;
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _stoppingDistance);

        if (isReturningToStart && _bossAI != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _bossAI.startingPosition);
        }
    }
}
=======
    public void HandleMovingState(float distanceToPlayer, float stopDistance, bool isAttacking)
    {
        if (isAttacking) return;

        if (isReturningToStart)
        {
            // Move toward the starting position
            Vector2 direction = (bossAI.startingPosition - (Vector2)transform.position).normalized;
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            animator.SetBool("IsWalking", true);

            // If close to the starting position, stop moving
            if (Vector2.Distance(transform.position, bossAI.startingPosition) < 0.1f)
            {
                rb.velocity = Vector2.zero;
                animator.SetBool("IsWalking", false);
                isReturningToStart = false; // Reset the flag
                bossAI.currentState = BossState.Idle; // Transition to Idle state
            }
        }
        else
        {
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

    public void StartReturningToStart()
    {
        isReturningToStart = true; // Set the flag to start returning to the starting position
    }
}
>>>>>>> parent of 33b28b5 (Fixed_Layers_For_Player)
