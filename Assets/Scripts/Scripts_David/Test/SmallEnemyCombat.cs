using UnityEngine;
using System.Collections;

public class SmallEnemyCombat : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Delay before attack collider activates (for anticipation frames)")]
    [SerializeField] private float attackDelay = 0.3f;

    [Tooltip("Duration the attack collider stays active")]
    [SerializeField] private float colliderActiveDuration = 0.15f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private Animator animator;
    private float lastAttackTime;
    private Transform player;
    private SmallEnemyAttackCollider attackCollider;

    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;
    public bool IsAttacking { get; private set; }
    public float AttackRange => attackRange;
    public Transform Player => player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackCollider = GetComponentInChildren<SmallEnemyAttackCollider>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ExecuteAttack();
        }
    }

    public void ExecuteAttack()
    {
        lastAttackTime = Time.time;
        IsAttacking = true;
        animator.SetTrigger("Attack");

        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        // Anticipation phase
        yield return new WaitForSeconds(attackDelay);

        // Active hit frames
        EnableAttackCollider();
        yield return new WaitForSeconds(colliderActiveDuration);
        DisableAttackCollider();

        IsAttacking = false;
    }

    // Manual control (for animation events if needed)
    public void EnableAttackCollider()
    {
        if (attackCollider != null)
            attackCollider.EnableAttackCollider();
    }

    public void DisableAttackCollider()
    {
        if (attackCollider != null)
            attackCollider.DisableAttackCollider();
        IsAttacking = false;
    }

    public bool IsPlayerInAttackRange()
    {
        return player != null && Vector2.Distance(transform.position, player.position) <= attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}