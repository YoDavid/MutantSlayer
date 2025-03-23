using UnityEngine;
using System.Collections;

public enum CameraState
{
    Combat,
    Exploration
}

public class CameraDeadZoneFollow : MonoBehaviour
{
    [Header("Camera States")]
    public CameraState currentState = CameraState.Exploration;
    public Transform player;
    public Transform enemy;

    [Header("Combat Settings")]
    public float combatDistanceThreshold;
    [SerializeField] private float combatCameraSize;

    [Header("Dead Zone Settings")]
    public Vector2 boundsSize = new Vector2(20f, 10f);

    [Header("Idle and Centering Settings")]
    [SerializeField] private float idleCenterTime;
    [SerializeField] private float initialCenterSpeed;
    [SerializeField] private float maxCenterSpeed;
    [SerializeField] private float speedIncreaseRate;

    [Header("Camera Management")]
    private float defaultCameraSize;
    private Camera cameraComponent;
    private Vector3 lastPlayerPosition;
    private float fixedZ;

    [Header("Idle Tracking")]
    [SerializeField] private float idleTimer;
    private bool isIdle;
    private float currentCenterSpeed;

    [Header("Shake Effect")]
    public float shakeDuration;
    public float minshakeMagnitude;
    public float maxshakeMagnitude;
    public float shakeMagnitude;
    public float dampingSpeed;
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float initialShakeMagnitude;

    [Header("Debugging")]
    public bool showGizmos = false;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Camera: No player assigned!");
            return;
        }

        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null || !cameraComponent.orthographic)
        {
            Debug.LogError("Camera: No orthographic camera found!");
            return;
        }

        defaultCameraSize = cameraComponent.orthographicSize;
        fixedZ = transform.position.z;
        lastPlayerPosition = player.position;
        idleTimer = 0f;
        currentCenterSpeed = initialCenterSpeed;
        initialShakeMagnitude = shakeMagnitude; // Store initial shake magnitude
    }

    void Update()
    {
        if (player == null) return;

        if (Vector2.Distance(player.position, enemy.position) < combatDistanceThreshold)
        {
            currentState = CameraState.Combat;
        }
        else
        {
            currentState = CameraState.Exploration;
        }

        switch (currentState)
        {
            case CameraState.Combat:
                HandleCombatCamera();
                break;
            case CameraState.Exploration:
                HandleExplorationCamera();
                break;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ShakeCamera();
        }
    }

    void HandleCombatCamera()
    {
        cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, combatCameraSize, Time.deltaTime * 2f);
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }

    void HandleExplorationCamera()
    {
        cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, defaultCameraSize, Time.deltaTime * 2f);

        Vector3 camPos = transform.position;
        Vector3 minBounds = new Vector3(camPos.x - boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);
        Vector3 maxBounds = new Vector3(camPos.x + boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);

        Vector3 newPos = camPos;
        Vector3 playerDelta = player.position - lastPlayerPosition;

        if (playerDelta.magnitude > 0)
        {
            idleTimer = 0f;
            isIdle = false;
            currentCenterSpeed = initialCenterSpeed;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleCenterTime)
            {
                isIdle = true;
            }
        }

        if (player.position.x < minBounds.x || player.position.x > maxBounds.x)
            newPos.x += playerDelta.x;

        if (player.position.y < minBounds.y || player.position.y > maxBounds.y)
            newPos.y += playerDelta.y;

        if (isIdle)
        {
            currentCenterSpeed = Mathf.Min(currentCenterSpeed + speedIncreaseRate * Time.deltaTime, maxCenterSpeed);
            newPos = Vector3.Lerp(camPos, player.position, Time.deltaTime * currentCenterSpeed);
        }

        transform.position = new Vector3(newPos.x, newPos.y, fixedZ);
        lastPlayerPosition = player.position;
    }

    public void ShakeCamera()
    {
        if (!isShaking)
        {
            shakeMagnitude = initialShakeMagnitude; // Reset shake magnitude
            originalPosition = transform.position; // Save the starting position
            StartCoroutine(Shake());
        }
    }

    private IEnumerator Shake()
    {
        isShaking = true;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(minshakeMagnitude, maxshakeMagnitude) * shakeMagnitude;
            float y = Random.Range(minshakeMagnitude, maxshakeMagnitude) * shakeMagnitude;

            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, fixedZ);

            elapsedTime += Time.deltaTime;
            shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0, dampingSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = originalPosition;
        isShaking = false;
    }

    void OnDrawGizmos()
    {
        if (player == null || !showGizmos) return;

        Gizmos.color = Color.green;
        Vector3 camPos = transform.position;
        Vector3 topLeft = new Vector3(camPos.x - boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);
        Vector3 topRight = new Vector3(camPos.x + boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);
        Vector3 bottomLeft = new Vector3(camPos.x - boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);
        Vector3 bottomRight = new Vector3(camPos.x + boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, combatDistanceThreshold);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
