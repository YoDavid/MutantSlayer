using UnityEngine;

public class FaceAnchor : MonoBehaviour
{
    [Header("Base Settings")]
    public Vector3 baseOffset = new Vector3(0, 1.5f, -10);
    public float followSharpness = 15f;
    public bool snapAllTransitions = true;

    [Header("Animation Offsets")]
    public Vector3 idleOffset = new Vector3(0f, 0, -10);
    public Vector3 runOffset = new Vector3(4f, -0.6f, -10);
    public Vector3 jumpOffset = new Vector3(3.2f, 0.8f, -10);
    public Vector3 fallOffset = new Vector3(3.2f, 0.8f, -10);
    public Vector3 dashOffset = new Vector3(-1f, -4.5f, -10);

    [Header("Attack Offsets")]
    public Vector3 attack1Offset = new Vector3(3f, -2.15f, -10);
    public Vector3 attack2Offset = new Vector3(0.6f, -3.3f, -10);
    public Vector3 attack3Offset = new Vector3(6f, -3f, -10);

    [Header("Ranged Attack Offsets")]
    public Vector3 rangedChargeOffset = new Vector3(0f, -4.5f, -10);
    public Vector3 rangedReleaseOffset = new Vector3(2f, -1f, -10);

    [Header("Combo Attack Camera")]
    public Vector3 comboStartOffset = new Vector3(0f, -4.5f, -10);
    public Vector3 comboActiveOffset = new Vector3(1.5f, -0.5f, -10);
    [SerializeField] private float comboStartDuration = 6f; // Seconds before snapping to active position

    [Header("Combo Camera Size Control")]
    [SerializeField] private float normalCameraSize = 0.5f;
    [SerializeField] private float comboActiveCameraSize = 0.7f; // New size during active phase
    [SerializeField] private float sizeChangeSpeed = 5f; // How fast size changes

    private Camera targetCamera;
    private float currentTargetSize;
    private float comboStateEnterTime;
    private bool isInComboState;
    private bool isPastComboStartPhase;

    [Header("Reaction Offsets")]
    public Vector3 hitOffset = new Vector3(-2f, 1f, -10);
    public Vector3 healOffset = new Vector3(0f, 2.5f, -10);

    [Header("Hit Reaction Settings")]
    public float hitFreezeDuration = 0.3f;
    public AnimationCurve hitRecoveryCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Animator animator;
    private Vector3 targetPosition;
    private Vector3 hitTargetPosition;
    private float hitFreezeTimer;
    private bool isInHitReaction;
    private string currentState;
    private string previousState;
    private bool hitTriggerProcessed = false;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        currentTargetSize = normalCameraSize;
        animator = GetComponentInParent<Animator>();
        targetPosition = baseOffset;
    }

    void LateUpdate()
    {
        bool nowInCombo = animator.GetBool("ComboAttackStart") ||
                         animator.GetBool("IsComboAttacking");

        // Handle combo state transitions
        if (nowInCombo && !isInComboState)
        {
            isInComboState = true;
            isPastComboStartPhase = false;
            comboStateEnterTime = Time.time;
        }
        else if (!nowInCombo && isInComboState)
        {
            isInComboState = false;
            isPastComboStartPhase = false;
        }

        previousState = currentState;
        currentState = GetCurrentAnimationState();

        HandleHitReaction();

        if (!isInHitReaction)
        {
            UpdateNormalPosition();
        }

        UpdateCameraSize();
    }


    void UpdateCameraSize()
    {
        if (isInComboState && isPastComboStartPhase)
        {
            currentTargetSize = comboActiveCameraSize;
        }
        else
        {
            currentTargetSize = normalCameraSize;
        }

        if (targetCamera != null)
        {
            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize,
                currentTargetSize,
                sizeChangeSpeed * Time.deltaTime
            );
        }
    }

    string GetCurrentAnimationState()
    {
        if (isInHitReaction) return "Hit";
        if (animator.GetBool("IsHealing")) return "Heal";

        // Combo attack states
        if (animator.GetBool("ComboAttackStart") || animator.GetBool("IsComboAttacking"))
            return "ComboAttack";

        // Ranged attack states
        if (animator.GetBool("RangedAttackAttack")) return "RangedRelease";
        if (animator.GetBool("RangedAttackStart") || animator.GetBool("RangedAttackLoop"))
            return "RangedCharge";

        // Normal attacks
        if (animator.GetBool("IsAttacking"))
            return "Attack" + animator.GetInteger("AttackCount");

        // Movement states
        if (animator.GetBool("IsJumping")) return "Jump";
        if (animator.GetBool("IsFalling")) return "Fall";
        if (animator.GetBool("IsDashing")) return "Dash";
        if (animator.GetFloat("Speed") > 0.1f) return "Run";

        return "Idle";
    }

    Vector3 GetCurrentOffset()
    {
        if (isInComboState)
        {
            // Check if we've passed the start phase duration
            if (!isPastComboStartPhase &&
                Time.time >= comboStateEnterTime + comboStartDuration)
            {
                isPastComboStartPhase = true;
            }

            return isPastComboStartPhase ? comboActiveOffset : comboStartOffset;
        }

        switch (currentState)
        {
            case "Hit": return hitOffset;
            case "Heal": return healOffset;

            // Combo attacks
            case "ComboStart": return comboStartOffset;
            case "ComboActive": return comboActiveOffset;

            // Ranged attacks
            case "RangedCharge": return rangedChargeOffset;
            case "RangedRelease": return rangedReleaseOffset;

            // Normal attacks
            case "Attack1": return attack1Offset;
            case "Attack2": return attack2Offset;
            case "Attack3": return attack3Offset;

            // Movement
            case "Jump": return jumpOffset;
            case "Fall": return fallOffset;
            case "Dash": return dashOffset;
            case "Run": return runOffset;

            default: return idleOffset;
        }
    }

    void HandleHitReaction()
    {
        bool hitTriggered = animator.GetBool("TakenHit");

        if (!hitTriggered)
        {
            hitTriggerProcessed = false;
            return;
        }

        if (!hitTriggerProcessed && !isInHitReaction)
        {
            StartHitReaction();
            hitTriggerProcessed = true;
        }

        if (isInHitReaction)
        {
            hitFreezeTimer -= Time.deltaTime;
            if (hitFreezeTimer <= 0)
            {
                isInHitReaction = false;
                // Force immediate position update on next frame
                snapAllTransitions = true;
            }
            transform.localPosition = hitTargetPosition;
        }
    }

    void StartHitReaction()
    {
        isInHitReaction = true;
        hitFreezeTimer = hitFreezeDuration;
        hitTargetPosition = baseOffset + hitOffset;
        transform.localPosition = hitTargetPosition;
    }

    void UpdateNormalPosition()
    {
        Vector3 currentOffset = GetCurrentOffset();
        targetPosition = baseOffset + currentOffset;

        if (snapAllTransitions || StateChanged() || IsAttackState())
        {
            transform.localPosition = targetPosition;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                followSharpness * Time.deltaTime
            );
        }
    }

    bool StateChanged() => currentState != previousState;
    bool IsAttackState() => currentState.Contains("Attack") || currentState.Contains("Combo");

    public void OnHitAnimationTriggered()
    {
        if (!isInHitReaction)
        {
            StartHitReaction();
        }
    }
}