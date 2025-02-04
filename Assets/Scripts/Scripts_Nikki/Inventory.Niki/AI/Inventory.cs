using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventoryUI inventoryUI;
    // מבנה פנימי לייצוג פריט במלאי
    [System.Serializable]
    public struct Item
    {
        public string name;
        public int quantity;
    }

    public List<Item> items = new List<Item>(); // רשימת הפריטים במלאי

    // פונקציה להוספת פריט למלאי
    public void AddItem(string itemName, int itemQuantity)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == itemName)
            {
                // הפריט קיים, ניצור מבנה חדש עם הכמות המעודכנת
                Item updatedItem = new Item { name = itemName, quantity = items[i].quantity + itemQuantity };

                // נחליף את המבנה הישן ברשימה עם המבנה החדש
                items[i] = updatedItem;
                return;
            }
        }

        // הפריט לא קיים, נוסיף אותו לרשימה
        InventoryUI inventoryUI = GameObject.Find("Canvas").GetComponent<InventoryUI>();
        inventoryUI.UpdateInventoryUI();
    }

    // פונקציה להסרת פריט מהמלאי
    public void RemoveItem(string itemName, int itemQuantity)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == itemName)
            {
                // הפריט קיים, ניצור מבנה חדש עם הכמות המעודכנת
                Item updatedItem = new Item { name = itemName, quantity = items[i].quantity - itemQuantity };

                // נחליף את המבנה הישן ברשימה עם המבנה החדש
                items[i] = updatedItem;

                // אם הכמות הגיעה ל-0, נסיר את הפריט מהרשימה
                if (updatedItem.quantity <= 0)
                {
                    items.RemoveAt(i);
                }

                // עדכון ה-UI לאחר שינוי במלאי
                InventoryUI inventoryUI = GameObject.Find("Canvas").GetComponent<InventoryUI>();
                inventoryUI.UpdateInventoryUI();

                return;
            }
        }
    }

    // פונקציה לבדיקה אם פריט קיים במלאי
    public bool HasItem(string itemName)
    {
        foreach (var item in items)
        {
            if (item.name == itemName)
            {
                return true;
            }
        }

        return false;
    }

    // פונקציה לקבלת כמות של פריט במלאי
    public int GetItemQuantity(string itemName)
    {
        foreach (var item in items)
        {
            if (item.name == itemName)
            {
                return item.quantity;
            }
        }

        return 0;
    }
}