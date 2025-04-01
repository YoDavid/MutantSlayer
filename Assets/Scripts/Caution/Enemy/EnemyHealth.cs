using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyConfig config;
    public int currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // Added this line
    private Color originalColor; // Added this line

    public event System.Action OnDeath;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Added this line
        originalColor = spriteRenderer.color; // Added this line
        currentHealth = config.maxHealth;
    }
    public void TakeDamage(int damage)
    {
        // Apply damage
        currentHealth -= damage;

        // Visual feedback
        StartCoroutine(BlinkEffect());

        // Handle death
        if (currentHealth <= 0) Die();
    }


    private void Die()
    {
        GetComponent<Collider2D>().enabled = false;
        OnDeath?.Invoke();
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
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