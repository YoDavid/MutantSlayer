using UnityEngine;

public class CameraDeadZoneFollow : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Position Settings")]
    [SerializeField] private float cameraZPosition = -10f;

    [Header("Dead Zone Settings")]
    [Tooltip("Size of the dead zone where player can move without camera following")]
    public Vector2 boundsSize = new Vector2(20f, 10f);

    [Header("Behavior Settings")]
    [Tooltip("Time before camera starts centering on idle player")]
    [SerializeField] private float idleCenterTime = 2f;
    [Tooltip("Initial speed when starting to center on player")]
    [SerializeField] private float initialCenterSpeed = 2f;
    [Tooltip("Maximum speed when centering on player")]
    [SerializeField] private float maxCenterSpeed = 5f;
    [Tooltip("How quickly centering speed increases")]
    [SerializeField] private float speedIncreaseRate = 0.5f;
    [Tooltip("Speed when centering due to enemies")]
    [SerializeField] private float enemyCenterSpeed = 5f;

    [Header("Teleport Detection")]
    [Tooltip("Minimum distance to consider movement a teleport")]
    [SerializeField] private float teleportDistanceThreshold = 5f;
    [Tooltip("Maximum time window to detect teleport movement")]
    [SerializeField] private float teleportTimeThreshold = 0.5f;

    [Header("Component References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private EnemyProximityZoom proximityZoom;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = false;

    private Camera cam;
    private Vector3 lastPlayerPosition;
    private float currentCenterSpeed;
    private float idleTimer;
    private bool isIdle;
    private float movementTimer;
    private Vector3 previousPlayerPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        ForceCameraReposition(); // Changed from direct position set
        InitializeReferences();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

    }


    private void InitializeReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (cameraShake == null) cameraShake = GetComponent<CameraShake>();
        if (proximityZoom == null) proximityZoom = GetComponent<EnemyProximityZoom>();

        lastPlayerPosition = player.position;
        previousPlayerPosition = player.position;
        currentCenterSpeed = initialCenterSpeed;
        movementTimer = 0f;
        transform.position = new Vector3(player.position.x, player.position.y, cameraZPosition);
    }

    private void Update()
    {
        if (player == null) return;

        HandleTeleportDetection();
        HandleCameraMovement();
    }

    private void HandleTeleportDetection()
    {
        movementTimer += Time.deltaTime;

        float distanceMoved = Vector3.Distance(player.position, previousPlayerPosition);
        if (distanceMoved > teleportDistanceThreshold && movementTimer < teleportTimeThreshold)
        {
            CenterOnPlayerImmediately();
            ResetTeleportDetection();
        }

        if (movementTimer >= teleportTimeThreshold)
        {
            ResetTeleportDetection();
        }
    }

    private void ResetTeleportDetection()
    {
        movementTimer = 0f;
        previousPlayerPosition = player.position;
    }

    private void HandleCameraMovement()
    {
        if (proximityZoom != null && proximityZoom.AreEnemiesInRange())
        {
            CenterOnPlayerImmediately();
            return;
        }

        ApplyDeadZoneBehavior();
    }

    private void CenterOnPlayerImmediately()
    {
        Vector3 target = new Vector3(
            player.position.x,
            transform.position.y, // Keep current Y (controlled by EnemyProximityZoom)
            cameraZPosition
        );

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            enemyCenterSpeed * Time.deltaTime
        );
        lastPlayerPosition = player.position;
    }

    private void ApplyDeadZoneBehavior()
    {
        Vector3 camPos = transform.position;
        Vector3 minBounds = new Vector3(camPos.x - boundsSize.x / 2, -Mathf.Infinity, camPos.z);
        Vector3 maxBounds = new Vector3(camPos.x + boundsSize.x / 2, Mathf.Infinity, camPos.z);

        UpdateIdleState();

        float newX = camPos.x;
        Vector3 playerDelta = player.position - lastPlayerPosition;

        if (player.position.x < minBounds.x || player.position.x > maxBounds.x)
            newX += playerDelta.x;

        if (isIdle)
        {
            currentCenterSpeed = Mathf.Min(
                currentCenterSpeed + speedIncreaseRate * Time.deltaTime,
                maxCenterSpeed
            );
            newX = Mathf.Lerp(
                camPos.x,
                player.position.x,
                currentCenterSpeed * Time.deltaTime
            );
        }

        transform.position = new Vector3(newX, transform.position.y, cameraZPosition);
        lastPlayerPosition = player.position;
    }

    private void UpdateIdleState()
    {
        Vector3 playerDelta = player.position - lastPlayerPosition;

        if (playerDelta.magnitude > 0.01f)
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

    public void ForceCameraReposition()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            Vector3 targetPos = new Vector3(
                player.position.x,
                player.position.y,
                cameraZPosition
            );

            transform.position = targetPos;
            lastPlayerPosition = player.position;
            previousPlayerPosition = player.position;

            Debug.Log("Camera forcibly repositioned to player");
        }
    }

    private void OnEnable()
    {
        // Reset camera when enabled
        if (player != null)
        {
            ForceCameraReposition();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || player == null) return;

        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(boundsSize.x, boundsSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}