using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScriptNiki : MonoBehaviour
{
    public static MainMenuScriptNiki instance; // יצירת Instance סטטי - גישה נוחה מכל מקום

    public ChangeColor_MainMenu[] mainMenuSlots; // מערך של סלוטים במלאי

    private int selectedSlot = -1; // אינדקס של הסלוט הנבחר

    private void Awake() // אתחול לפני Start
    {
        instance = this; // הגדרת ה-instance
    }

    private void Start() // אתחול
    {
        ChangeSelectedSlot(0); // בחירת סלוט ראשון
        Debug.Log("mainMenuSlots Length: " + mainMenuSlots.Length); // הדפסת אורך המערך
    }

    private void Update() // עדכון
    {
        // מעבר בין סלוטים באמצעות מקשי W ו-S
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeSelectedSlot(selectedSlot - 1); // בחירת סלוט קודם
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeSelectedSlot(selectedSlot + 1); // בחירת סלוט הבא
        }
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