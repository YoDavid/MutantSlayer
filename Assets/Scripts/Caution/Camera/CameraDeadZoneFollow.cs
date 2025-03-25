using UnityEngine;

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

    [Header("Camera Shake References")]
    public CameraShake cameraShake; 

    [Header("Debugging")]
    public bool showGizmos = false;

    void Start()
    {
        InitializeReferences();

    }

    private void InitializeReferences()
    {
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, -10f);
        }

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
    }

    void Update()
    {
        UpdateCameraState();
        HandleCameraState();
    }

    private void UpdateCameraState()
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
    }

    private void HandleCameraState()
    {
        switch (currentState)
        {
            case CameraState.Combat:
                HandleCombatCamera();
                break;
            case CameraState.Exploration:
                HandleExplorationCamera();
                break;
        }
    }

    private void HandleCombatCamera()
    {
        cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, combatCameraSize, Time.deltaTime * 2f);
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }

    private void HandleExplorationCamera()
    {
        cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, defaultCameraSize, Time.deltaTime * 2f);

        Vector3 camPos = transform.position;
        Vector3 minBounds = new Vector3(camPos.x - boundsSize.x / 2, camPos.y - boundsSize.y / 2, camPos.z);
        Vector3 maxBounds = new Vector3(camPos.x + boundsSize.x / 2, camPos.y + boundsSize.y / 2, camPos.z);

        Vector3 newPos = camPos;
        Vector3 playerDelta = player.position - lastPlayerPosition;

        UpdateIdleState();

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

    private void UpdateIdleState()
    {
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
