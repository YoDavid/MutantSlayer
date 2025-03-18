using UnityEngine;

public class CameraDeadZoneFollow : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public Vector2 boundsSize = new Vector2(5f, 3f); // Width & Height of the dead zone
    [SerializeField] private float idleCenterTime = 3f; // Time after which the camera centers on the player (exposed in Inspector)
    [SerializeField] private float initialCenterSpeed = 2f; // Initial camera centering speed
    [SerializeField] private float maxCenterSpeed = 10f; // Maximum camera centering speed
    [SerializeField] private float speedIncreaseRate = 1f; // Rate at which the speed increases

    private Vector3 lastPlayerPosition;
    private float fixedZ; // Keep Z constant
    [SerializeField] private float idleTimer; // Timer for tracking idle time
    private bool isIdle; // To check if the player is idle
    private float currentCenterSpeed; // Current speed of centering

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Camera_Test: No player assigned!");
            return;
        }
        fixedZ = transform.position.z; // Store the initial Z position
        lastPlayerPosition = player.position; // Start tracking the player
        idleTimer = 0f;
        currentCenterSpeed = initialCenterSpeed; // Set initial speed
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 camPos = transform.position;
        Vector3 minBounds = new Vector3(camPos.x - boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);
        Vector3 maxBounds = new Vector3(camPos.x + boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);

        Vector3 newPos = camPos;
        Vector3 playerDelta = player.position - lastPlayerPosition; // Get player's movement

        // Check if the player is moving or idle
        if (playerDelta.magnitude > 0)
        {
            idleTimer = 0f; // Reset idle timer if the player is moving
            isIdle = false;
            currentCenterSpeed = initialCenterSpeed; // Reset speed to initial when the player starts moving
        }
        else
        {
            idleTimer += Time.deltaTime; // Increment idle timer if the player isn't moving
            if (idleTimer >= idleCenterTime)
            {
                isIdle = true; // Player is idle, so start centering the camera
            }
        }

        // Move camera only when the player is outside the bounds
        if (player.position.x < minBounds.x || player.position.x > maxBounds.x)
            newPos.x += playerDelta.x; // Move at player's speed

        if (player.position.y < minBounds.y || player.position.y > maxBounds.y)
            newPos.y += playerDelta.y; // Move at player's speed

        // If the player is idle, start centering the camera gradually
        if (isIdle)
        {
            // Increase centering speed gradually until it reaches the max speed
            currentCenterSpeed = Mathf.Min(currentCenterSpeed + speedIncreaseRate * Time.deltaTime, maxCenterSpeed);

            // Gradually move the camera to center on the player
            newPos = Vector3.Lerp(camPos, player.position, Time.deltaTime * currentCenterSpeed);
        }

        // Keep Z position fixed
        transform.position = new Vector3(newPos.x, newPos.y, fixedZ);

        // Update last player position for the next frame
        lastPlayerPosition = player.position;
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Vector3 camPos = transform.position;
        Vector3 topLeft = new Vector3(camPos.x - boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);
        Vector3 topRight = new Vector3(camPos.x + boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);
        Vector3 bottomLeft = new Vector3(camPos.x - boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);
        Vector3 bottomRight = new Vector3(camPos.x + boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
