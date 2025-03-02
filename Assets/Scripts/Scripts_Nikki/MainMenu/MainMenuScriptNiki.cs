using System.Collections;
using UnityEngine;

public class MainMenuScriptNiki : MonoBehaviour
{
    public static MainMenuScriptNiki instance;
    public ChangeColor_MainMenu[] mainMenuSlots;
    public float fadeDuration = 0.5f; // משך זמן הדהייה

    private int selectedSlot = -1;
    private bool isTransitioning = false; // בדיקה אם מתבצע מעבר

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ChangeSelectedSlot(0);
        Debug.Log("mainMenuSlots Length: " + mainMenuSlots.Length);
    }

    private void Update()
    {
        if (isTransitioning) return; // מניעת קלט בזמן מעבר

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(TransitionSlots(selectedSlot - 1));
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(TransitionSlots(selectedSlot + 1));
        }
    }

    IEnumerator TransitionSlots(int newSlotIndex)
    {
        isTransitioning = true; // סימון תחילת מעבר

        if (selectedSlot >= 0 && selectedSlot < mainMenuSlots.Length)
        {
            yield return StartCoroutine(FadeSlot(mainMenuSlots[selectedSlot], false)); // דהייה לסלוט הנוכחי
            mainMenuSlots[selectedSlot].Deselect();
        }

        newSlotIndex = Mathf.Clamp(newSlotIndex, 0, mainMenuSlots.Length - 1);

        if (newSlotIndex >= 0 && newSlotIndex < mainMenuSlots.Length)
        {
            mainMenuSlots[newSlotIndex].Select();
            yield return StartCoroutine(FadeSlot(mainMenuSlots[newSlotIndex], true)); // דהייה לסלוט החדש
            selectedSlot = newSlotIndex;
        }

        isTransitioning = false; // סימון סיום מעבר
    }

    IEnumerator FadeSlot(ChangeColor_MainMenu slot, bool fadeIn)
    {
        float startTime = Time.time;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        while (Time.time < startTime + fadeDuration)
        {
            float timePassed = Time.time - startTime;
            float t = timePassed / fadeDuration; // ערך בין 0 ל-1
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            slot.SetAlpha(alpha); // שינוי שקיפות
            yield return null;
        }

        slot.SetAlpha(endAlpha); // הגדרה סופית
    }


void ChangeSelectedSlot(int newValue) // שינוי הסלוט הנבחר
    {
        if (selectedSlot >= 0 && selectedSlot < mainMenuSlots.Length) // בדיקה בטווח
        {
            mainMenuSlots[selectedSlot].Deselect(); // ביטול בחירת הסלוט הקודם
        }

        // הגבלת newValue לטווח המערך
        newValue = Mathf.Clamp(newValue, 0, mainMenuSlots.Length - 1);

        if (newValue >= 0 && newValue < mainMenuSlots.Length) // בדיקה בטווח
        {
            mainMenuSlots[newValue].Select(); // בחירת הסלוט החדש
            selectedSlot = newValue; // עדכון אינדקס הסלוט הנבחר
        }
    }
}