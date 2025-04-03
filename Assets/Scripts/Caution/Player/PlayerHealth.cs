using UnityEngine;
using System.Collections;

public class PlayerHealth : HealthSystem
{
    [Header("Player Status")]
    public bool isDead = false;

    [Header("Player Settings")]
    [SerializeField] private int playerMaxHealth = 100;
    [SerializeField] private float invulnerabilityTime = 0.5f;

    private PlayerMovementController playerMovement;
    private PlayerHurtbox playerHurtbox;
    private PlayerAnimationController playerAnimation;
    private bool isInvulnerable = false;

    protected override void Awake()
    {
        MaxHealth = playerMaxHealth;
        base.Awake();
        playerMovement = GetComponent<PlayerMovementController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
        playerAnimation = GetComponent<PlayerAnimationController>();
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDead || isInvulnerable || playerHurtbox == null || !playerHurtbox.enabled) return;

        base.TakeDamage(damage, isCritical);

        // Visual feedback and hit stun
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
        isDead = true;
        playerAnimation.enabled = false;
        base.Die();
        Debug.Log("Player died!");
    }

    public bool IsPlayerInvulnerable() => playerMovement.isDashing || isInvulnerable;

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}