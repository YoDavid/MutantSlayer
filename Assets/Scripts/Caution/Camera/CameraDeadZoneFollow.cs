using UnityEngine;

public class CameraDeadZoneFollow : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Position Settings")]
    [SerializeField] private float cameraZPosition = -10f;

    [Header("Dead Zone Settings")]
    public Vector2 boundsSize = new Vector2(20f, 10f);

    [Header("Behavior Settings")]
    [SerializeField] private float idleCenterTime = 2f;
    [SerializeField] private float initialCenterSpeed = 2f;
    [SerializeField] private float maxCenterSpeed = 5f;
    [SerializeField] private float speedIncreaseRate = 0.5f;
    [SerializeField] private float enemyCenterSpeed = 5f;

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

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            cameraZPosition
        );

        InitializeReferences();
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
        currentCenterSpeed = initialCenterSpeed;
        transform.position = new Vector3(player.position.x, player.position.y, cameraZPosition);
    }

    private void Update()
    {
        if (player == null) return;

        HandleCameraMovement();
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
        // Only modify X position (let EnemyProximityZoom handle Y)
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
        Vector3 minBounds = new Vector3(camPos.x - boundsSize.x / 2, -Mathf.Infinity, camPos.z); // No Y bounds
        Vector3 maxBounds = new Vector3(camPos.x + boundsSize.x / 2, Mathf.Infinity, camPos.z); // No Y bounds

        UpdateIdleState();

        float newX = camPos.x;
        Vector3 playerDelta = player.position - lastPlayerPosition;

        // Only check X-axis bounds (ignore Y)
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

        // Apply only X movement (Y remains controlled by EnemyProximityZoom)
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

    private void OnDrawGizmos()
    {
        if (!showGizmos || player == null) return;

        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(boundsSize.x, boundsSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}