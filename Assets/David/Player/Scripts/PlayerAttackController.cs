using System.Collections;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
   
    #region Components and References
    private PlayerAnimationController animationController;
    private PlayerMovementController movementController;

    [Header("References")]
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private UIPlayerStaminaBar staminaBar;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject chargeProjectilePrefab;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private PlayerComboHitbox comboHitbox;
    #endregion

    #region Attack State Variables
    [SerializeField] private int attackCount;
    [SerializeField] private float lastAttackTime;
    [SerializeField] private float lastAttackEndTime;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    private Coroutine currentAttackRoutine;

    public bool IsAttacking { get; private set; }
    [SerializeField] private bool isAttackEnabled = true;
    #endregion

    #region Charge Attack Variables
    private bool isCharging = false;
    [SerializeField] private float chargeStartTime;
    private bool hasFiredChargeAttack = false;
    private float staminaDepleted = 0f;
    private Coroutine chargeAudioRoutine;
    private bool playedSwordDraw = false;
    private bool playedClimax = false;
    #endregion

    #region Combo Attack Variables
    [Header("Combo Settings")]
    [SerializeField] private float leftMouseButtonHoldTime = 0f;
    [SerializeField] private bool isComboAttackTriggered = false;

    [Header("Combo Audio Delays")]
    [SerializeField] private float comboChargingSoundDelay = 0.25f;
    [SerializeField] private float comboSwordDrawDelay = 0.4f;
    [SerializeField] private float comboClimaxDelay = 1.15f;
    [SerializeField] private float comboStaminaDrainRate = 25f;
    [SerializeField] private float comboStaminaDepleted = 0f;

    private bool playedComboSwordDraw = false;
    private bool playedComboClimax = false;
    #endregion

    #region Settings
    [Header("Normal Attacks Settings")]
    [SerializeField] private float attackResetTime = 0.8f;
    [SerializeField] private float[] attackDurations = { 0.35f, 0.35f, 0.3f };

    [Header("Ground Requirements")]
    [SerializeField] private bool requireGrounded = true;
    [SerializeField] private bool cancelAttackIfAirborne = true;

    [Header("Hitbox Settings (Per Attack)")]
    [SerializeField] private Vector2[] colliderOffsets;
    [SerializeField] private Vector2[] colliderSizes;
    [SerializeField] private float[] hitboxEnableDelays;
    [SerializeField] private float[] hitboxActiveTimes;

    [Header("Ranged Attack Settings")]
    [SerializeField] private float chargeStartDelay = 0.25f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float chargeProjectileDelay = 0.3f;

    [Header("Range Audio Delays")]
    [SerializeField] private float chargingSoundDelay = 0.25f;
    [SerializeField] private float swordDrawDelay = 0.4f;
    [SerializeField] private float climaxDelay = 1.15f;

    [Header("Electricity Loop Audio Settings")]
    [SerializeField] private float electricityLoopStartDelay = 0.2f; // Delay before first play
    [SerializeField] private float electricityLoopInterval = 0.3f; // Time between plays
    [SerializeField] private float electricityLoopStopDelay = 0.1f; // Delay after animation ends
    private Coroutine electricityLoopRoutine;
    private bool shouldPlayElectricityLoop = false;

    [SerializeField] private PlayerEarlyExitAttack earlyExitAttack;

    [SerializeField] private bool wasComboInterrupted = false;
    [SerializeField] private bool wasRangedInterrupted = false;
    [SerializeField] private bool requireNewRangedInput = false;

    #endregion

    #region Initialization
    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        movementController = GetComponent<PlayerMovementController>();
        earlyExitAttack = gameObject.GetComponentInChildren<PlayerEarlyExitAttack>();

        if (attackCollider != null)
        {
            originalOffset = attackCollider.offset;
            originalSize = attackCollider.size;
            attackCollider.enabled = false;
        }

        if (staminaBar == null)
            staminaBar = FindObjectOfType<UIPlayerStaminaBar>();

        if (projectileSpawnPoint == null)
            projectileSpawnPoint = transform.Find("ProjectilePos");
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        HandleNormalAttackInput();
        HandleComboAttackInput();
        HandleRangedAttackInput();

        if (!IsGrounded())
        {
            // If we were in a combo and became airborne, interrupt it
            if (isComboAttackTriggered)
            {
                InterruptCombo();
            }
            ResetComboInputState();
            return;
        }

        if (!IsGrounded() && animationController.animator.GetBool("RangedAttackLoop"))
        {
            ResetRangedAttack();
        }
    }
    #endregion

    #region Normal Attack Methods
    private void HandleNormalAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            PerformNormalAttack();
        }
    }

    private void PerformNormalAttack()
    {
        if (attackCount >= attackDurations.Length)
        {
            ResetNormalAttackCount();
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
            ResetNormalAttackCount();
    }
    #endregion

    #region Combo Attack Methods
    public void HandleComboAttackInput()
    {
        if (!IsGrounded())
        {
            ResetComboInputState();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            leftMouseButtonHoldTime += Time.deltaTime;

            if (leftMouseButtonHoldTime >= 0.25f && !isComboAttackTriggered && staminaBar.GetStamina() >= 25f)
            {
                animationController.SetComboAttackStart(true);
                animationController.SetIsComboAttacking(true);
                isComboAttackTriggered = true;

                // Start the combo hitbox
                if (comboHitbox != null)
                {
                    comboHitbox.OnComboStarted();
                }

                if (chargeAudioRoutine != null) StopCoroutine(chargeAudioRoutine);
                chargeAudioRoutine = StartCoroutine(HandleComboAttackSounds());
            }

            // Handle stamina consumption during combo attack
            if (isComboAttackTriggered && !staminaBar.IsEmpty)
            {
                float staminaUsed = comboStaminaDrainRate * Time.deltaTime;
                comboStaminaDepleted += staminaUsed;
                staminaBar.DepleteStamina(staminaUsed);

                if (staminaBar.IsEmpty)
                {
                    EndComboAttack();  // End combo attack when stamina runs out
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))  // Left mouse button released
        {
            ResetComboInputState();  // Reset hold time when button is released
            if (isComboAttackTriggered)
            {
                EndComboAttack();  // End combo when button is released
            }
        }
    }

    private IEnumerator HandleComboAttackSounds()
    {
        float elapsedTime = 0f;
        bool chargingSoundPlaying = false;

        // Wait for the combo charging sound delay
        yield return new WaitForSeconds(comboChargingSoundDelay);
        elapsedTime += comboChargingSoundDelay;

        // Check if combo attack conditions are false
        if (!animationController.animator.GetBool("ComboAttackStart") ||
            !animationController.animator.GetBool("IsComboAttacking"))
        {
            yield break; // Immediate exit if conditions fail
        }

        // Start charging sound
        AudioManager.Instance.PlayerChargingRangeAttack();
        chargingSoundPlaying = true;

        // Main sound sequence
        while (elapsedTime < 2.55f &&
               animationController.animator.GetBool("ComboAttackStart") &&
               animationController.animator.GetBool("IsComboAttacking"))
        {
            // Play sword draw sound at the right time
            if (!playedComboSwordDraw && elapsedTime >= comboSwordDrawDelay - comboChargingSoundDelay)
            {
                AudioManager.Instance.PlayerChargingSwordDraw();
                playedComboSwordDraw = true;
            }

            // Play climax sound at the right time
            if (!playedComboClimax && elapsedTime >= comboClimaxDelay - comboChargingSoundDelay)
            {
                AudioManager.Instance.PlayerChargingClimax();
                playedComboClimax = true;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop sound only when:
        // 1. 2.6 seconds have passed, OR
        // 2. Combo conditions become false
        if (chargingSoundPlaying)
        {
            AudioManager.Instance.StopSound("Player", "sfx_player_charge_range_attack");
        }

        // Reset input state when combo naturally completes
        if (elapsedTime >= 2.55f)
        {
            ResetComboInputState();
        }
    }

    private void EndComboAttack()
    {
        // Check if combo was interrupted by being hit and becoming airborne
        bool wasInterruptedByHit = !IsGrounded() && wasComboInterrupted;

        bool isEarlyExit = leftMouseButtonHoldTime < comboHitbox.windupDuration && !wasInterruptedByHit;

        animationController.StopComboAttack();
        isComboAttackTriggered = false;
        playedComboSwordDraw = false;
        playedComboClimax = false;
        comboStaminaDepleted = 0f;

        animationController.SetComboAttackStart(false);
        animationController.SetIsComboAttacking(false);

        if (isEarlyExit)
        {
            ResetComboInputState();
            animationController.SetEarlyComboExit(true);
            StartCoroutine(animationController.ResetEarlyComboExit());

            // Use the new early exit system
            if (earlyExitAttack != null)
            {
                earlyExitAttack.ExecuteEarlyExit();
            }
        }

        // Always end the combo normally too
        if (comboHitbox != null)
        {
            comboHitbox.OnComboEnded();
        }

        // Reset interruption flag
        wasComboInterrupted = false;
    }

    public void InterruptCombo()
    {
        if (isComboAttackTriggered)
        {
            wasComboInterrupted = true;
            EndComboAttack();
        }
    }

    private void ResetComboInputState()
    {
        leftMouseButtonHoldTime = 0f;
    }
    #endregion

    #region Ranged Attack Methods
    private void HandleRangedAttackInput()
    {
        bool isRightMouseHeld = Input.GetMouseButton(1);
        bool isRightMouseReleased = Input.GetMouseButtonUp(1);
        bool isRightMousePressed = Input.GetMouseButtonDown(1);

        // Reset charge start time if we're airborne and trying to charge
        if (!IsGrounded() && isRightMousePressed)
        {
            chargeStartTime = 0f;
            return;
        }

        // Full reset if we become airborne during any part of the ranged attack
        if (!IsGrounded() && (isCharging || hasFiredChargeAttack || animationController.animator.GetBool("RangedAttackLoop")))
        {
            ResetRangedAttack();
            return;
        }

        // If we need fresh input and button is still held, wait for release
        if (requireNewRangedInput && isRightMouseHeld)
        {
            return;
        }

        // Clear the requirement if button was released
        if (requireNewRangedInput && !isRightMouseHeld)
        {
            requireNewRangedInput = false;
        }

        // Only start new charge if we have fresh input and are grounded
        if (isRightMousePressed && !requireNewRangedInput && IsGrounded())
        {
            chargeStartTime = Time.time;
            wasRangedInterrupted = false; // Reset interruption flag on new press
        }

        // Handle charging only with fresh input and while grounded
        if (isRightMouseHeld && !requireNewRangedInput && IsGrounded())
        {
            // Don't start charging if we were interrupted until button is released
            if (wasRangedInterrupted)
                return;

            if (!isCharging && Time.time - chargeStartTime >= chargeStartDelay)
            {
                if (staminaBar.GetStamina() >= 25f)
                {
                    StartRangedCharge();
                }
            }

            // STAMINA DEPLETION - RESTORED FROM PREVIOUS VERSION
            if (isCharging && !staminaBar.IsEmpty)
            {
                float staminaUsed = staminaDrainRate * Time.deltaTime;
                staminaDepleted += staminaUsed;
                staminaBar.DepleteStamina(staminaUsed);

                if (staminaBar.IsEmpty && !hasFiredChargeAttack)
                {
                    TriggerRangedAttackSequence();
                }
            }

            if (isCharging && Time.time - chargeStartTime >= 2.3f)
            {
                animationController.SetRangedAttackLoop(true);
                animationController.SetRangedAttackStart(false);

                if (!shouldPlayElectricityLoop)
                {
                    StartElectricityLoop();
                }
            }
        }

        // Handle button release
        if (isRightMouseReleased)
        {
            if (isCharging && !hasFiredChargeAttack)
            {
                TriggerRangedAttackSequence();
            }
            else if (hasFiredChargeAttack)
            {
                CleanUpRangedAttack();
            }
        }
    }

    private IEnumerator HandleRangedAttackSounds()
    {
        float elapsedTime = 0f;
        bool chargingSoundPlaying = false;

        yield return new WaitForSeconds(chargingSoundDelay);
        elapsedTime += chargingSoundDelay;

        if (wasRangedInterrupted || !isCharging)
            yield break;

        // Play charging sound if conditions are met
        if (isCharging)
        {
            AudioManager.Instance.PlayerChargingRangeAttack();
            chargingSoundPlaying = true;
        }

        // Calculate sword draw wait time
        float waitForSwordDraw = swordDrawDelay - chargingSoundDelay;
        if (waitForSwordDraw > 0)
        {
            yield return new WaitForSeconds(waitForSwordDraw);
            elapsedTime += waitForSwordDraw;
        }

        // Play sword draw sound if conditions are met
        if (isCharging && !playedSwordDraw && elapsedTime < 2.3f)
        {
            AudioManager.Instance.PlayerChargingSwordDraw();
            playedSwordDraw = true;
        }

        // Calculate climax wait time
        float waitForClimax = climaxDelay - swordDrawDelay;
        if (waitForClimax > 0)
        {
            yield return new WaitForSeconds(waitForClimax);
            elapsedTime += waitForClimax;
        }

        // Play climax sound if conditions are met
        if (isCharging && !playedClimax && elapsedTime < 2.3f)
        {
            AudioManager.Instance.PlayerChargingClimax();
            playedClimax = true;
        }

        // Continue checking while charging
        while (isCharging && !hasFiredChargeAttack)
        {
            // Stop charging sound if we're past 2.3s AND not in attack loop state
            if (elapsedTime >= 2.3f && !animationController.animator.GetBool("RangedAttackLoop"))
            {
                if (chargingSoundPlaying)
                {
                    AudioManager.Instance.StopSound("Player", "sfx_player_charge_range_attack");
                    chargingSoundPlaying = false;
                }
            }
            // Keep playing if we're in attack loop state
            else if (elapsedTime >= 2.3f && animationController.animator.GetBool("RangedAttackLoop"))
            {
                if (!chargingSoundPlaying)
                {
                    AudioManager.Instance.PlayerChargingRangeAttack();
                    chargingSoundPlaying = true;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final check to stop charging sound
        if (chargingSoundPlaying)
        {
            AudioManager.Instance.StopSound("Player", "sfx_player_charge_range_attack");
        }
    }

    private void TriggerRangedAttackSequence()
    {
        if (hasFiredChargeAttack || wasRangedInterrupted) return;

        isCharging = false;
        animationController.SetRangedAttackStart(false);
        animationController.SetRangedAttackLoop(false);
        animationController.SetRangedAttack();

        // Stop step sounds when firing ranged attack
        if (movementController != null)
        {
            movementController.StopStepSounds();
        }

        hasFiredChargeAttack = true;
        StopElectricityLoop();
        StartCoroutine(FireChargeProjectileAfterDelay());
        StartCoroutine(ResetRangedAttackState());
    }

    private void StartRangedCharge()
    {
        animationController.SetRangedAttackStart(true);
        isCharging = true;
        playedSwordDraw = false;
        playedClimax = false;
        wasRangedInterrupted = false;

        if (chargeAudioRoutine != null) StopCoroutine(chargeAudioRoutine);
        chargeAudioRoutine = StartCoroutine(HandleRangedAttackSounds());
    }

    private void CleanUpRangedAttack()
    {
        isCharging = false;
        animationController.SetRangedAttackStart(false);
        animationController.SetRangedAttackLoop(false);
        StopElectricityLoop();
    }


    private IEnumerator ElectricityLoopSoundRoutine()
    {
        yield return new WaitForSeconds(electricityLoopStartDelay);

        while (shouldPlayElectricityLoop && animationController.animator.GetBool("RangedAttackLoop"))
        {
            AudioManager.Instance.PlayPlayerChargeElectricityLoop();
            yield return new WaitForSeconds(electricityLoopInterval);
        }
    }

    private void StartElectricityLoop()
    {
        if (electricityLoopRoutine != null)
            StopCoroutine(electricityLoopRoutine);

        shouldPlayElectricityLoop = true;
        electricityLoopRoutine = StartCoroutine(ElectricityLoopSoundRoutine());
    }

    private void StopElectricityLoop()
    {
        shouldPlayElectricityLoop = false;

        if (electricityLoopRoutine != null)
        {
            StopCoroutine(electricityLoopRoutine);
            electricityLoopRoutine = null;
        }

        // Optional: Add a stop sound call if your audio system supports it
        // AudioManager.Instance.StopPlayerChargeElectricityLoop();
    }

    private IEnumerator FireChargeProjectileAfterDelay()
    {
        yield return new WaitForSeconds(chargeProjectileDelay);
        AudioManager.Instance.PlaySwing_00();
        AudioManager.Instance.PlayPlayerReleaseRangeGrunt();

        if (chargeProjectilePrefab != null && projectileSpawnPoint != null)
        {
            float chargeDuration = Time.time - chargeStartTime;
            float maxChargeTime = 9f;
            float t = Mathf.Clamp01(chargeDuration / maxChargeTime);

            float minScale = 0.1f;
            float maxScale = 0.4f;
            float finalScale = Mathf.Lerp(minScale, maxScale, t);

            int damage = Mathf.RoundToInt(Mathf.Lerp(5f, 50f, t));

            Vector3 spawnPosition = projectileSpawnPoint.position;
            if (chargeDuration >= 4.5f)
            {
                spawnPosition.y += Mathf.Lerp(0f, 2f, (chargeDuration - 4.5f) / (maxChargeTime - 4.5f));
            }

            GameObject projectile = Instantiate(chargeProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.localScale = new Vector3(finalScale, finalScale, 1f);

            ChargeProjectile cp = projectile.GetComponent<ChargeProjectile>();
            if (cp != null)
            {
                // Changed this to use the player's facing direction more reliably
                bool isFacingRight = movementController.IsFacingRight(); // Or your preferred method to get facing
                cp.damage = damage;
                cp.Launch(isFacingRight);
            }
        }
    }


    private IEnumerator ResetRangedAttackState()
    {
        yield return new WaitForSeconds(0.5f);
        animationController.ResetRangedAttack();
        animationController.SetRangedAttackLoop(false);
        hasFiredChargeAttack = false;
        wasRangedInterrupted = false;
        requireNewRangedInput = false; // Clear the input requirement
        staminaDepleted = 0f;
    }

    private void ResetRangedAttack()
    {
        // Reset all possible states
        isCharging = false;
        hasFiredChargeAttack = false;
        wasRangedInterrupted = true;
        requireNewRangedInput = true;
        chargeStartTime = 0f;
        staminaDepleted = 0f;

        // Reset animation states
        animationController.SetRangedAttackStart(false);
        animationController.SetRangedAttackLoop(false);
        animationController.ResetRangedAttack();

        // Stop all audio
        StopElectricityLoop();
        if (chargeAudioRoutine != null)
        {
            StopCoroutine(chargeAudioRoutine);
            chargeAudioRoutine = null;
        }
        AudioManager.Instance.StopSound("Player", "sfx_player_charge_range_attack");
        AudioManager.Instance.StopSound("Player", "sfx_player_charge_electricity_loop");

        // Reset sound flags
        playedSwordDraw = false;
        playedClimax = false;

        // Stop step sounds if playing
        movementController?.StopStepSounds();
    }
    #endregion

    #region Utility Methods
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
               (attackCount < 4 || Time.time - lastAttackEndTime <= attackResetTime) &&
               (!requireGrounded || IsGrounded());
    }

    private bool IsGrounded()
    {
        return movementController != null && movementController.isGrounded;
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

    private void ResetNormalAttackCount()
    {
        attackCount = 0;
        Debug.Log("Combo Reset");
        animationController.SetAttackState(0);
    }
    #endregion

    #region Gizmos
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
    #endregion
}