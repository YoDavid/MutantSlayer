using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    public EnemyConfig config;

    private Animator animator;
    private float lastAttackTime;
    private Transform player;
    private EnemyAttackCollider attackCollider;
    private EnemyMovement movement;
    private CameraShake cameraShake;

    public bool CanAttack => Time.time >= lastAttackTime + config.attackCooldown;
    public bool IsAttacking { get; private set; }
    public float AttackRange => config.attackRange;
    public Transform Player => player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackCollider = GetComponentInChildren<EnemyAttackCollider>(true);
        movement = GetComponent<EnemyMovement>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
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
        // Lock movement at start
        movement.LockMovement();
        IsAttacking = true;

        // First attack
        yield return new WaitForSeconds(config.attackDelay);

        // Only shake camera if this is a worm enemy
        if (config.hasDualAttack && cameraShake != null)
        {
            cameraShake.ShakeCamera();
            // OR use custom worm shake if available:
            // cameraShake.ShakeCameraWormAttack();
        }

        attackCollider.SetAttackPhase(true);
        attackCollider.EnableAttackCollider();
        yield return new WaitForSeconds(config.colliderActiveDuration);
        attackCollider.DisableAttackCollider();

        // Second attack (worm only)
        if (config.hasDualAttack)
        {
            yield return new WaitForSeconds(config.secondAttackDelay);
            attackCollider.SetAttackPhase(false);
            attackCollider.EnableAttackCollider();
            yield return new WaitForSeconds(config.secondColliderActiveDuration);
            attackCollider.DisableAttackCollider();
        }

        // Unlock movement at end
        IsAttacking = false;
        movement.UnlockMovement();
    }

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
        return player != null && Vector2.Distance(transform.position, player.position) <= config.attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, config.attackRange);
    }
}