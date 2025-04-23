using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public float maxStamina = 100f;
    public float refillRate = 20f;
    public float depletionRate = 100f;
    public float staminaBlinkSpeed = 0.5f;

    [Header("Stamina Colors")]
    public Color fillColor = Color.yellow; // Default yellow, editable in Inspector
    public Color blinkColor = Color.white; // Blinks to white when full

    private bool isBlinking = false;
    private Coroutine blinkRoutine;

    public bool IsFull => staminaSlider.value >= maxStamina;
    public bool IsEmpty => staminaSlider.value <= 0;


    private void Start()
    {
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;

        // Set initial fill color
        if (staminaSlider.fillRect != null)
        {
            staminaSlider.fillRect.GetComponent<Image>().color = fillColor;
        }
    }

    private void Update()
    {
        if (!IsFull)
        {
            staminaSlider.value += refillRate * Time.deltaTime;
            if (isBlinking)
            {
                StopCoroutine(blinkRoutine);
                ResetBlink();
            }
        }
        else if (!isBlinking)
        {
            blinkRoutine = StartCoroutine(BlinkBar());
        }
    }

    private IEnumerator BlinkBar()
    {
        isBlinking = true;
        Image fill = staminaSlider.fillRect.GetComponent<Image>();

        while (IsFull)
        {
            fill.color = blinkColor; // Blink to white
            yield return new WaitForSeconds(staminaBlinkSpeed);
            fill.color = fillColor; // Return to yellow
            yield return new WaitForSeconds(staminaBlinkSpeed);
        }

        fill.color = fillColor; // Reset to yellow when not full
        isBlinking = false;
    }

    private void ResetBlink()
    {
        if (staminaSlider.fillRect != null)
        {
            staminaSlider.fillRect.GetComponent<Image>().color = fillColor;
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