using System.Collections;
using UnityEngine;

public class BossHealthTesting : HealthSystem
{
    [Header("Settings")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0);

    private Coroutine blinkRoutine;

    protected override void Awake()
    {
        base.Awake();  // Inherits from HealthSystem to initialize health
    }

    public override void TakeDamage(int damage, bool isCritical = false)
    {
        base.TakeDamage(damage, isCritical);  // Calls HealthSystem's TakeDamage to handle health reduction

        // Show damage popup
        if (DamagePopUp.Instance != null)
        {
            DamagePopUp.Instance.CreateDamageText(
                damage,
                transform.position + popupOffset,
                isPlayer: false,
                isBoss: true,
                isCritical: isCritical
            );
        }

        // Handle blink effect
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkEffect());

        if (CurrentHealth <= 0) Die();
    }

    protected override void Die()
    {
        base.Die();  

        Destroy(gameObject);
    }

    protected override IEnumerator BlinkEffect()
    {
        // Custom blink effect for the boss
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = damageBlinkColor; // Flash red
            yield return new WaitForSeconds(damageBlinkDuration);
            spriteRenderer.color = originalColor; // Revert
            yield return new WaitForSeconds(damageBlinkDuration);
        }
    }
}
