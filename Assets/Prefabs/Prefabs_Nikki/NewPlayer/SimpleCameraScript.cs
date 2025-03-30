using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraScript : MonoBehaviour
{
    [SerializeField] Transform playerPosition;

    private void Update()
    {
        this.transform.position = playerPosition.position;
    }
}
