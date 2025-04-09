using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealth component not found in parent!", this);
            return;
        }

        InitializeHealthBar();
        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += HandleEnemyDeath;
    }

    private void InitializeHealthBar()
    {
        healthSlider.maxValue = enemyHealth.config.maxHealth;
        healthSlider.value = enemyHealth.config.maxHealth;
        fillImage.color = fullHealthColor;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor,
                                   (float)currentHealth / enemyHealth.config.maxHealth);
    }

    private void HandleEnemyDeath()
    {
        // Optional: Add death animation to health bar
        gameObject.SetActive(false); // Or Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
            enemyHealth.OnDeath -= HandleEnemyDeath;
        }
    }
}