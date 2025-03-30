using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustCamera : MonoBehaviour
{
    [SerializeField] Transform playerPosition;
    [SerializeField] private float cameraZPosition = -10f;

    private void Update()
    {
        this.transform.position = new Vector3(playerPosition.position.x, playerPosition.position.y, cameraZPosition);
    }
}
