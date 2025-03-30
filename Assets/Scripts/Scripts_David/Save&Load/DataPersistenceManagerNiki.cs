using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManagerNiki : MonoBehaviour
{
    [Header("File Store Config")]
    [SerializeField] private string fileName;

    private DataNiki DataNiki;

    private List<IDataPersistenceNiki> dataPersistenceObjects;

    private FileDataHandlerNiki dataHandlerNiki;

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
        this.dataHandlerNiki = new FileDataHandlerNiki(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.DataNiki = new DataNiki();
    }

    public void LoadGame()
    {
        this.DataNiki = dataHandlerNiki.Load();

        if(this.DataNiki == null)
        {
            Debug.Log("NO Data was Found");
            NewGame();
        }

        foreach (IDataPersistenceNiki dataPresistenceObj in dataPersistenceObjects)
        {
            dataPresistenceObj.LoadData(DataNiki);
        }

    }

    public void SaveGame()
    {
        foreach (IDataPersistenceNiki dataPresistenceObj in dataPersistenceObjects)
        {
            dataPresistenceObj.SaveData(ref DataNiki);
        }

        dataHandlerNiki.Save(DataNiki);
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
