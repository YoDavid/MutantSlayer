using UnityEngine;
using UnityEngine.UI;

public class UIPlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float colorChangeThreshold = 0.3f;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = playerHealth.maxHealth;
    }

    private void Update()
    {
        if (playerHealth == null) return;

        // Smooth health updates
        healthSlider.value = Mathf.Lerp(healthSlider.value,
                                      playerHealth.currentHealth,
                                      10f * Time.deltaTime);

        // Color change based on health
        float healthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        fillImage.color = Color.Lerp(criticalColor,
                                   healthyColor,
                                   healthPercent / colorChangeThreshold);
    }

    public void OnPlayerHealthChanged(int newHealth)
    {
        // Optional: Can be called via events instead of Update
        healthSlider.value = newHealth;
    }
}
