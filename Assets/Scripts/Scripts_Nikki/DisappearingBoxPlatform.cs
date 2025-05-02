using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Renderer))] // דורש גם רכיב Renderer כדי לשלוט בנראות
public class DisappearingBoxPlatform : MonoBehaviour
{
    [Header("הגדרות זמן")]
    [Tooltip("זמן שהשחקן חייב לעמוד על הפלטפורמה לפני שהיא נעלמת")]
    public float activationDelay = 2f;

    [Tooltip("זמן שהפלטפורמה תישאר נעלמת")]
    public float disappearDuration = 3f;

    private BoxCollider2D platformCollider;
    private Renderer platformRenderer;
    private float standingTimer = 0f;
    private bool isPlayerOn = false;
    private Coroutine disappearRoutine;

    void Start()
    {
        platformCollider = GetComponent<BoxCollider2D>();
        platformRenderer = GetComponent<Renderer>();

        if (!platformCollider || !platformRenderer)
        {
            Debug.LogError("האובייקט חייב להכיל רכיבי BoxCollider2D ו-Renderer!");
            enabled = false;
        }
    }

    void Update()
    {
        if (isPlayerOn)
        {
            standingTimer += Time.deltaTime;

            if (standingTimer >= activationDelay && disappearRoutine == null)
            {
                disappearRoutine = StartCoroutine(DisappearAndReappear());
                standingTimer = 0f;
            }
        }
        else
        {
            standingTimer = 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = true;
            standingTimer = 0f;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
            standingTimer = 0f;

            // עצור את הקורוטינה אם השחקן עוזב לפני שהיא הסתיימה
            if (disappearRoutine != null)
            {
                StopCoroutine(disappearRoutine);
                disappearRoutine = null;
                EnablePlatform(); // הפעל מחדש את הפלטפורמה
            }
        }
    }

    IEnumerator DisappearAndReappear()
    {
        // כיבוי הפלטפורמה
        DisablePlatform();

        // המתנה למשך זמן ההיעלמות
        yield return new WaitForSeconds(disappearDuration);

        // הפעלה מחדש של הפלטפורמה
        EnablePlatform();
        disappearRoutine = null; // אפשר להפעיל את הקורוטינה שוב
    }

    void DisablePlatform()
    {
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }
        if (platformRenderer != null)
        {
            platformRenderer.enabled = false;
        }
    }

    void EnablePlatform()
    {
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }
        if (platformRenderer != null)
        {
            platformRenderer.enabled = true;
        }
    }
}