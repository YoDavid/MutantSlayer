using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class DataNiki
{
  public int deathCount;
    public Vector3 playerPosition;

    public DataNiki()
    {
        this.deathCount = 0;
        playerPosition = Vector3.zero;
    }
}
