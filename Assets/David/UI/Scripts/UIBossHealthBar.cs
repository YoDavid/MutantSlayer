using UnityEngine;
using UnityEngine.UI;

public class UIBossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;

    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossLevelScaling bossLevelScaling;

    private void Awake()
    {
        bossHealth = FindObjectOfType<BossHealth>();
        bossLevelScaling = FindObjectOfType<BossLevelScaling>();

        if (bossHealth == null || bossLevelScaling == null)
        {
            Debug.LogError("Boss references not found!");
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

        if (bossLevelScaling != null)
        {
            bossLevelScaling.OnLevelUp += UpdateHealthBarMaxHealth;
        }
    }

    private void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= UpdateHealthBar;
            bossHealth.OnDeath -= HandleBossDeath;
        }

        if (bossLevelScaling != null)
        {
            bossLevelScaling.OnLevelUp -= UpdateHealthBarMaxHealth;
        }
    }

    private void InitializeHealthBar()
    {
        healthSlider.maxValue = bossHealth.maxHealth;
        healthSlider.value = bossHealth.currentHealth;
        fillImage.color = fullHealthColor;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        healthSlider.maxValue = bossHealth.maxHealth; // Always update max in case of level up
        UpdateHealthColor();
    }

    private void UpdateHealthBarMaxHealth()
    {
        healthSlider.maxValue = bossHealth.maxHealth;
        UpdateHealthColor();
    }

    private void UpdateHealthColor()
    {
        float t = (float)bossHealth.currentHealth / bossHealth.maxHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, t);
    }

    private void HandleBossDeath()
    {
        gameObject.SetActive(false);
    }
}