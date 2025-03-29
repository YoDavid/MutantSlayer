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

    private Camera cam;
    private Dictionary<string, Transform> activeEnemies = new Dictionary<string, Transform>();
    private float targetZoom;
    private float zoomVelocity;
    private float targetYOffset;
    private float yOffsetVelocity;
    private float originalZ;
    private bool enemiesInRange;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originalZ = transform.position.z;
        InitializeDefaults();
        CacheActiveEnemies();
    }

    private void InitializeDefaults()
    {
        targetZoom = defaultProfile.zoomSize;
        targetYOffset = defaultProfile.yOffset;
        cam.orthographicSize = targetZoom;
        ApplyImmediateYOffset();
    }

    private void Update()
    {
        UpdateActiveProfile();
        ApplySmoothZoom();
        ApplySmoothYOffset();
    }

    public bool AreEnemiesInRange()
    {
        return enemiesInRange;
    }

    private void CacheActiveEnemies()
    {
        activeEnemies.Clear();
        CacheEnemiesWithTag(regularEnemyProfile.enemyTag);
        CacheEnemiesWithTag(bossEnemyProfile.enemyTag);
    }

    private void CacheEnemiesWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;

        foreach (var enemy in GameObject.FindGameObjectsWithTag(tag))
        {
            if (!activeEnemies.ContainsKey(tag) || activeEnemies[tag] == null)
            {
                activeEnemies[tag] = enemy.transform;
            }
        }
    }

    private void UpdateActiveProfile()
    {
        bool bossDetected = CheckEnemyInRange(bossEnemyProfile);
        bool regularEnemyDetected = CheckEnemyInRange(regularEnemyProfile);
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

    private bool CheckEnemyInRange(ZoomProfile profile)
    {
        if (string.IsNullOrEmpty(profile.enemyTag)) return false;
        if (!activeEnemies.TryGetValue(profile.enemyTag, out Transform enemy) || enemy == null) return false;

        return Vector2.Distance(transform.position, enemy.position) < profile.detectionRange;
    }

    private void SetActiveProfile(ZoomProfile profile)
    {
        targetZoom = profile.zoomSize;
        targetYOffset = profile.yOffset;
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
        float newY = Mathf.SmoothDamp(
            pos.y,
            targetYOffset,
            ref yOffsetVelocity,
            yOffsetSmoothTime
        );
        transform.position = new Vector3(pos.x, newY, originalZ);
    }

    private void ApplyImmediateYOffset()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, targetYOffset, originalZ);
    }

    public void RegisterEnemy(Transform enemy, bool isBoss = false)
    {
        string tag = isBoss ? bossEnemyProfile.enemyTag : regularEnemyProfile.enemyTag;
        if (!string.IsNullOrEmpty(tag))
        {
            activeEnemies[tag] = enemy;
        }
    }

    public void UnregisterEnemy(Transform enemy)
    {
        List<string> toRemove = new List<string>();
        foreach (var kvp in activeEnemies)
        {
            if (kvp.Value == enemy) toRemove.Add(kvp.Key);
        }
        foreach (var key in toRemove)
        {
            activeEnemies.Remove(key);
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