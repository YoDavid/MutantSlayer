using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public float maxStamina = 100f;
    public float refillRate = 20f;
    public float depletionRate = 100f;

    [Header("Blink Settings")]
    public float fullStaminaBlinkSpeed = 0.5f; // Slower blink when full
    public float lowStaminaBlinkSpeed = 0.2f;  // Faster blink when low

    [Header("Stamina Colors")]
    public Color fillColor = Color.yellow;     // Default color
    public Color blinkColor = Color.white;     // Blinks to white when full
    public Color lowStaminaColor = Color.red;  // Blinks to red when below 25%

    private bool isBlinking = false;
    private Coroutine blinkRoutine;
    private Image fillImage; // Cache the fill image

    public bool IsFull => staminaSlider.value >= maxStamina;
    public bool IsEmpty => staminaSlider.value <= 0;
    public bool IsLowStamina => staminaSlider.value <= maxStamina * 0.25f; // Below 25%

    private void Start()
    {
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;

        // Cache the fill image
        if (staminaSlider.fillRect != null)
        {
            fillImage = staminaSlider.fillRect.GetComponent<Image>();
            fillImage.color = fillColor;
        }
    }

    private void Update()
    {
        if (!IsFull)
        {
            staminaSlider.value += refillRate * Time.deltaTime;

            // Stop blinking if not full and not low
            if (isBlinking && !IsLowStamina)
            {
                StopBlinking();
            }
        }

        // Blink if full or low stamina
        if (!isBlinking)
        {
            if (IsFull)
            {
                blinkRoutine = StartCoroutine(BlinkBar(blinkColor, fullStaminaBlinkSpeed));
            }
            else if (IsLowStamina)
            {
                blinkRoutine = StartCoroutine(BlinkBar(lowStaminaColor, lowStaminaBlinkSpeed));
            }
        }
    }

    private IEnumerator BlinkBar(Color targetColor, float blinkSpeed)
    {
        isBlinking = true;

        while ((IsFull || IsLowStamina) && fillImage != null)
        {
            fillImage.color = targetColor; // Blink to target color
            yield return new WaitForSeconds(blinkSpeed);
            fillImage.color = fillColor;  // Return to default
            yield return new WaitForSeconds(blinkSpeed);
        }

        // Reset to default color when done
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }
        isBlinking = false;
    }

    private void StopBlinking()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }
        isBlinking = false;
    }

    public void DepleteStamina(float amount)
    {
        staminaSlider.value = Mathf.Max(0f, staminaSlider.value - amount);
    }

    public void SetStamina(float value)
    {
        staminaSlider.value = Mathf.Clamp(value, 0, maxStamina);
    }

    public float GetStamina() => staminaSlider.value;
}