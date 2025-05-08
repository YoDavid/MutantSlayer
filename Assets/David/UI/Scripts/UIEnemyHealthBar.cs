using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage; // Reference to background image
    [SerializeField] private Color fullHealthColor = Color.red;
    [SerializeField] private Color zeroHealthColor = Color.black;
    [SerializeField] private float visibilityDistance = 10f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Enemy Type Offsets")]
    [SerializeField] private float smallEnemyYOffset = 50f;
    [SerializeField] private float mediumEnemyYOffset = 70f;

    // Private references
    private RectTransform rectTransform;
    private Camera mainCamera;
    private Transform playerTransform;
    private float currentYOffset;
    private float currentAlpha = 0f;
    private bool needsFade = false;

    // Public references
    public EnemyHealth enemyHealth { get; set; }
    public EnemyLevelScaling enemyLevelScaling { get; private set; }

    public bool IsInRangeToShowHealth { get; private set; }

    public void Initialize(EnemyHealth health, Camera cam)
    {
        // Set core references
        enemyHealth = health;
        mainCamera = cam;
        rectTransform = GetComponent<RectTransform>();

        // Get the scaling component from the SAME enemy
        enemyLevelScaling = enemyHealth.GetComponent<EnemyLevelScaling>();

        // Set Y-offset based on enemy type
        currentYOffset = enemyHealth.enemyType switch
        {
            EnemyHealth.EnemyType.Small => smallEnemyYOffset,
            EnemyHealth.EnemyType.Medium => mediumEnemyYOffset,
            _ => smallEnemyYOffset
        };

        // Initialize health values
        RefreshHealthValues();

        // Subscribe to events
        enemyHealth.OnHealthChanged += UpdateHealthBar;
        enemyHealth.OnDeath += HandleEnemyDeath;

        if (enemyLevelScaling != null)
        {
            enemyLevelScaling.OnLevelUp += UpdateHealthBarAfterLevelUp;
        }

        // Initial visibility state
        SetAlpha(0f);
        IsInRangeToShowHealth = false;
    }

    private void RefreshHealthValues()
    {
        healthSlider.maxValue = enemyHealth.MaxHealth;
        healthSlider.value = enemyHealth.CurrentHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor,
                                   (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth);
    }

    private void UpdateHealthBarAfterLevelUp()
    {
        RefreshHealthValues();
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor,
                                   (float)currentHealth / enemyHealth.MaxHealth);
    }

    private void HandleEnemyDeath()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (enemyHealth == null || mainCamera == null) return;

        UpdatePosition();
        UpdateVisibility();
    }

    private void UpdatePosition()
    {
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(enemyHealth.transform.position);
        rectTransform.position = new Vector3(screenPosition.x, screenPosition.y + currentYOffset, screenPosition.z);
    }

    private void UpdateVisibility()
    {
        if (playerTransform == null)
        {
            TryFindPlayer();
            return;
        }

        float distance = Vector3.Distance(playerTransform.position, enemyHealth.transform.position);
        bool shouldBeVisible = distance <= visibilityDistance;

        if (shouldBeVisible != IsInRangeToShowHealth)
        {
            IsInRangeToShowHealth = shouldBeVisible;
            needsFade = true;
        }

        if (needsFade)
        {
            float targetAlpha = IsInRangeToShowHealth ? 1f : 0f;
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(currentAlpha);

            if (Mathf.Approximately(currentAlpha, targetAlpha))
            {
                needsFade = false;
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        // Apply alpha to all relevant images
        Color fillColor = fillImage.color;
        fillColor.a = alpha;
        fillImage.color = fillColor;

        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = alpha;
            backgroundImage.color = bgColor;
        }

        // If you have other UI elements to fade, add them here
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else if (currentAlpha > 0)
        {
            SetAlpha(0f);
            IsInRangeToShowHealth = false;
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
            enemyHealth.OnDeath -= HandleEnemyDeath;
        }

        if (enemyLevelScaling != null)
        {
            enemyLevelScaling.OnLevelUp -= UpdateHealthBarAfterLevelUp;
        }
    }
}