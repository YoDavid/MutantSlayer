using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistenceNiki
{
    void LoadData(DataNiki dataNiki);
    void SaveData(ref DataNiki dataNiki);
}
