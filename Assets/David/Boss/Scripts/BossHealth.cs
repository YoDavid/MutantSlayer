using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    [SerializeField] public int maxHealth = 500;
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0);
    private Coroutine blinkRoutine;

    [SerializeField] private BloodSplashParticlesPool bloodSplashPool;

    public int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public event System.Action<int> OnHealthChanged;
    public event System.Action OnDeath;
    public int MaxHealth => maxHealth;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, bool isCritical = false, bool isCombo = false, int comboCount = 0)
    {
        currentHealth -= damage;

        OnHealthChanged?.Invoke(currentHealth);

        if (DamagePopUp.Instance != null)
        {
            Vector3 spawnPosition = transform.position + popupOffset;

            if (isCombo)
            {
                // Apply combo-specific offset
                spawnPosition += new Vector3(
                    comboCount * 0.5f,  // Horizontal spacing
                    comboCount * 0.3f,  // Vertical offset
                    0
                );
            }
            else
            {
                // Add some randomness for regular hits
                spawnPosition += new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0,
                    0
                );
            }

            DamagePopUp.Instance.CreateDamageText(
                damage,
                spawnPosition,
                isPlayer: false,
                isBoss: true,
                isCritical: isCritical,
                isCombo: isCombo,
                comboIndex: comboCount
            );
        }

        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator BlinkEffect()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void Die()
    {
        if (bloodSplashPool != null)
        {
            bloodSplashPool.PlayDeathSplash(transform.position);
        }
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}