using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private Color blinkColor = Color.white; // Customizable in Inspector
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 3;
    private Coroutine blinkRoutine;

    public int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine); // Cancel previous blink
        blinkRoutine = StartCoroutine(BlinkEffect());

        if (currentHealth <= 0) Die();
    }

    private IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = blinkColor; // Flash red
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor; // Revert
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated!");
        Destroy(gameObject); // Replace with death animation later
    }
}