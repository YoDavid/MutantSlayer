using UnityEngine;

public class BossComboAttackHitboxTesting : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private DamageConfig damageConfig;
    [SerializeField] private float attackDuration;
    [SerializeField] private float[] attackTimings;

    [Header("Collider Settings")]
    [SerializeField] private float colliderShift;
    [SerializeField] private Collider2D attackCollider;

    [Header("Player References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Collider2D playerHurtBoxCollider;

    private BossColliderHandlerTesting colliderHandler;
    private BossComboAttackTimerTesting attackTimer;
    private BossDamageHandlerTesting damageHandler;

    private void Awake()
    {
        colliderHandler = new BossColliderHandlerTesting(attackCollider, colliderShift);
        damageHandler = new BossDamageHandlerTesting(damageConfig, playerHealth, playerHurtBoxCollider);
        attackTimer = gameObject.AddComponent<BossComboAttackTimerTesting>();
        attackTimer.Initialize(attackTimings, attackDuration, colliderHandler, damageHandler);
    }

    private void Start()
    {
        colliderHandler.SetupCollider();
        damageHandler.FindPlayerReferences();
    }

    public void ActivateComboAttackCollider()
    {
        attackTimer.StartComboAttack();
    }

    public void FlipCollider(bool isFlipped)
    {
        colliderHandler.FlipCollider(isFlipped);
    }
}