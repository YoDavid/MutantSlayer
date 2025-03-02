using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;

    [Header("Attack Debugging")]
    [SerializeField, HideInInspector] private int attackCount = 0;
    [SerializeField, HideInInspector] private float lastAttackTime = 0f;
    [SerializeField, HideInInspector] private bool isAttacking = false;

    [Header("Attack Settings")]
    [SerializeField] private float attackResetTime = 0.8f;
    [SerializeField] private float attackCooldownTime = 0.3f;
    [SerializeField, Tooltip("Duration for each attack in the combo")] private float[] attackDurations = { 0.4f, 0.35f, 0.3f };

    [Header("Attack Collider")]
    [SerializeField] private Collider2D attackCollider;
    [SerializeField] private float hitboxEnableDelay = 0.1f;
    [SerializeField] private float hitboxActiveTime = 0.2f;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();

        if (animationController == null)
        {
            Debug.LogError("PlayerAnimationController component is missing on this GameObject.");
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Attack Collider is not assigned in PlayerAttackController!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && CanAttack())
        {
            PerformAttack();
        }

        if (isAttacking && Time.time - lastAttackTime > attackResetTime)
        {
            ResetAttack();
        }
    }

    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldownTime;
    }

    private void PerformAttack()
    {
        if (Time.time - lastAttackTime > attackResetTime || attackCount >= attackDurations.Length)
        {
            attackCount = 0;
        }

        attackCount++;
        lastAttackTime = Time.time;
        isAttacking = true;

        animationController.SetAttackState(attackCount);

        StartCoroutine(EnableHitboxWithDelay(hitboxEnableDelay, hitboxActiveTime));
        StartCoroutine(ResetAnimatorAfterAttack(attackDurations[attackCount - 1]));
    }

    private IEnumerator EnableHitboxWithDelay(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        attackCollider.enabled = true;
        yield return new WaitForSeconds(duration);
        attackCollider.enabled = false;
    }

    private IEnumerator ResetAnimatorAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        animationController.SetAttackState(0);
    }

    private void ResetAttack()
    {
        attackCount = 0;
        isAttacking = false;
        animationController.SetAttackState(0);
    }
}
