using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemPickupNiki : MonoBehaviour
{
    public string itemName = "New Item"; // שם הפריט שיוצג במלאי
    public int itemQuantity = 1; // כמות הפריטים שיתווספו למלאי

    private Inventory inventory; // הפניה למערכת המלאי

    void Start()
    {
        // מציאת מערכת המלאי
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();

        // בדיקה אם מערכת המלאי קיימת
        if (inventory == null)
        {
            Debug.LogError("Inventory system not found!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // בדיקה אם השחקן אסף את הפריט
        if (other.CompareTag("Player"))
        {
            // הוספת הפריט למלאי
            inventory.AddItem(itemName, itemQuantity);

            // השמדת הפריט
            Destroy(gameObject);
        }
    }
}