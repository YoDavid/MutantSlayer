using UnityEngine;

public class BossAIStateTesting : MonoBehaviour
{
    private BossStateHandlerTesting stateHandler;
    private BossCooldownHandlerTesting cooldownHandler;
    private BossFlipHandlerTesting flipHandler;
    private BossGroundCheckHandlerTesting groundCheckHandler;
    private BossAttackHandlerTesting attackHandler;
    private Transform player;

    // Attack range variables (added back from original script)
    [Header("Attack Range Definitions")]
    public float attackRange;
    [SerializeField] private float rangedAttackRange;
    [SerializeField] private float aoeAttackRange;
    [SerializeField] private float walkingRange;
    [SerializeField] private float jumpingAttackRange;
    [SerializeField] bool showGizmos;


    void Start()
    {
        stateHandler = GetComponent<BossStateHandlerTesting>();
        cooldownHandler = GetComponent<BossCooldownHandlerTesting>();
        flipHandler = GetComponent<BossFlipHandlerTesting>();
        groundCheckHandler = GetComponent<BossGroundCheckHandlerTesting>();
        attackHandler = GetComponent<BossAttackHandlerTesting>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (groundCheckHandler.IsGrounded())
        {
            flipHandler.FlipTowardsPlayer();
        }

        // Get the current distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Decide on attack type based on distance to player
        if (distanceToPlayer < attackRange)
        {
            stateHandler.currentState = BossState.ComboAttack; // Close-range attack
        }
        else if (distanceToPlayer < rangedAttackRange)
        {
            stateHandler.currentState = BossState.RangedAttack; // Medium-range attack
        }
        else if (distanceToPlayer < aoeAttackRange)
        {
            stateHandler.currentState = BossState.AOEAttack; // AOE attack
        }
        else if (distanceToPlayer < walkingRange)
        {
            stateHandler.currentState = BossState.Moving; // Moving towards the player
        }

        // Execute the attack based on the selected state
        attackHandler.ExecuteAttack(stateHandler.currentState);

        stateHandler.Update();
        cooldownHandler.Update();
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange); // Melee range

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange); // Ranged attack range

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, walkingRange); // Walking range

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aoeAttackRange); // AOE attack range

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, jumpingAttackRange); // Jumping attack range
        }
    }
}
