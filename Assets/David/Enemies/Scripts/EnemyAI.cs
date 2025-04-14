using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyBehaviorState { Idle, Chasing, Attacking, ReturningHome }
    private EnemyBehaviorState currentState;

    private EnemyMovement movement;
    private EnemyCombat combat;
    private EnemyHealth health;

    private AudioManager audioManager;

    // --- New variables exposed to inspector ---
    [SerializeField] private float screamDistanceThreshold = 6f;
    [SerializeField] private float screamCooldownDuration = 15f; 
    private float lastScreamTime = -15f;

    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();

        health.OnDeath += HandleEnemyDeath;
        currentState = EnemyBehaviorState.Idle;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        GameObject audioObj = GameObject.Find("AudioManager");
        if (audioObj != null) audioManager = audioObj.GetComponent<AudioManager>();
    }

    private void Update()
    {
        if (health.CurrentHealth <= 0) return;

        CheckAndPlayScream();

        switch (currentState)
        {
            case EnemyBehaviorState.Idle:
                UpdateIdleBehavior();
                break;

            case EnemyBehaviorState.Chasing:
                UpdateChasingBehavior();
                break;

            case EnemyBehaviorState.Attacking:
                UpdateAttackingBehavior();
                break;

            case EnemyBehaviorState.ReturningHome:
                UpdateReturningBehavior();
                break;
        }
    }

    private void UpdateIdleBehavior()
    {
        if (movement.IsPlayerInDetectionRange())
        {
            TransitionToState(EnemyBehaviorState.Chasing);
        }
    }

    private void UpdateChasingBehavior()
    {
        if (combat.IsPlayerInAttackRange())
        {
            TransitionToState(EnemyBehaviorState.Attacking);
        }
        else if (!movement.IsPlayerInDetectionRange() ||
                 !movement.IsPlayerWithinChaseBounds())
        {
            TransitionToState(EnemyBehaviorState.ReturningHome);
        }
        else
        {
            movement.MoveToTarget(combat.Player.position);
        }
    }

    private void UpdateAttackingBehavior()
    {
        if (!combat.IsPlayerInAttackRange())
        {
            TransitionToState(EnemyBehaviorState.Chasing);
        }
        else if (combat.CanAttack)
        {
            movement.StopMovement();
            combat.ExecuteAttack();
        }
    }

    private void UpdateReturningBehavior()
    {
        if (movement.HasReachedPosition(movement.SpawnPosition))
        {
            TransitionToState(EnemyBehaviorState.Idle);
        }
        else if (movement.IsPlayerInDetectionRange() &&
                 movement.IsPlayerWithinChaseBounds())
        {
            TransitionToState(EnemyBehaviorState.Chasing);
        }
        else
        {
            movement.MoveToTarget(movement.SpawnPosition);
        }
    }

    private void TransitionToState(EnemyBehaviorState newState)
    {
        switch (currentState)
        {
            case EnemyBehaviorState.Chasing:
            case EnemyBehaviorState.ReturningHome:
                movement.StopMovement();
                break;
        }

        currentState = newState;
    }

    private void HandleEnemyDeath()
    {
        movement.StopMovement();
        enabled = false;
    }

    private void CheckAndPlayScream()
    {
        if (playerTransform == null || audioManager == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= screamDistanceThreshold && Time.time >= lastScreamTime + screamCooldownDuration)
        {
            if(this.gameObject.name == "MediumEnemy")
            {
                audioManager.PlayMediumEnemyScream();
                lastScreamTime = Time.time;

            } 
            else if (gameObject.name == "SmallEnemy")
            {
                audioManager.PlaySmallEnemyScream();
                lastScreamTime = Time.time;
            }
           
        }
    }

}
