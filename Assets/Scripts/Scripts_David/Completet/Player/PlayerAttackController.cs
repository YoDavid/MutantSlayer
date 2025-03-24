using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;
    private PlayerMovementController movementController;

    [Header("Attack Debugging")]
    [SerializeField] private int attackCount = 0;
    [SerializeField] private float lastAttackTime = 0f;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private float comboWindowTimer = 0f;

    [Header("Combo Settings")]
    [SerializeField] private float comboWindowDuration = 5f;
    [SerializeField] private float attackCooldownTime = 0.3f;
    [SerializeField] private float[] attackDurations = { 0.4f, 0.35f, 0.3f };

    [Header("Attack Collider")]
    [SerializeField] private Collider2D attackCollider;
    [SerializeField] private float hitboxEnableDelay = 0.1f;

    private Coroutine currentAttackRoutine;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        movementController = GetComponent<PlayerMovementController>();
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void Update()
    {
        UpdateComboWindow();

        if (Input.GetKeyDown(KeyCode.X))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // Can only attack when grounded
        if (!movementController.isGrounded) return;

        if (CanAttack())
        {
            if (currentAttackRoutine != null)
            {
                StopCoroutine(currentAttackRoutine);
            }
            currentAttackRoutine = StartCoroutine(PerformAttack());
        }
    }

    private void UpdateComboWindow()
    {
        if (attackCount > 0)
        {
            comboWindowTimer += Time.deltaTime;

            if (comboWindowTimer >= comboWindowDuration)
            {
                ResetCombo();
            }
        }
    }

    private bool CanAttack()
    {
        // Can attack if we're not in an attack animation
        // OR if we're in a combo and the minimum cooldown has passed
        return !isAttacking ||
               (Time.time - lastAttackTime >= attackCooldownTime);
    }

    private IEnumerator PerformAttack()
    {
        // Reset combo if window expired or we've completed all attacks
        if (comboWindowTimer >= comboWindowDuration || attackCount >= attackDurations.Length)
        {
            attackCount = 0;
        }

        attackCount++;
        lastAttackTime = Time.time;
        comboWindowTimer = 0f;
        isAttacking = true;

        // Set the attack state (triggers animation)
        animationController.SetAttackState(attackCount);

        // Enable hitbox after delay
        yield return new WaitForSeconds(hitboxEnableDelay);
        if (attackCollider != null) attackCollider.enabled = true;

        // Keep attack active for the full animation duration
        float attackDuration = attackDurations[Mathf.Clamp(attackCount - 1, 0, attackDurations.Length - 1)];
        yield return new WaitForSeconds(attackDuration - hitboxEnableDelay);

        // Disable hitbox
        if (attackCollider != null) attackCollider.enabled = false;

        // Reset states
        isAttacking = false;
        animationController.SetAttackState(0);
    }

    private void ResetCombo()
    {
        attackCount = 0;
        comboWindowTimer = 0f;
        isAttacking = false;
        animationController.SetAttackState(0);

        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
            currentAttackRoutine = null;
        }
    }
}