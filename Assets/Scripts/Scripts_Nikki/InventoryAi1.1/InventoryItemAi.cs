using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemAi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")] // כותרת ב-Inspector
    public Image image; // רכיב התמונה של הפריט
    public Text countText; // רכיב הטקסט של כמות הפריט

    [HideInInspector] public ItemNikiAi itemNiki; // הפניה ל-Scriptable Object של הפריט
    [HideInInspector] public int count = 1; // כמות הפריט
    [HideInInspector] public Transform parentAfterDrag; // ההורה המקורי של הפריט

    private CanvasGroup canvasGroup; // רכיב CanvasGroup לשליטה על שקיפות

    private void Awake() // אתחול לפני Start
    {
        canvasGroup = GetComponent<CanvasGroup>(); // אתחול רכיב ה-CanvasGroup
    }

    public void InitializeItem(ItemNikiAi newItem) // אתחול הפריט
    {
        Debug.Log("newItem: " + newItem);
        itemNiki = newItem; // שמירת ה-Scriptable Object
        image.sprite = newItem.image; // הגדרת תמונת הפריט
        RefreshCount(); // עדכון תצוגת הכמות
    }

    public void RefreshCount() // עדכון תצוגת הכמות
    {
        countText.text = count.ToString(); // עדכון הטקסט
        countText.gameObject.SetActive(count > 1); // הפעלה/כיבוי של הטקסט (אם כמות > 1)
    }

    public void OnBeginDrag(PointerEventData eventData) // תחילת גרירה
    {
        parentAfterDrag = transform.parent; // שמירת ההורה המקורי
        transform.SetParent(transform.root); // שינוי הורה לשורש הקנבס (חשוב!)
        transform.SetAsLastSibling(); // הבאת הפריט לחזית
        image.raycastTarget = false; // כיבוי Raycast Target (מונע חסימה של אלמנטים מתחת)
        canvasGroup.alpha = 0.6f; // שינוי שקיפות
    }

    public void OnDrag(PointerEventData eventData) // גרירה
    {
        transform.position = Input.mousePosition; // עדכון מיקום
    }

    public void OnEndDrag(PointerEventData eventData) // סיום גרירה
    {
        transform.SetParent(parentAfterDrag); // החזרת ההורה המקורי
        image.raycastTarget = true; // הפעלת Raycast Target
        canvasGroup.alpha = 1f; // החזרת שקיפות רגילה
    }
}
