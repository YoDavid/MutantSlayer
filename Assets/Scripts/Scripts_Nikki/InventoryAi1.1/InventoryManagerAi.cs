using System;
using UnityEngine;

public class InventoryManagerAi : MonoBehaviour
{
    public static InventoryManagerAi instance; // יצירת Instance סטטי - גישה נוחה מכל מקום

    public int maxStackedItems = 4; // כמות מקסימלית של פריטים בערימה
    public InventorySlotAi[] inventorySlots; // מערך של סלוטים במלאי
    public GameObject inventoryItemPrefab; // Prefab של פריט במלאי

    private int selectedSlot = -1; // אינדקס של הסלוט הנבחר

    private void Awake() // אתחול לפני Start
    {
        instance = this; // הגדרת ה-instance
    }

    private void Start() // אתחול
    {
        ChangeSelectedSlot(0); // בחירת סלוט ראשון
        Debug.Log("inventorySlots Length: " + inventorySlots.Length); // הדפסת אורך המערך

        // בדיקה נוספת לוודא שכל סלוט מאותחל
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                Debug.LogError("Inventory slot " + i + " is null!");
            }
            else
            {
                Debug.Log("inventorySlots[" + i + "]: " + inventorySlots[i]); // הדפסת כל אלמנט
            }
        }
    }

    private void Update() // עדכון
    {
        // קלט מספרים - טיפול במספרים מרובים
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c)) // בדיקה אם מדובר בספרה
            {
                int number = int.Parse(c.ToString()); // המרת התו למספר
                if (number > 0 && number <= inventorySlots.Length) // בדיקה בטווח המערך
                {
                    ChangeSelectedSlot(number - 1); // בחירת סלוט
                }
            }
        }

        // בדיקה לשימוש בפריט עם כפתור E
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }
    }

    void ChangeSelectedSlot(int newValue) // שינוי הסלוט הנבחר
    {
        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length) // בדיקה בטווח
        {
            inventorySlots[selectedSlot].Deselect(); // ביטול בחירת הסלוט הקודם
        }

        if (newValue >= 0 && newValue < inventorySlots.Length) // בדיקה בטווח
        {
            inventorySlots[newValue].Select(); // בחירת הסלוט החדש
            selectedSlot = newValue; // עדכון אינדקס הסלוט הנבחר
        }
    }

    public bool AddItemAi(ItemNikiAi itemNikiAi)
    {
        if (inventorySlots == null)
        {
            Debug.LogError("inventorySlots is not initialized!");
            return false;
        }

        foreach (InventorySlotAi slot in inventorySlots)
        {
            if (slot == null) // הבדיקה החשובה!
            {
                Debug.LogWarning("A slot in inventorySlots is null!");
                continue; // המשך לולאה לאיבר הבא
            }

            InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>(); // רכיב InventoryItem בסלוט
            if (itemInSlot != null &&
                itemInSlot.itemNiki == itemNikiAi &&
                itemInSlot.count < maxStackedItems &&
                itemNikiAi.stackableAi)
            {
                itemInSlot.count++; // הגדלת כמות
                itemInSlot.RefreshCount(); // עדכון תצוגה
                return true; // הפריט נוסף
            }
        }

        // חיפוש סלוט ריק
        foreach (InventorySlotAi slot in inventorySlots)
        {
            InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>();
            if (itemInSlot == null)
            {
                GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform); // יצירת פריט חדש
                InventoryItemAi inventoryItem = newItemGo.GetComponent<InventoryItemAi>(); // רכיב InventoryItem של הפריט החדש
                inventoryItem.InitializeItem(itemNikiAi); // אתחול הפריט החדש
                return true; // הפריט נוסף
            }
        }
        return false; // לא נמצא מקום
    }

    public ItemNikiAi GetSelectedItem(bool use) // קבלת פריט נבחר
    {
        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length) // בדיקה בטווח
        {
            InventorySlotAi slot = inventorySlots[selectedSlot]; // הסלוט הנבחר
            if (slot != null)
            {
                InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>(); // רכיב InventoryItem בסלוט

                if (itemInSlot != null) // יש פריט בסלוט
                {
                    ItemNikiAi item = itemInSlot.itemNiki; // הפריט
                    if (use) // אם רוצים להשתמש בפריט
                    {
                        itemInSlot.count--; // הקטנת כמות
                        if (itemInSlot.count <= 0) // אם הכמות הגיעה ל-0
                        {
                            Destroy(itemInSlot.gameObject); // השמדת הפריט
                        }
                        else
                        {
                            itemInSlot.RefreshCount(); // עדכון תצוגה
                        }
                        return item; // החזרת הפריט
                    }
                    return item; // החזרת הפריט (ללא שימוש)
                }
            }
        }
        return null; // אין פריט נבחר
    }

    public void UseSelectedItem() // שימוש בפריט נבחר
    {
        ItemNikiAi selectedItem = GetSelectedItem(true); // קבלת פריט ושימוש בו
        if (selectedItem != null) // יש פריט נבחר
        {
            // כאן תוסיפו את הלוגיקה לשימוש בפריט
            Debug.Log("Using item: " + selectedItem.itemName); // הדפסת שם הפריט
        }
    }

    // פונקציה להסרת פריט מהאינבנטורי
    public void RemoveItemAi(ItemNikiAi itemNikiAi)
    {
        if (inventorySlots == null)
        {
            Debug.LogError("inventorySlots is not initialized!");
            return;
        }

        foreach (InventorySlotAi slot in inventorySlots)
        {
            if (slot == null)
            {
                Debug.LogWarning("A slot in inventorySlots is null!");
                continue;
            }

            InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>();
            if (itemInSlot != null && itemInSlot.itemNiki == itemNikiAi)
            {
                Destroy(itemInSlot.gameObject);
                return;
            }
        }
    }
}