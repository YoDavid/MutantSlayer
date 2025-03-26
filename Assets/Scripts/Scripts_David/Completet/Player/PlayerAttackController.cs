using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;

    [Header("Attack Debugging")]
    [SerializeField, HideInInspector] private int attackCount = 0;
    [SerializeField, HideInInspector] private float lastAttackTime = 0f;
    [SerializeField, HideInInspector] public bool isAttacking = false;

    [Header("Attack Settings")]
    [SerializeField] private float attackResetTime = 0.8f;
    [SerializeField] private float attackCooldownTime = 0.3f;
    [SerializeField] private float[] attackDurations = { 0.4f, 0.35f, 0.3f };

    [Header("Hitbox Timing (Per Attack)")]
    [SerializeField] private float[] hitboxEnableDelays = { 0.2f, 0.15f, 0.1f }; // When hitbox activates
    [SerializeField] private float[] hitboxActiveTimes = { 0.15f, 0.15f, 0.1f }; // How long hitbox stays active

    [Header("Attack Collider")]
    [SerializeField] private Collider2D attackCollider;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        if (attackCollider != null) attackCollider.enabled = false;
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

        int currentAttackIndex = attackCount;
        attackCount++;
        lastAttackTime = Time.time;
        isAttacking = true;

        // Start animation and hitbox control
        animationController.SetAttackState(attackCount);
        StartCoroutine(ControlAttackHitbox(currentAttackIndex));
    }

    private IEnumerator ControlAttackHitbox(int attackIndex)
    {
        // Wait for hitbox activation time (unique per attack)
        yield return new WaitForSeconds(hitboxEnableDelays[attackIndex]);

        // Enable hitbox for precise duration
        attackCollider.enabled = true;
        yield return new WaitForSeconds(hitboxActiveTimes[attackIndex]);
        attackCollider.enabled = false;

        // Wait for remaining animation time before resetting
        float remainingTime = attackDurations[attackIndex] -
                            (hitboxEnableDelays[attackIndex] + hitboxActiveTimes[attackIndex]);
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        // Reset animator if no new attack started
        if (Time.time - lastAttackTime >= attackDurations[attackIndex] - 0.1f)
        {
            animationController.SetAttackState(0);
        }
    }

    private void ResetAttack()
    {
        attackCount = 0;
        isAttacking = false;
        animationController.SetAttackState(0);
    }
}