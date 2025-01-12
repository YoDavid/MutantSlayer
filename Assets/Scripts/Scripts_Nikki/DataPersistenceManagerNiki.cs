using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManagerNiki : MonoBehaviour
{
    private DataNiki DataNiki;

    private List<IDataPersistenceNiki> dataPersistenceObjects;

    public static DataPersistenceManagerNiki Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("found ");
        }
        Instance = this;
    }

    private void Start()
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.DataNiki = new DataNiki();
    }

    public void LoadGame()
    {
        if(this.DataNiki == null)
        {
            Debug.Log("NO Data was Found");
            NewGame();
        }

        foreach (IDataPersistenceNiki dataPresistenceObj in dataPersistenceObjects)
        {
            dataPresistenceObj.LoadData(DataNiki);
        }

        Debug.Log("Loaded deathCount = " + DataNiki.deathCount);
    }

    public void SaveGame()
    {
        foreach (IDataPersistenceNiki dataPresistenceObj in dataPersistenceObjects)
        {
            dataPresistenceObj.SaveData(ref DataNiki);
        }
        Debug.Log("Saved death count = " + DataNiki.deathCount);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistenceNiki> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistenceNiki> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistenceNiki>();

        return new List<IDataPersistenceNiki>(dataPersistenceObjects);
    }
}
