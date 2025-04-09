using UnityEngine;

public class BossCooldownHandlerTesting : MonoBehaviour
{
    [Header("Cooldown Timers")]
    public float minAttackTime;
    public float maxAttackTime;
    public float attackCooldownTimer;
    public float jumpCooldownTimer;
    [SerializeField] private float maxJumpCooldown;

    public void Update()
    {
        HandleCooldowns();
    }

    void HandleCooldowns()
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }
    }

    public void ResetAttackCooldown()
    {
        attackCooldownTimer = Random.Range(minAttackTime, maxAttackTime);
    }

    public void ResetJumpCooldown()
    {
        jumpCooldownTimer = maxJumpCooldown;
    }
}
