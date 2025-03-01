using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor_MainMenu : MonoBehaviour
{
    public Image image; // רכיב התמונה של הסלוט
    public Color selectedColor = Color.white; // צבע נבחר (ברירת מחדל - לבן)
    public Color notSelectedColor = Color.gray; // צבע לא נבחר (ברירת מחדל - אפור)

    private void Awake() // אתחול לפני Start
    {
        Deselect(); // הגדרת צבע ברירת מחדל
    }

    public void Select() // בחירת הסלוט
    {
        image.color = selectedColor; // שינוי צבע
    }

    public void Deselect() // ביטול בחירת הסלוט
    {
        image.color = notSelectedColor; // שינוי צבע
    }
}
