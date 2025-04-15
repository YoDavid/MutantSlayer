using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class EnemyProximityZoom : MonoBehaviour
{
    [System.Serializable]
    public class ZoomProfile
    {
        public string name;
        public string enemyTag;
        public float detectionRange = 35f;
        public float zoomSize = 5f;
        public float yOffset = 0f;
        public Color gizmoColor = Color.white;
    }

    [Header("Zoom Profiles")]
    [SerializeField]
    private ZoomProfile defaultProfile = new ZoomProfile()
    {
        name = "Default",
        enemyTag = "",
        zoomSize = 5f,
        yOffset = 0f,
        gizmoColor = Color.gray
    };

    [SerializeField]
    private ZoomProfile regularEnemyProfile = new ZoomProfile()
    {
        name = "Regular Enemy",
        enemyTag = "Enemy",
        detectionRange = 35f,
        zoomSize = 7f,
        yOffset = 1f,
        gizmoColor = Color.red
    };

    [SerializeField]
    private ZoomProfile bossEnemyProfile = new ZoomProfile()
    {
        name = "Boss Enemy",
        enemyTag = "BossEnemy",
        detectionRange = 45f,
        zoomSize = 9f,
        yOffset = 2f,
        gizmoColor = Color.yellow
    };

    [Header("Settings")]
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float yOffsetSmoothTime = 0.2f;
    [SerializeField] private bool showGizmos = true;

    [Header("Player Reference")] // NEW: Add player reference
    public Transform player;

    private Camera cam;
    private List<Transform> allEnemies = new List<Transform>();
    private List<Transform> allBosses = new List<Transform>();
    private float targetZoom;
    private float zoomVelocity;
    private float targetYOffset;
    private float yOffsetVelocity;
    private float originalZ;
    private bool enemiesInRange;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        originalZ = transform.position.z;

        InitializeDefaults();
        FindAllEnemies();
        CheckInitialZoom();

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            originalZ
        );
    }


    private void InitializeDefaults()
    {
        targetZoom = defaultProfile.zoomSize;
        targetYOffset = defaultProfile.yOffset;
        cam.orthographicSize = targetZoom;
        ApplyImmediateYOffset();
    }

    private void FindAllEnemies()
    {
        // Clear existing lists
        allEnemies.Clear();
        allBosses.Clear();

        // Find all objects with Enemy tag
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag(regularEnemyProfile.enemyTag);
        foreach (GameObject enemy in enemyObjects)
        {
            allEnemies.Add(enemy.transform);
        }

        // Find all objects with EnemyBoss tag
        GameObject[] bossObjects = GameObject.FindGameObjectsWithTag(bossEnemyProfile.enemyTag);
        foreach (GameObject boss in bossObjects)
        {
            allBosses.Add(boss.transform);
        }
    }

    private void CheckInitialZoom()
    {
        // Check if there are any bosses in range at start
        foreach (Transform boss in allBosses)
        {
            if (boss != null && Vector2.Distance(transform.position, boss.position) < bossEnemyProfile.detectionRange)
            {
                SetActiveProfile(bossEnemyProfile);
                ApplyImmediateZoom();
                return;
            }
        }

        // Check if there are any regular enemies in range at start
        foreach (Transform enemy in allEnemies)
        {
            if (enemy != null && Vector2.Distance(transform.position, enemy.position) < regularEnemyProfile.detectionRange)
            {
                SetActiveProfile(regularEnemyProfile);
                ApplyImmediateZoom();
                return;
            }
        }

        // Default if no enemies found
        SetActiveProfile(defaultProfile);
        ApplyImmediateZoom();
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            UpdateActiveProfile();
            ApplySmoothZoom();
            ApplySmoothYOffset();
        }
    }


    public bool AreEnemiesInRange()
    {
        return enemiesInRange;
    }

    private void UpdateActiveProfile()
    {
        bool bossDetected = CheckEnemiesInRange(allBosses, bossEnemyProfile.detectionRange);
        bool regularEnemyDetected = CheckEnemiesInRange(allEnemies, regularEnemyProfile.detectionRange);
        enemiesInRange = bossDetected || regularEnemyDetected;

        if (bossDetected)
        {
            SetActiveProfile(bossEnemyProfile);
        }
        else if (regularEnemyDetected)
        {
            SetActiveProfile(regularEnemyProfile);
        }
        else
        {
            SetActiveProfile(defaultProfile);
        }
    }

    private bool CheckEnemiesInRange(List<Transform> enemies, float range)
    {
        foreach (Transform enemy in enemies)
        {
            if (enemy != null && Vector2.Distance(transform.position, enemy.position) < range)
            {
                return true;
            }
        }
        return false;
    }

    private void SetActiveProfile(ZoomProfile profile)
    {
        targetZoom = profile.zoomSize;
        targetYOffset = profile.yOffset;
    }

    private void ApplyImmediateZoom()
    {
        cam.orthographicSize = targetZoom;
    }

    private void ApplySmoothZoom()
    {
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private void ApplySmoothYOffset()
    {
        Vector3 pos = transform.position;
        if (player == null) return;
        float newY = Mathf.SmoothDamp(
            pos.y,
            player.position.y + targetYOffset, // Apply offset relative to player
            ref yOffsetVelocity,
            yOffsetSmoothTime
        );
        if (player == null) return;
        transform.position = new Vector3(
            pos.x, // Keep X (controlled by CameraDeadZoneFollow)
            newY,
            originalZ // Keep Z at -10
        );
    }

    private void ApplyImmediateYOffset()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(
            pos.x,
            player.position.y + targetYOffset, // Immediate Y adjustment
            originalZ
        );
    }

    public void RegisterEnemy(Transform enemy, bool isBoss = false)
    {
        if (isBoss)
        {
            if (!allBosses.Contains(enemy))
            {
                allBosses.Add(enemy);
            }
        }
        else
        {
            if (!allEnemies.Contains(enemy))
            {
                allEnemies.Add(enemy);
            }
        }
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (allBosses.Contains(enemy))
        {
            allBosses.Remove(enemy);
        }
        if (allEnemies.Contains(enemy))
        {
            allEnemies.Remove(enemy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || cam == null) return;

        DrawProfileGizmo(defaultProfile);
        DrawProfileGizmo(regularEnemyProfile);
        DrawProfileGizmo(bossEnemyProfile);
    }

    private void DrawProfileGizmo(ZoomProfile profile)
    {
        Gizmos.color = profile.gizmoColor;
        Gizmos.DrawWireSphere(transform.position, profile.detectionRange);
        Vector3 offsetPos = transform.position + Vector3.up * profile.yOffset;
        Gizmos.DrawLine(transform.position, offsetPos);
        Gizmos.DrawSphere(offsetPos, 0.3f);
    }
}