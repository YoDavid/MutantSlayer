using UnityEngine;

public class PlayerHealth : HealthSystem
{
    [Header("Player Status")]
    public bool isDead = false;

    [Header("Player Settings")]
    [SerializeField] private int playerMaxHealth = 100;

    private PlayerDamageBlink damageBlink;
    private PlayerMovementController playerMovement;
    private PlayerHurtbox playerHurtbox;

    protected override void Awake()
    {
        MaxHealth = playerMaxHealth;
        base.Awake();
        damageBlink = GetComponent<PlayerDamageBlink>();
        playerMovement = GetComponent<PlayerMovementController>();
        playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (isDead || playerHurtbox == null || !playerHurtbox.enabled) return;

        base.TakeDamage(damage, isCritical);

        DamagePopUp.Instance?.CreateDamageText(
            damage, transform.position + Vector3.up * 1.8f,
            isPlayer: true, isBoss: false, isCritical);

        damageBlink?.TriggerBlinkEffect();
    }

    protected override void Die()
    {
        isDead = true;
        base.Die();
        Debug.Log("Player died!");
    }

    public bool IsPlayerInvulnerable() => playerMovement.isDashing;

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}