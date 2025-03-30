using System.Collections;
using UnityEngine;

public class PlatformDisappearAndReappear : MonoBehaviour
{
    // משך הזמן עד שהפלטפורמה נעלמת
    public float disappearTime = 3f;
    // משך זמן ההבהוב
    public float blinkDuration = 1f;
    // מספר ההבהובים
    public int blinkCount = 5;
    // משך הזמן עד שהפלטפורמה חוזרת
    public float reappearTime = 5f;

    private bool playerOnPlatform = false;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Vector3 originalPosition;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (playerOnPlatform)
        {
            timer += Time.deltaTime;

            if (timer >= disappearTime)
            {
                StartCoroutine(BlinkAndDisappear());
                playerOnPlatform = false; // עצירת הטיימר
            }
        }
        else
        {
            timer = 0f; // איפוס הטיימר
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
        }
    }

    IEnumerator BlinkAndDisappear()
    {
        // הבהוב
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
        }

        // כיבוי הפלטפורמה
        spriteRenderer.enabled = false;
        platformCollider.enabled = false;

        // המתנה ואיפוס הפלטפורמה
        yield return new WaitForSeconds(reappearTime);
        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        transform.position = originalPosition;
        spriteRenderer.color = originalColor;
    }
}