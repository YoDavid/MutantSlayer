using UnityEngine;
using UnityEngine.UI;

public class UIBossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;

    private BossHealth bossHealth;

    private void Awake()
    {
        bossHealth = FindObjectOfType<BossHealth>();
        if (bossHealth == null)
        {
            Debug.LogError("BossHealth not found in scene!");
            return;
        }

        InitializeHealthBar();
    }

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged += UpdateHealthBar;
            bossHealth.OnDeath += HandleBossDeath;
        }
    }

    private void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
            bossHealth.OnDeath -= HandleBossDeath;
        }
    }

    private void InitializeHealthBar()
    {
        healthSlider.maxValue = bossHealth.maxHealth;
        healthSlider.value = bossHealth.maxHealth;
        fillImage.color = fullHealthColor;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        float t = (float)currentHealth / bossHealth.maxHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, t);
    }

    private void HandleBossDeath()
    {
        gameObject.SetActive(false); // hide on death
    }
}
