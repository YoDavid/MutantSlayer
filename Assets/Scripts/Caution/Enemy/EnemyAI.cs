using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyBehaviorState { Idle, Chasing, Attacking, ReturningHome }
    private EnemyBehaviorState currentState;

    private EnemyMovement movement;
    private EnemyCombat combat;
    private EnemyHealth health;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();

        health.OnDeath += HandleEnemyDeath;
    }

    private void Update()
    {
        if (health.currentHealth <= 0) return;

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
        else if (!movement.IsPlayerInDetectionRange())
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
            movement.StopMovement(); // Add this line
            combat.ExecuteAttack();
        }
    }

    private void UpdateReturningBehavior()
    {
        if (movement.HasReachedPosition(movement.SpawnPosition))
        {
            TransitionToState(EnemyBehaviorState.Idle);
        }
        else if (movement.IsPlayerInDetectionRange())
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
}