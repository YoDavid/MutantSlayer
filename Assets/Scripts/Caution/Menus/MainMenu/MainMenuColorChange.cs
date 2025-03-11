using UnityEngine;
using UnityEngine.UI;

public class MainMenuColorChange : MonoBehaviour
{
    public Image image; // הפניה לרכיב התמונה
    public Color selectedColor = Color.yellow;
    public Color notSelectedColor = Color.white;

    private Color originalColor;

    void Start()
    {
        originalColor = image.color;
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public void SetAlpha(float alpha)
    {
        Color newColor = image.color;
        newColor.a = alpha;
        image.color = newColor;
    }
}