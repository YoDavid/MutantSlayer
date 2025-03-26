using UnityEngine;
using System.Collections;

public class SmallEnemyCombat : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    public SmallEnemyConfig config;

    private Animator animator;
    private float lastAttackTime;
    private Transform player;
    private SmallEnemyAttackCollider attackCollider;

    public bool CanAttack => Time.time >= lastAttackTime + config.attackCooldown; // Now using config
    public bool IsAttacking { get; private set; }
    public float AttackRange => config.attackRange; // Now using config
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
        yield return new WaitForSeconds(config.attackDelay); // Now using config
        EnableAttackCollider();
        yield return new WaitForSeconds(config.colliderActiveDuration); // Now using config
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
        return player != null && Vector2.Distance(transform.position, player.position) <= config.attackRange; // Fixed
    }


    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, config.attackRange); // Fixed
    }
}