// UIEnemyHealthBar.cs
using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;
    [SerializeField] private float yOffset = 50f;

    private RectTransform rectTransform;
    public EnemyHealth enemyHealth;
    private Camera mainCamera;

    public void Initialize(EnemyHealth health, Camera cam)
    {
        enemyHealth = health;
        mainCamera = cam;
        rectTransform = GetComponent<RectTransform>();

        healthSlider.maxValue = enemyHealth.config.maxHealth;
        healthSlider.value = enemyHealth.config.maxHealth;
        fillImage.color = fullHealthColor;

        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += HandleEnemyDeath;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor,
                                   (float)currentHealth / enemyHealth.config.maxHealth);
    }

    private void HandleEnemyDeath()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 1f); // Optional delay for any fade effects
    }

    private void Update()
    {
        if (enemyHealth != null && mainCamera != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemyHealth.transform.position);
            rectTransform.position = new Vector3(screenPosition.x, screenPosition.y + yOffset, screenPosition.z);
        }
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