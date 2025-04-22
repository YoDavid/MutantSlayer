using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    private PlayerAnimationController animationController;
    private PlayerMovementController movementController;

    [Header("Combo Settings")]
    [SerializeField] private float attackResetTime = 0.8f;
    [SerializeField] private float[] attackDurations = { 0.4f, 0.35f, 0.3f };

    [Header("Ground Requirements")]
    [SerializeField] private bool requireGrounded = true;
    [SerializeField] private bool cancelAttackIfAirborne = true;

    [Header("Hitbox Settings (Per Attack)")]
    [SerializeField] private Vector2[] colliderOffsets;
    [SerializeField] private Vector2[] colliderSizes;
    [SerializeField] private float[] hitboxEnableDelays;
    [SerializeField] private float[] hitboxActiveTimes;

    [Header("References")]
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private bool showGizmos = true;

    [Header("Charge Attack Settings")]
    [SerializeField] private float chargeStartDelay = 0.25f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private UIPlayerStaminaBar staminaBar;

    private int attackCount;
    private float lastAttackTime;
    private float lastAttackEndTime;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    private Coroutine currentAttackRoutine;

    public bool IsAttacking { get; private set; }
    private bool isAttackEnabled = true;

    private bool isCharging = false;
    private float chargeStartTime;

    [Header("Projectile")]
    [SerializeField] private GameObject chargeProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float chargeProjectileDelay = 0.3f;
    private bool hasFiredChargeAttack = false;
    private float staminaDepleted = 0f; // Track the stamina consumed during the charge cycle

    [Header("Charge Audio Delays")]
    [SerializeField] private float chargingSoundDelay = 0.1f;
    [SerializeField] private float swordDrawDelay = 0.4f;
    [SerializeField] private float climaxDelay = 1.8f;

    private Coroutine chargeAudioRoutine;
    private bool playedSwordDraw = false;
    private bool playedClimax = false;




    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        movementController = GetComponent<PlayerMovementController>();

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
            ResetCombo();

        if (Input.GetMouseButtonDown(0) && CanAttack())
            PerformAttack();

        if (IsAttacking && cancelAttackIfAirborne && !IsGrounded())
            CancelCurrentAttack();

        HandleChargeAttackInput();
    }

    private void HandleChargeAttackInput()
    {
        // Start the charging process
        if (Input.GetMouseButtonDown(1))
            chargeStartTime = Time.time;

        // While holding the right mouse button
        if (Input.GetMouseButton(1))
        {
            if (!isCharging && Time.time - chargeStartTime >= chargeStartDelay)
            {
                if (staminaBar.GetStamina() >= 25f)
                {
                    animationController.SetChargeStart(true);
                    isCharging = true;
                    playedSwordDraw = false;
                    playedClimax = false;

                    // Start the continuous and delayed sound logic
                    if (chargeAudioRoutine != null) StopCoroutine(chargeAudioRoutine);
                    chargeAudioRoutine = StartCoroutine(HandleChargeSounds());
                }
                else
                {
                    return;
                }
            }

            if (isCharging && Time.time - chargeStartTime >= 2.3f)
            {
                animationController.SetChargingLoop(true);
                animationController.SetChargeStart(false);
            }

            // Depleting stamina while charging
            if (isCharging && !staminaBar.IsEmpty)
            {
                float staminaUsed = staminaDrainRate * Time.deltaTime;
                staminaDepleted += staminaUsed;
                staminaBar.DepleteStamina(staminaUsed);
            }

            // Trigger charge attack once stamina is empty
            if (isCharging && staminaBar.IsEmpty && !hasFiredChargeAttack)
            {
                TriggerChargeAttackSequence();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isCharging && !hasFiredChargeAttack)
            {
                TriggerChargeAttackSequence();
            }

            if (hasFiredChargeAttack)
            {
                isCharging = false;
                animationController.SetChargeStart(false);
                animationController.SetChargingLoop(false);
            }
        }
    }

    private IEnumerator HandleChargeSounds()
    {
        // Wait and play charging loop sound
        yield return new WaitForSeconds(chargingSoundDelay);
        if (isCharging)
            AudioManager.Instance.PlayerChargingRangeAttack();

        // Wait for sword draw
        float waitForSwordDraw = swordDrawDelay - chargingSoundDelay;
        if (waitForSwordDraw > 0)
            yield return new WaitForSeconds(waitForSwordDraw);

        if (isCharging && !playedSwordDraw)
        {
            AudioManager.Instance.PlayerChargingSwordDraw();
            playedSwordDraw = true;
        }

        // Wait for climax
        float waitForClimax = climaxDelay - swordDrawDelay;
        if (waitForClimax > 0)
            yield return new WaitForSeconds(waitForClimax);

        if (isCharging && !playedClimax)
        {
            AudioManager.Instance.PlayerChargingClimax();
            playedClimax = true;
        }
    }


    private void TriggerChargeAttackSequence()
    {
        if (hasFiredChargeAttack) return; // Prevent firing more than once

        // Stop charging and reset charge-related animation states
        isCharging = false;
        animationController.SetChargeStart(false);
        animationController.SetChargingLoop(false);
        animationController.SetChargeAttack(); // Trigger the charge attack animation

        // Mark that the charge attack has been fired
        hasFiredChargeAttack = true;

        // Fire the projectile after a short delay
        StartCoroutine(FireChargeProjectileAfterDelay());
        StartCoroutine(ResetChargeAttackState());
    }

    private IEnumerator FireChargeProjectileAfterDelay()
    {
        yield return new WaitForSeconds(chargeProjectileDelay);

        if (chargeProjectilePrefab != null && projectileSpawnPoint != null)
        {
            float chargeDuration = Time.time - chargeStartTime;
            float maxChargeTime = 9f;

            // Clamp duration and calculate interpolation factor
            float t = Mathf.Clamp01(chargeDuration / maxChargeTime);

            // Calculate scale and damage using Lerp
            float minScale = 0.1f;
            float maxScale = 0.3f;
            float finalScale = Mathf.Lerp(minScale, maxScale, t);

            int damage = Mathf.RoundToInt(Mathf.Lerp(5f, 50f, t));

            // Adjust spawn position based on charge duration
            Vector3 spawnPosition = projectileSpawnPoint.position;
            if (chargeDuration >= 4.5f) // Mid to max charge raises position
            {
                spawnPosition.y += Mathf.Lerp(0f, 2f, (chargeDuration - 4.5f) / (maxChargeTime - 4.5f));
            }

            GameObject projectile = Instantiate(chargeProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.localScale = new Vector3(finalScale, finalScale, 1f);

            ChargeProjectile cp = projectile.GetComponent<ChargeProjectile>();
            if (cp != null)
            {
                bool isFacingRight = transform.localScale.x > 0f;
                cp.damage = damage;
                cp.Launch(isFacingRight);
            }
        }
    }

    private IEnumerator ResetChargeAttackState()
    {
        // Wait for a short period (to allow any lingering animation or effects to finish)
        yield return new WaitForSeconds(0.5f);

        // Reset the animation states
        animationController.ResetChargeAttack();
        animationController.SetChargingLoop(false); // Explicitly reset ChargingLoop to false

        // Allow firing the next charge attack
        hasFiredChargeAttack = false; // Reset so you can charge again

        // Reset stamina depletion tracker for next charge cycle
        staminaDepleted = 0f;
    }

    public void SetAttackEnabled(bool enabled)
    {
        isAttackEnabled = enabled;
        if (!enabled && IsAttacking)
            CancelCurrentAttack();
    }

    private bool CanAttack()
    {
        return isAttackEnabled &&
               !IsAttacking &&
               (attackCount == 0 || Time.time - lastAttackEndTime <= attackResetTime) &&
               (!requireGrounded || IsGrounded());
    }

    private bool IsGrounded()
    {
        return movementController != null && movementController.isGrounded;
    }

    private void PerformAttack()
    {
        if (attackCount >= attackDurations.Length)
        {
            ResetCombo();
            return;
        }

        lastAttackTime = Time.time;
        IsAttacking = true;
        attackCount++;

        animationController.SetAttackState(attackCount);
        AudioManager.Instance.PlayPlayerAttackSwing(attackCount - 1);

        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);
        currentAttackRoutine = StartCoroutine(ExecuteAttack(attackCount - 1));
    }

    private IEnumerator ExecuteAttack(int attackIndex)
    {
        yield return new WaitForSeconds(hitboxEnableDelays[attackIndex]);

        if (!requireGrounded || IsGrounded())
        {
            attackCollider.offset = colliderOffsets[attackIndex];
            attackCollider.size = colliderSizes[attackIndex];
            attackCollider.enabled = true;

            yield return new WaitForSeconds(hitboxActiveTimes[attackIndex]);
            attackCollider.enabled = false;
        }

        attackCollider.offset = originalOffset;
        attackCollider.size = originalSize;

        float remainingTime = attackDurations[attackIndex] - (hitboxEnableDelays[attackIndex] + hitboxActiveTimes[attackIndex]);

        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        IsAttacking = false;
        lastAttackEndTime = Time.time;

        if (attackIndex < attackDurations.Length - 1)
            animationController.SetAttackState(0);
        else
            ResetCombo();
    }

    private void CancelCurrentAttack()
    {
        if (currentAttackRoutine != null)
            StopCoroutine(currentAttackRoutine);

        attackCollider.enabled = false;
        attackCollider.offset = originalOffset;
        attackCollider.size = originalSize;

        IsAttacking = false;
        animationController.SetAttackState(0);
    }

    private void ResetCombo()
    {
        attackCount = 0;
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
