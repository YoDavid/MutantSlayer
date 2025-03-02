using UnityEngine;
using System.Collections;

public class PlayerDamageBlink : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Blink Effect Settings")]
    [SerializeField] private float blinkDuration = 0.1f; // Duration of each blink
    [SerializeField] private int blinkCount = 3; // Number of blinks after taking damage

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on the player.");
        }
    }

    public void TriggerBlinkEffect()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(BlinkCoroutine());
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            // Make the player invisible (transparent)
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            yield return new WaitForSeconds(blinkDuration);

            // Restore original color (visible)
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}
