using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotAi : MonoBehaviour, IDropHandler
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

    public void OnDrop(PointerEventData eventData) // אירוע Drop
    {
        if (transform.childCount == 0) // בדיקה אם הסלוט ריק
        {
            GameObject dropped = eventData.pointerDrag; // האובייקט שנגרר
            InventoryItemAi inventoryItem = dropped.GetComponent<InventoryItemAi>(); // רכיב InventoryItem של האובייקט

            if (inventoryItem != null) // בדיקה אם קיים רכיב InventoryItem
            {
                inventoryItem.parentAfterDrag = transform; // הגדרת הורה חדש לאובייקט
            }
        }
    }
}