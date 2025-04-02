using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyConfig config;
    public int currentHealth { get; private set; } // Encapsulated with public get
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Events
    public event System.Action OnDeath;
    public event System.Action<int> OnHealthChanged; // Added for health bar updates

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        InitializeHealth();
    }

    private void InitializeHealth()
    {
        currentHealth = config.maxHealth;
        OnHealthChanged?.Invoke(currentHealth); // Initialize health bar
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // ADD THIS LINE FOR DAMAGE POPUP:
        DamagePopUp.Instance?.CreateDamageText(damage, transform.position + Vector3.up * 1.5f, isPlayer: false, isBoss: false);

        OnHealthChanged?.Invoke(currentHealth);
        StartCoroutine(BlinkEffect());

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        GetComponent<Collider2D>().enabled = false;
        OnDeath?.Invoke();
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject); // HealthBar will be destroyed automatically as child
    }

    private IEnumerator BlinkEffect()
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