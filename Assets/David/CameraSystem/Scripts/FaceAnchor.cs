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
    public Vector3 slideOffset = new Vector3(-1f, -4.5f, -10);
    public Vector3 chargeStartOffset = new Vector3(0f, 0.5f, -10);
    public Vector3 chargeAttackOffset = new Vector3(2f, -1f, -10);
    public Vector3 attack1Offset = new Vector3(3f, -2.15f, -10);
    public Vector3 attack2Offset = new Vector3(0.6f, -3.3f, -10);
    public Vector3 attack3Offset = new Vector3(6f, -3f, -10);
    public Vector3 takenHitOffset = new Vector3(-2f, 1f, -10);
    public Vector3 healingOffset = new Vector3(0f, 2.5f, -10);

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

    // Track if we've processed the current hit
    private bool hitTriggerProcessed = false;
    private int lastHitFrame = -1;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
        targetPosition = baseOffset;
    }

    void LateUpdate()
    {
        previousState = currentState;
        currentState = GetCurrentState();

        // Check for TakenHit trigger in the animator
        bool hitTriggered = animator.GetBool("TakenHit");

        // Reset hit tracking if trigger is no longer active
        if (!hitTriggered)
        {
            hitTriggerProcessed = false;
        }

        // Handle hit reaction timing - only when the trigger is first detected
        if (hitTriggered && !hitTriggerProcessed && !isInHitReaction)
        {
            hitTriggerProcessed = true;
            StartHitReaction();
        }

        if (isInHitReaction)
        {
            UpdateHitReaction();
            return; // Skip normal camera updates during hit reaction
        }

        UpdateNormalCameraPosition();
    }

    void StartHitReaction()
    {
        isInHitReaction = true;
        hitFreezeTimer = hitFreezeDuration;
        // Calculate the hit target position including the offset
        hitTargetPosition = baseOffset + takenHitOffset;
        // Snap immediately to hit position
        transform.localPosition = hitTargetPosition;
    }

    void UpdateHitReaction()
    {
        hitFreezeTimer -= Time.deltaTime;

        if (hitFreezeTimer <= 0)
        {
            isInHitReaction = false;
            return;
        }

        // Keep camera at hit offset position
        transform.localPosition = hitTargetPosition;
    }

    void UpdateNormalCameraPosition()
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

    string GetCurrentState()
    {
        if (isInHitReaction) return "Hit";
        if (animator.GetBool("IsHealing")) return "Heal";
        if (animator.GetBool("ChargeAttack")) return "ChargeAttack";
        if (animator.GetBool("ChargeStart") || animator.GetBool("ChargingLoop")) return "ChargeStart";
        if (animator.GetBool("IsAttacking")) return "Attack" + animator.GetInteger("AttackCount");
        if (animator.GetBool("IsJumping")) return "Jump";
        if (animator.GetBool("IsFalling")) return "Fall";
        if (animator.GetBool("IsDashing")) return "Dash";
        if (animator.GetFloat("Speed") > 0.1f) return "Run";
        return "Idle";
    }

    Vector3 GetCurrentOffset()
    {
        switch (currentState)
        {
            case "Hit": return takenHitOffset;
            case "Heal": return healingOffset;
            case "ChargeAttack": return chargeAttackOffset;
            case "ChargeStart": return chargeStartOffset;
            case "Attack1": return attack1Offset;
            case "Attack2": return attack2Offset;
            case "Attack3": return attack3Offset;
            case "Jump": return jumpOffset;
            case "Fall": return fallOffset;
            case "Dash": return slideOffset;
            case "Run": return runOffset;
            default: return idleOffset;
        }
    }

    public void OnHitAnimationTriggered()
    {
        if (!isInHitReaction)
        {
            StartHitReaction();
        }
    }

    bool StateChanged() => currentState != previousState;
    bool IsAttackState() => currentState.StartsWith("Attack");
}