using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Test : MonoBehaviour
{
    // Reference to the player's transform
    public Transform player;

    // Offset between the camera and the player
    public Vector3 offset = new Vector3(0, 0, -10); // Default offset for 2D (z = -10 for camera distance)

    // Smoothing speed for camera movement
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // Check if the player is assigned
        if (player != null)
        {
            // Calculate the desired position for the camera
            Vector3 desiredPosition = player.position + offset;

            // Smoothly interpolate between the current camera position and the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Update the camera's position
            transform.position = smoothedPosition;
        }
        else
        {
            Debug.LogWarning("Player not assigned to the CameraFollow script.");
        }
    }
}