using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public int maxHealth = 500;
    [SerializeField] private Color blinkColor = Color.white; // Customizable in Inspector
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0);
    private Coroutine blinkRoutine;

    [SerializeField] private GameObject bloodSplashPrefab;

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

    public void TakeDamage(int damage, bool isCritical = false)
    {
        currentHealth -= damage;

        OnHealthChanged?.Invoke(currentHealth); // <- Add this

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
            spriteRenderer.color = blinkColor; // Flash red
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor; // Revert
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void Die()
    {
        if (bloodSplashPrefab != null)
        {
            Instantiate(bloodSplashPrefab, transform.position, Quaternion.identity);
        }

        OnDeath?.Invoke(); // <- Add this
        Destroy(gameObject);
    }
}