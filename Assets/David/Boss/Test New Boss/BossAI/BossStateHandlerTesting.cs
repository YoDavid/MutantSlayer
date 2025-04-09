using UnityEngine;

public class BossStateHandlerTesting : MonoBehaviour
{
    public BossState currentState;
    private BossMovementHandlerTesting movementHandler;
    [SerializeField] private BossAttackHandlerTesting attackHandler;
    [SerializeField] private Transform player;

    void Start()
    {
        movementHandler = GetComponent<BossMovementHandlerTesting>();
        attackHandler = GetComponent<BossAttackHandlerTesting>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        HandleState(distanceToPlayer);
    }

    void HandleState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case BossState.Idle:
                if (distanceToPlayer < 5f)
                {
                    currentState = BossState.Moving;
                }
                break;

            case BossState.Moving:
                movementHandler.HandleMovement(distanceToPlayer, false);
                if (distanceToPlayer < 2f)
                {
                    currentState = BossState.ComboAttack;
                }
                else if (distanceToPlayer > 10f)
                {
                    currentState = BossState.RangedAttack;
                }
                break;

            case BossState.ComboAttack:
            case BossState.RangedAttack:
            case BossState.AOEAttack:
                attackHandler.ExecuteAttack(currentState);
                break;
        }
    }
}
