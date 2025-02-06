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
    public float saveDistanceThreshold = 1f; // מרחק מינימלי לשמירה

    private int lastSaveSpawnIndex = -1; // אינדקס נקודת ההשרצה האחרונה שבה נשמר המשחק

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // שמירה אוטומטית ובדיקה לפי מרחק
        int currentSpawnIndex = FindNearestSpawnPointIndex();
        if (currentSpawnIndex != lastSaveSpawnIndex &&
            Vector3.Distance(player.transform.position, spawnPoints[currentSpawnIndex].position) <= saveDistanceThreshold)
        {
            SaveGame();
            lastSaveSpawnIndex = currentSpawnIndex;
        }
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
            lastSaveSpawnIndex = data.playerSpawnIndex; // עדכון אינדקס השמירה האחרונה

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