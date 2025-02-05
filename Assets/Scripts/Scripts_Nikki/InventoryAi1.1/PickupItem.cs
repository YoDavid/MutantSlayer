using UnityEngine;
using System;


public class PickupItem : MonoBehaviour
{
    public ItemNikiAi item; // הפניה ל-ItemNikiAi (Scriptable Object)

    /*private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // ודאו שלשחקן יש תגית "Player"
        {
            if (InventoryManagerAi.instance.AddItemAi(item)) // הוספת הפריט למלאי
            {
                Destroy(gameObject); // השמדת אובייקט הפריט בעולם המשחק
                Debug.Log(item.itemName + " picked up!"); // הודעה לקונסול
            }
            else
            {
                Debug.Log("Inventory is full!"); // הודעה לקונסול
            }
        }
    }*/
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (InventoryManagerAi.instance == null)
        {
            Debug.LogError("InventoryManagerAi.instance is null!");
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (InventoryManagerAi.instance.AddItemAi(item)) // הוספת הפריט למלאי
            {
                Destroy(gameObject); // השמדת אובייקט הפריט בעולם המשחק
                Debug.Log(item.itemName + " picked up!"); // הודעה לקונסול
            }
            else
            {
                Debug.Log("Inventory is full!"); // הודעה לקונסול
            }
        }
    }
}
