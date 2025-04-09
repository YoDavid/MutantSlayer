using System.Collections;
using UnityEngine;

public class EnemyHealth : HealthSystem
{
    [Header("Enemy Settings")]
    public EnemyConfig config;
    [SerializeField] private bool overrideHealth = false;
    [SerializeField] private int customMaxHealth = 30;

    private Animator animator;

    protected override void Awake()
    {
        MaxHealth = overrideHealth ? customMaxHealth : config.maxHealth;
        base.Awake();
        animator = GetComponent<Animator>();
        MaxHealth = config.maxHealth;
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        if (CurrentHealth <= 0) return;

        base.TakeDamage(damage, isCritical);

        DamagePopUp.Instance?.CreateDamageText(
            damage, transform.position + Vector3.up * 1.5f,
            isPlayer: false, isBoss: false, isCritical);

        if (CurrentHealth <= 0) Die();
    }

    protected override void Die()
    {
        GetComponent<Collider2D>().enabled = false;
        base.Die();
        Destroy(gameObject, 1f);
    }

    protected override IEnumerator BlinkEffect()
    {
        for (int i = 0; i < config.enemyBlinkCount; i++)
        {
            spriteRenderer.color = config.enemyBlinkColor;
            yield return new WaitForSeconds(config.enemyBlinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(config.enemyBlinkDuration);
        }
    }
}