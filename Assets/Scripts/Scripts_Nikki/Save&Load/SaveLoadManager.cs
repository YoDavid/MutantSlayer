using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager instance;

    public string saveFileName = "save.json";
    public Transform[] spawnPoints;
    public InventoryManagerAi inventoryManager;
    public GameObject player;

    private int lastSaveSpawnIndex = -1; // אינדקס נקודת ההשרצה האחרונה שבה נשמר המשחק

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // שמירה אוטומטית רק כאשר השחקן מגיע *בדיוק* לנקודת שמירה
        int currentSpawnIndex = FindNearestSpawnPointIndex();

        // בדיקה קפדנית של מיקום השחקן ונקודת ההשרצה
        if (currentSpawnIndex != lastSaveSpawnIndex && IsPlayerAtSpawnPoint(currentSpawnIndex))
        {
            SaveGame();
            lastSaveSpawnIndex = currentSpawnIndex;
        }
    }

    // פונקציה לבדיקה אם השחקן נמצא *בדיוק* בנקודת השרצה
    private bool IsPlayerAtSpawnPoint(int spawnIndex)
    {
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length) return false; // בדיקה לגבולות המערך

        Vector3 spawnPointPosition = spawnPoints[spawnIndex].position;
        Vector3 playerPosition = player.transform.position;

        // השוואה עם טווח דיוק קטן מאוד (אפסילון)
        return Vector3.SqrMagnitude(playerPosition - spawnPointPosition) < 0.0001f; // השוואת ריבוע המרחק
    }


    public void SaveGame()
    {
        SaveData data = new SaveData();

        int playerSpawnIndex = FindNearestSpawnPointIndex();
        data.playerSpawnIndex = playerSpawnIndex;

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

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);

        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/" + saveFileName;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            player.transform.position = spawnPoints[data.playerSpawnIndex].position;
            lastSaveSpawnIndex = data.playerSpawnIndex;

            foreach (InventorySlotAi slot in inventoryManager.inventorySlots)
            {
                foreach (Transform child in slot.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (InventoryItemData itemData in data.inventory)
            {
                ItemNikiAi itemNikiAi = Resources.Load<ItemNikiAi>("Items/" + itemData.itemName);

                if (itemNikiAi != null)
                {
                    inventoryManager.AddItemAi(itemNikiAi);
                    foreach (InventorySlotAi slot in inventoryManager.inventorySlots)
                    {
                        InventoryItemAi itemInSlot = slot.GetComponentInChildren<InventoryItemAi>();
                        if (itemInSlot != null && itemInSlot.itemNiki != null && itemInSlot.itemNiki.itemName == itemData.itemName)
                        {
                            itemInSlot.count = itemData.count;
                            itemInSlot.RefreshCount();
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to load item: " + itemData.itemName + ". Check the Resources folder and the item name.");
                }
            }

            Debug.Log("Game Loaded!");
        }
        else
        {
            Debug.Log("No save file found.");
        }
    }

    private int FindNearestSpawnPointIndex()
    {
        float minDistance = Vector3.Distance(player.transform.position, spawnPoints[0].position);
        int nearestIndex = 0;

        for (int i = 1; i < spawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(player.transform.position, spawnPoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}

// מחלקה לייצוג נתוני השמירה
[System.Serializable]
public class SaveData
{
    public int playerSpawnIndex;
    public List<InventoryItemData> inventory;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public int count;
}