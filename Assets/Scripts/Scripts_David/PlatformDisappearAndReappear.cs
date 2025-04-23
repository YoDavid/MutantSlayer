using System.Collections;
using UnityEngine;

public class PlatformDisappearAndReappear : MonoBehaviour
{
    public float disappearTime = 3f;
    public float blinkDuration = 1f;
    public int blinkCount = 5;
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
                playerOnPlatform = false; 
            }
        }
        else
        {
            timer = 0f; 
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
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration / (blinkCount * 2));
        }

        spriteRenderer.enabled = false;
        platformCollider.enabled = false;

        yield return new WaitForSeconds(reappearTime);
        spriteRenderer.enabled = true;
        platformCollider.enabled = true;
        transform.position = originalPosition;
        spriteRenderer.color = originalColor;
    }
}