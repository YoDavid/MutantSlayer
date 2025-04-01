using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;

    [Header("Combo Settings")]
    [SerializeField] private float attackResetTime = 0.8f;
    [SerializeField] private float[] attackDurations = { 0.4f, 0.35f, 0.3f };

    [Header("Hitbox Settings (Per Attack)")]
    [SerializeField]
    private Vector2[] colliderOffsets = {
        new Vector2(0.42f, -9.74f),
        new Vector2(5.24f, -6.08f),
        new Vector2(9.67f, -8.71f)
    };
    [SerializeField]
    private Vector2[] colliderSizes = {
        new Vector2(29.9f, 11.12f),
        new Vector2(16.3f, 23.8f),
        new Vector2(22.28f, 16.5f)
    };
    [SerializeField] private float[] hitboxEnableDelays = { 0.15f, 0.15f, 0.1f };
    [SerializeField] private float[] hitboxActiveTimes = { 0.15f, 0.2f, 0.3f };

    [Header("References")]
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private bool showGizmos = true;

    private int attackCount;
    [SerializeField] private float lastAttackTime;
    [SerializeField] private float lastAttackEndTime;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    private Coroutine currentAttackRoutine;

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        if (attackCollider != null)
        {
            originalOffset = attackCollider.offset;
            originalSize = attackCollider.size;
            attackCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsAttacking && attackCount > 0 && Time.time - lastAttackEndTime > attackResetTime)
        {
            ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.X) && CanAttack())
        {
            PerformAttack();

        }
    }

    private bool CanAttack()
    {
        return !IsAttacking && (attackCount == 0 || Time.time - lastAttackEndTime <= attackResetTime);
    }

    private void PerformAttack()
    {
        if (attackCount >= attackDurations.Length)
        {
            ResetCombo();
        }

        lastAttackTime = Time.time;
        IsAttacking = true;
        attackCount++;
        Debug.Log(attackCount);

        if (currentAttackRoutine != null)
        {
            StopCoroutine(currentAttackRoutine);
        }

        attackCollider.offset = colliderOffsets[attackCount - 1];
        attackCollider.size = colliderSizes[attackCount - 1];

        animationController.SetAttackState(attackCount);
        currentAttackRoutine = StartCoroutine(ExecuteAttack(attackCount - 1));
    }

    private IEnumerator ExecuteAttack(int attackIndex)
    {
        yield return new WaitForSeconds(hitboxEnableDelays[attackIndex]);
        attackCollider.enabled = true;

        yield return new WaitForSeconds(hitboxActiveTimes[attackIndex]);
        attackCollider.enabled = false;

        attackCollider.offset = originalOffset;
        attackCollider.size = originalSize;

        float remainingTime = attackDurations[attackIndex] -
                           (hitboxEnableDelays[attackIndex] + hitboxActiveTimes[attackIndex]);
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime);

        IsAttacking = false;
        lastAttackEndTime = Time.time;

        // Only reset animation state if not the final attack
        if (attackIndex < attackDurations.Length - 1)
        {
            animationController.SetAttackState(0);
        }
        else 
        {
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        attackCount = 0;
        Debug.Log("ResetCombo");
        animationController.SetAttackState(0);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || attackCollider == null || colliderOffsets == null || colliderSizes == null)
            return;

        Gizmos.color = Color.green;
        Matrix4x4 originalMatrix = Gizmos.matrix;

        for (int i = 0; i < Mathf.Min(colliderOffsets.Length, colliderSizes.Length); i++)
        {
            Gizmos.matrix = Matrix4x4.TRS(
                transform.TransformPoint(colliderOffsets[i]),
                transform.rotation,
                transform.lossyScale
            );
            Gizmos.DrawWireCube(Vector3.zero, colliderSizes[i]);
        }
        Gizmos.matrix = originalMatrix;
    }
}