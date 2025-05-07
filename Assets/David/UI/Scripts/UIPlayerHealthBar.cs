using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image fillImage;

    [Header("Health Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float colorChangeThreshold = 0.3f;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.48f;
    [SerializeField] private float blinkThreshold = 0.25f;

    private bool isBlinking = false;
    private Coroutine blinkRoutine;
    private bool wasDisabled = false;

    [SerializeField] private PlayerLevelSystem playerLevelSystem;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.MaxHealth;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            playerLevelSystem.OnLevelUp += UpdateHealthBarMaxHealth;
        }

        // Force check blinking state when UI is re-enabled
        if (wasDisabled)
        {
            wasDisabled = false;
            CheckBlinkingStateImmediately();
        }
    }

    private void OnDisable()
    {
        wasDisabled = true;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerLevelSystem.OnLevelUp -= UpdateHealthBarMaxHealth;
        }

        StopAllCoroutines();
        isBlinking = false;
    }

    private void Update()
    {
        if (playerHealth == null) return;

        UpdateHealthSlider();
        CheckBlinkingState();
    }

    private void UpdateHealthSlider()
    {
        healthSlider.value = Mathf.Lerp(healthSlider.value,
                                      playerHealth.CurrentHealth,
                                      10f * Time.deltaTime);
    }

    private void CheckBlinkingState()
    {
        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;

        if (healthPercent <= blinkThreshold)
        {
            if (!isBlinking)
            {
                StartBlinking();
            }
        }
        else if (isBlinking)
        {
            StopBlinking();
        }
        else
        {
            UpdateHealthColor(healthPercent);
        }
    }

    private void CheckBlinkingStateImmediately()
    {
        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;

        if (healthPercent <= blinkThreshold)
        {
            StartBlinking();
        }
        else
        {
            UpdateHealthColor(healthPercent);
        }
    }

    private void StartBlinking()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkHealthBar());
        isBlinking = true;
    }

    private IEnumerator BlinkHealthBar()
    {
        Color originalColor = fillImage.color;

        while ((float)playerHealth.CurrentHealth / playerHealth.MaxHealth <= blinkThreshold)
        {
            fillImage.color = criticalColor;
            yield return new WaitForSeconds(blinkInterval);

            fillImage.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);

            // Update original color in case it changed during blinking
            originalColor = Color.Lerp(criticalColor,
                                      healthyColor,
                                      ((float)playerHealth.CurrentHealth / playerHealth.MaxHealth) / colorChangeThreshold);
        }

        StopBlinking();
    }

    private void StopBlinking()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        float healthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        UpdateHealthColor(healthPercent);
        isBlinking = false;
    }

    private void UpdateHealthColor(float healthPercent)
    {
        fillImage.color = Color.Lerp(criticalColor,
                                   healthyColor,
                                   healthPercent / colorChangeThreshold);
    }

    public void OnPlayerHealthChanged(int newHealth)
    {
        healthSlider.value = newHealth;
        CheckBlinkingStateImmediately();
    }

    private void UpdateHealthBar(int currentHealth)
    {
        healthSlider.value = currentHealth;
        healthSlider.maxValue = playerHealth.MaxHealth; // Always update max in case of level up
        CheckBlinkingStateImmediately();
    }

    private void UpdateHealthBarMaxHealth()
    {
        healthSlider.maxValue = playerHealth.MaxHealth;
        CheckBlinkingStateImmediately();
    }
}