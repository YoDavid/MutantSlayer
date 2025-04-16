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

    [Header("Player Reference")]
    public Transform player;

    private Camera cam;
    private List<Transform> allEnemies = new List<Transform>();
    private List<Transform> allBosses = new List<Transform>();
    private float zoomVelocity;
    private float yOffsetVelocity;
    private float originalZ;
    private bool enemiesInRange;
    private ZoomProfile activeProfile;

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

        FindAllEnemies();
        SetActiveProfileBasedOnProximity(); // This replaces CheckInitialZoom()

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            originalZ
        );

        ApplyImmediateZoom();
        ApplyImmediateYOffset();
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        SetActiveProfileBasedOnProximity();
        ApplySmoothZoom();
        ApplySmoothYOffset();
    }

    private void SetActiveProfileBasedOnProximity()
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

    private void SetActiveProfile(ZoomProfile profile)
    {
        if (activeProfile != profile)
        {
            activeProfile = profile;
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

    private void ApplySmoothZoom()
    {
        if (activeProfile == null) return;

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            activeProfile.zoomSize,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private void ApplyImmediateZoom()
    {
        if (activeProfile != null)
            cam.orthographicSize = activeProfile.zoomSize;
    }

    private void ApplySmoothYOffset()
    {
        if (player == null || activeProfile == null) return;

        Vector3 pos = transform.position;
        float newY = Mathf.SmoothDamp(
            pos.y,
            player.position.y + activeProfile.yOffset,
            ref yOffsetVelocity,
            yOffsetSmoothTime
        );

        transform.position = new Vector3(
            pos.x,
            newY,
            originalZ
        );
    }

    private void ApplyImmediateYOffset()
    {
        if (player == null || activeProfile == null) return;

        Vector3 pos = transform.position;
        transform.position = new Vector3(
            pos.x,
            player.position.y + activeProfile.yOffset,
            originalZ
        );
    }

    private void FindAllEnemies()
    {
        allEnemies.Clear();
        allBosses.Clear();

        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag(regularEnemyProfile.enemyTag);
        foreach (GameObject enemy in enemyObjects)
        {
            allEnemies.Add(enemy.transform);
        }

        GameObject[] bossObjects = GameObject.FindGameObjectsWithTag(bossEnemyProfile.enemyTag);
        foreach (GameObject boss in bossObjects)
        {
            allBosses.Add(boss.transform);
        }
    }

    public bool AreEnemiesInRange() => enemiesInRange;

    public void RegisterEnemy(Transform enemy, bool isBoss = false)
    {
        if (isBoss)
        {
            if (!allBosses.Contains(enemy))
                allBosses.Add(enemy);
        }
        else
        {
            if (!allEnemies.Contains(enemy))
                allEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Transform enemy)
    {
        allBosses.Remove(enemy);
        allEnemies.Remove(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

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
