using UnityEngine;
using System.Collections;

public class PlayerHealth : HealthSystem
{
    [Header("Player Status")]
    public bool isDead = false;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    private PlayerMovementController playerMovement;
    private PlayerHurtbox playerHurtbox;
    private PlayerAnimationController playerAnimation;
    private bool isInvulnerable = false;

    protected override void Awake()
    {
        // No need to set MaxHealth here - it's set in the inspector
        base.Awake();
        playerMovement = GetComponent<PlayerMovementController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
        playerAnimation = GetComponent<PlayerAnimationController>();
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDead || isInvulnerable || playerHurtbox == null || !playerHurtbox.enabled) return;

        base.TakeDamage(damage, isCritical);

        playerAnimation.TriggerTakenHit();
        DamagePopUp.Instance?.CreateDamageText(
            damage, transform.position + Vector3.up * 1.8f,
            isPlayer: true, isBoss: false, isCritical);

        StartCoroutine(InvulnerabilityFrame());
    }

    private IEnumerator InvulnerabilityFrame()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        playerAnimation.enabled = false;
        playerMovement.enabled = false;
        base.Die();
       
        Debug.Log("Player died!");
    }

    public bool IsPlayerInvulnerable() => playerMovement.isDashing || isInvulnerable;

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}