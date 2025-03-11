using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenningSlideShow : MonoBehaviour
{
    // מערך של תמונות (Sprites)
    public Sprite[] images;
    // מערך של טקסטים
    public string[] texts;
    // רכיב התמונה (Image)
    public Image imageComponent;
    // רכיב הטקסט (Text)
    public Text textComponent;
    // שם הסצנה למעבר בסיום
    public string nextSceneName;
    // משך זמן הדהייה (בשניות)
    public float fadeDuration = 0.5f;

    private int currentIndex = 0; // אינדקס התמונה/טקסט הנוכחי
    private bool isFading = false; // האם מתבצעת דהייה

    void Start()
    {
        // הצגת התמונה והטקסט הראשונים
        if (images.Length > 0 && texts.Length > 0)
        {
            imageComponent.sprite = images[0];
            textComponent.text = texts[0];
        }
    }

    void Update()
    {
        // בדיקה אם נלחץ כפתור רווח ואין דהייה
        if (Input.GetKeyDown(KeyCode.Space) && !isFading)
        {
            // מעבר לתמונה/טקסט הבא
            currentIndex++;

            // בדיקה אם הגענו לתמונה/טקסט האחרון
            if (currentIndex >= images.Length || currentIndex >= texts.Length)
            {
                // מעבר לסצנה הבאה
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // התחלת מעבר עם דהייה
                StartCoroutine(FadeAndChange());
            }
        }
    }

    // קורוטינה לביצוע מעבר עם דהייה
    IEnumerator FadeAndChange()
    {
        isFading = true; // סימון תחילת דהייה

        // דהייה החוצה
        yield return StartCoroutine(Fade(1f, 0f));

        // שינוי תמונה וטקסט
        imageComponent.sprite = images[currentIndex];
        textComponent.text = texts[currentIndex];

        // דהייה פנימה
        yield return StartCoroutine(Fade(0f, 1f));

        isFading = false; // סימון סיום דהייה
    }

    // קורוטינה לביצוע דהייה
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0;
        Color imageColor = imageComponent.color;
        Color textColor = textComponent.color;

        while (time < fadeDuration)
        {
            // חישוב שקיפות נוכחית
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);

            // עדכון שקיפות תמונה וטקסט
            imageColor.a = alpha;
            textColor.a = alpha;
            imageComponent.color = imageColor;
            textComponent.color = textColor;

            time += Time.deltaTime;
            yield return null;
        }

        // הגדרה סופית של שקיפות
        imageColor.a = endAlpha;
        textColor.a = endAlpha;
        imageComponent.color = imageColor;
        textComponent.color = textColor;
    }
}