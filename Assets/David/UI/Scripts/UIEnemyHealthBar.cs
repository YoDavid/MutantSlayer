using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class UIEnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;
    [SerializeField] private float visibilityDistance = 10f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Enemy Type Offsets")]
    [SerializeField] private float smallEnemyYOffset = 50f;
    [SerializeField] private float mediumEnemyYOffset = 70f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public EnemyHealth enemyHealth;
    private Camera mainCamera;
    private float currentYOffset;
    private Transform playerTransform;
    private float currentFadeTime;
    private bool isFading;

    // Public access to visibility state
    public bool IsInRangeToShowHealth { get; private set; }

    public void Initialize(EnemyHealth health, Camera cam)
    {
        enemyHealth = health;
        mainCamera = cam;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        CameraParallax parallaxCam = FindObjectOfType<CameraParallax>();
        if (parallaxCam != null)
        {
            mainCamera = parallaxCam.GetComponent<Camera>();
        }

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Set Y-offset based on enemy type
        currentYOffset = enemyHealth.enemyType switch
        {
            EnemyHealth.EnemyType.Small => smallEnemyYOffset,
            EnemyHealth.EnemyType.Medium => mediumEnemyYOffset,
            _ => smallEnemyYOffset
        };

        healthSlider.maxValue = enemyHealth.config.maxHealth;
        healthSlider.value = enemyHealth.config.maxHealth;
        fillImage.color = fullHealthColor;

        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += HandleEnemyDeath;

        // Start fully transparent
        canvasGroup.alpha = 0f;
        IsInRangeToShowHealth = false;
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor,
                                   (float)currentHealth / enemyHealth.config.maxHealth);
    }

    private void HandleEnemyDeath()
    {
        // Fade out and then destroy
        StartCoroutine(FadeOutAndDestroy());
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        if (enemyHealth == null || mainCamera == null) return;

        // Always update position regardless of visibility
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemyHealth.transform.position);
        rectTransform.position = new Vector3(screenPosition.x, screenPosition.y + currentYOffset, screenPosition.z);

        // Check distance if we have a player reference
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(playerTransform.position, enemyHealth.transform.position);
            bool shouldBeVisible = distanceToPlayer <= visibilityDistance;

            // Only handle fade if visibility state changed
            if (shouldBeVisible != IsInRangeToShowHealth)
            {
                IsInRangeToShowHealth = shouldBeVisible;
                isFading = true;
                currentFadeTime = 0f;
            }

            // Handle fade in/out
            if (isFading)
            {
                currentFadeTime += Time.deltaTime;
                float progress = Mathf.Clamp01(currentFadeTime / fadeDuration);
                canvasGroup.alpha = IsInRangeToShowHealth ? progress : 1 - progress;

                if (currentFadeTime >= fadeDuration)
                {
                    isFading = false;
                }
            }
        }
        else
        {
            // Try to find player again if missing
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else if (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = 0;
                IsInRangeToShowHealth = false;
            }
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