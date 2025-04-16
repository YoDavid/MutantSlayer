using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParallax : MonoBehaviour
{
    public Transform target; // הטרנספורם של השחקן
    public float smoothSpeed = 0.125f; // קצב ההחלקה של תנועת המצלמה
    public Vector3 offset = new Vector3(0f, 0f, -10f); // קיזוז המיקום של המצלמה מהשחקן

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            Debug.LogWarning("Target (Player) not assigned to the CameraFollow script.");
        }
    }
}