using System.Collections.Generic;
using System.IO;
using UnityEngine;

// סקריפט לשמירה וטעינה של מיקום שחקן ואינבנטורי
public class SaveLoadManager : MonoBehaviour
{
    // אינסטנס סטטי לגישה מכל מקום
    public static SaveLoadManager instance;

    // שם קובץ השמירה
    public string saveFileName = "save.json";

    // מערך של נקודות השרצה (נקודות שמירה)
    public Transform[] spawnPoints;

    // הפניה למנהל האינבנטורי
    public InventoryManagerAi inventoryManager;

    // הפניה לאובייקט השחקן
    public GameObject player;

    // אינדקס נקודת ההשרצה האחרונה שבה נשמר המשחק
    private int lastSaveSpawnIndex = -1;

    // אתחול לפני Start
    private void Awake()
    {
        // הגדרת האינסטנס הסטטי
        instance = this;
    }

    // עדכון פריימים
    private void Update()
    {
        // מציאת אינדקס נקודת ההשרצה הקרובה ביותר
        int currentSpawnIndex = FindNearestSpawnPointIndex();

        // בדיקה אם השחקן נמצא בדיוק בנקודת שמירה חדשה
        if (currentSpawnIndex != lastSaveSpawnIndex && IsPlayerAtSpawnPoint(currentSpawnIndex))
        {
            // שמירת המשחק
            SaveGame();

            // עדכון אינדקס נקודת השמירה האחרונה
            lastSaveSpawnIndex = currentSpawnIndex;
        }
    }

    // פונקציה לבדיקה אם השחקן נמצא בדיוק בנקודת השרצה
    private bool IsPlayerAtSpawnPoint(int spawnIndex)
    {
        // בדיקה לגבולות המערך
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length) return false;

        // מיקום נקודת ההשרצה
        Vector3 spawnPointPosition = spawnPoints[spawnIndex].position;

        // מיקום השחקן
        Vector3 playerPosition = player.transform.position;

        // השוואה עם טווח דיוק קטן מאוד (אפסילון)
        return Vector3.SqrMagnitude(playerPosition - spawnPointPosition) < 0.0001f;
    }

    // פונקציה לשמירת המשחק
    public void SaveGame()
    {
        // יצירת אובייקט נתונים לשמירה
        SaveData data = new SaveData();

        // מציאת אינדקס נקודת ההשרצה הקרובה ביותר
        int playerSpawnIndex = FindNearestSpawnPointIndex();

        // שמירת אינדקס נקודת ההשרצה
        data.playerSpawnIndex = playerSpawnIndex;

        // שמירת אינבנטורי
        data.inventory = new List<InventoryItemData>();
        foreach (InventorySlotAi slot in inventoryManager.inventorySlots)
        {
            InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>();
            if (itemInSlot != null)
            {
                InventoryItemData itemData = new InventoryItemData();
                itemData.itemName = itemInSlot.itemNiki.itemName;
                itemData.count = itemInSlot.count;
                data.inventory.Add(itemData);
            }
        }

        // המרת הנתונים ל-JSON ושמירה לקובץ
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);

        // הדפסת הודעה לקונסול
        Debug.Log("Game Saved!");
    }

    // פונקציה לטעינת המשחק
    public void LoadGame()
    {
        // נתיב קובץ השמירה
        string path = Application.persistentDataPath + "/" + saveFileName;

        // בדיקה אם קובץ השמירה קיים
        if (File.Exists(path))
        {
            // קריאת תוכן קובץ השמירה
            string json = File.ReadAllText(path);

            // המרת ה-JSON לאובייקט נתונים
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // טעינת מיקום השחקן
            player.transform.position = spawnPoints[data.playerSpawnIndex].position;

            // עדכון אינדקס נקודת השמירה האחרונה
            lastSaveSpawnIndex = data.playerSpawnIndex;

            // ניקוי האינבנטורי
            foreach (InventorySlotAi slot in inventoryManager.inventorySlots)
            {
                foreach (Transform child in slot.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // טעינת פריטים לאינבנטורי
            foreach (InventoryItemData itemData in data.inventory)
            {
                // טעינת פריט מ-Resources
                ItemNikiAi itemNikiAi = Resources.Load<ItemNikiAi>("Items/" + itemData.itemName);

                // בדיקה אם הפריט נטען בהצלחה
                if (itemNikiAi != null)
                {
                    // מציאת פריט קיים באינבנטורי
                    InventoryItemAi existingItem = FindItemInInventory(itemData.itemName);

                    // עדכון פריט קיים או הוספת פריט חדש
                    if (existingItem != null)
                    {
                        existingItem.count = itemData.count;
                        existingItem.RefreshCount();
                    }
                    else
                    {
                        inventoryManager.AddItemAi(itemNikiAi);
                        InventoryItemAi newItem = FindItemInInventory(itemData.itemName);
                        if (newItem != null)
                        {
                            newItem.count = itemData.count;
                            newItem.RefreshCount();
                        }
                    }
                }
                else
                {
                    // הדפסת הודעת שגיאה
                    Debug.LogError("Failed to load item: " + itemData.itemName + ". Check the Resources folder and the item name.");
                }
            }

            // הדפסת הודעה לקונסול
            Debug.Log("Game Loaded!");
        }
        else
        {
            // הדפסת הודעה לקונסול
            Debug.Log("No save file found.");
        }
    }

    // פונקציה למציאת פריט באינבנטורי
    private InventoryItemAi FindItemInInventory(string itemName)
    {
        foreach (InventorySlotAi slot in inventoryManager.inventorySlots)
        {
            InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>();
            if (itemInSlot != null && itemInSlot.itemNiki != null && itemInSlot.itemNiki.itemName == itemName)
            {
                return itemInSlot;
            }
        }
        return null;
    }

    // פונקציה למציאת אינדקס נקודת ההשרצה הקרובה ביותר
    private int FindNearestSpawnPointIndex()
    {
        // מרחק מינימלי התחלתי
        float minDistance = Vector3.Distance(player.transform.position, spawnPoints[0].position);

        // אינדקס נקודת ההשרצה הקרובה ביותר
        int nearestIndex = 0;

        // מעבר על כל נקודות ההשרצה
        for (int i = 1; i < spawnPoints.Length; i++)
        {
            // חישוב מרחק בין השחקן לנקודת ההשרצה
            float distance = Vector3.Distance(player.transform.position, spawnPoints[i].position);

            // בדיקה אם המרחק קטן מהמרחק המינימלי
            if (distance < minDistance)
            {
                // עדכון מרחק מינימלי ואינדקסminDistance = distance;
                nearestIndex = i;
            }
        }

        // החזרת אינדקס נקודת ההשרצה הקרובה ביותר
        return nearestIndex;
    }
}

// מחלקה לייצוג נתוני השמירה
[System.Serializable]
public class SaveData
{
    // אינדקס נקודת ההשרצה
    public int playerSpawnIndex;

    // רשימה של נתוני פריטים באינבנטורי
    public List<InventoryItemData> inventory;
}

// מחלקה לייצוג נתוני פריט באינבנטורי
[System.Serializable]
public class InventoryItemData
{
    // שם הפריט
    public string itemName;

    // כמות הפריט
    public int count;
}