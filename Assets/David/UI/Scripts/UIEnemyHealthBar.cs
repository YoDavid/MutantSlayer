using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;

    // Separate Y-offsets for different enemy types
    [SerializeField] private float smallEnemyYOffset = 50f;
    [SerializeField] private float mediumEnemyYOffset = 70f;

    private RectTransform rectTransform;
    public EnemyHealth enemyHealth;
    private Camera mainCamera;
    private float currentYOffset; // Stores the correct offset based on enemy type

    public void Initialize(EnemyHealth health, Camera cam)
    {
        enemyHealth = health;
        mainCamera = cam;
        rectTransform = GetComponent<RectTransform>();

        CameraParallax parallaxCam = FindObjectOfType<CameraParallax>();
        if (parallaxCam != null)
        {
            mainCamera = parallaxCam.GetComponent<Camera>();
        }

        // Set the correct Y-offset based on enemy type
        if (enemyHealth.enemyType == EnemyHealth.EnemyType.Small)
            currentYOffset = smallEnemyYOffset;
        else if (enemyHealth.enemyType == EnemyHealth.EnemyType.Medium)
            currentYOffset = mediumEnemyYOffset;
        else
        {
            currentYOffset = smallEnemyYOffset; // Default fallback
        }

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
            rectTransform.position = new Vector3(screenPosition.x, screenPosition.y + currentYOffset, screenPosition.z);
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