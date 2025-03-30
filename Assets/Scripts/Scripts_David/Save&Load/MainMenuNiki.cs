using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuNiki : MonoBehaviour
{
    public void onNewGame()
    {
        DataPersistenceManagerNiki.Instance.NewGame();
    }

    public void onLoadGame()
    {
        DataPersistenceManagerNiki.Instance.LoadGame();
    }

    public void onSaveGame()
    {
        DataPersistenceManagerNiki.Instance.SaveGame();
    }
}
