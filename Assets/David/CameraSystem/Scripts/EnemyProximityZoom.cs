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

    [System.Serializable]
    public class Level3ZoneProfile
    {
        public float zoneZoomSize = 12f;
        public float zoneYOffset = 4f;
        public float zoneZoomSmoothTime = 0.5f;
        public float zoneYOffsetSmoothTime = 0.5f;
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

    [Header("Level-3 Zone Settings")]
    [SerializeField]
    private Level3ZoneProfile level3Zone = new Level3ZoneProfile();

    [Header("Settings")]
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float yOffsetSmoothTime = 0.2f;
    [SerializeField] private bool showGizmos = true;

    [Header("Player Reference")]
    public Transform player;

    [Header("Level-3 Zone X Bounds")]
    [SerializeField] private float level3MinX = 100f;
    [SerializeField] private float level3MaxX = 150f;

    [Header("Level-3 Zone Y Bounds")]
    [SerializeField] private float level3MinY = 2f;
    [SerializeField] private float level3MaxY = 8f;

    private Camera cam;
    private List<Transform> allEnemies = new List<Transform>();
    private List<Transform> allBosses = new List<Transform>();
    private float zoomVelocity;
    private float yOffsetVelocity;
    private float originalZ;
    private bool enemiesInRange;
    private ZoomProfile activeProfile;
    [SerializeField] private bool inLevel3Zone = false;

    private CameraShake cameraShake;

    [SerializeField] private bool showMinXGizmo = true;
    [SerializeField] private bool showMinYGizmo = true;

    [Header("Profile Switching Protection")]
    [SerializeField] private float profileSwitchCooldown = 0.5f;
    private float lastProfileSwitchTime = -Mathf.Infinity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cameraShake = GetComponent<CameraShake>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        originalZ = transform.position.z;
        FindAllEnemies();
        SetActiveProfile(defaultProfile);
        ApplyImmediateZoom();
        ApplyImmediateYOffset();
    }


    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

       
            // X bounds visualization
            Debug.DrawLine(new Vector3(level3MinX, -1000, transform.position.z),
                           new Vector3(level3MinX, 1000, transform.position.z),
                           Color.red);
            Debug.DrawLine(new Vector3(level3MaxX, -1000, transform.position.z),
                           new Vector3(level3MaxX, 1000, transform.position.z),
                           Color.blue);

            // Y bounds visualization
            Debug.DrawLine(new Vector3(-1000, level3MinY, transform.position.z),
                           new Vector3(1000, level3MinY, transform.position.z),
                           Color.red);
            Debug.DrawLine(new Vector3(-1000, level3MaxY, transform.position.z),
                           new Vector3(1000, level3MaxY, transform.position.z),
                           Color.blue);
        

        // Rest of your logic...
        CheckLevel3ZoneByXBounds();

        if (!inLevel3Zone)
        {
            SetActiveProfileBasedOnProximity();
        }

        ApplySmoothZoom();
        ApplySmoothYOffset();

    }



    private void CheckLevel3ZoneByXBounds()
    {
        float camX = transform.position.x;
        float camY = transform.position.y;
        bool wasInLevel3 = inLevel3Zone;

        // Check both X and Y bounds
        inLevel3Zone = camX >= level3MinX && camX <= level3MaxX &&
                       camY >= level3MinY && camY <= level3MaxY;

        if (inLevel3Zone != wasInLevel3)
        {
            ApplySmoothZoom();
            ApplySmoothYOffset();
        }
    }


    private void SetActiveProfileBasedOnProximity()
    {
        if (Time.time - lastProfileSwitchTime < profileSwitchCooldown)
            return; // Still in cooldown

        bool bossDetected = CheckEnemiesInRange(allBosses, bossEnemyProfile.detectionRange);
        bool regularEnemyDetected = CheckEnemiesInRange(allEnemies, regularEnemyProfile.detectionRange);
        enemiesInRange = bossDetected || regularEnemyDetected;

        ZoomProfile newProfile = defaultProfile;

        if (bossDetected)
            newProfile = bossEnemyProfile;
        else if (regularEnemyDetected)
            newProfile = regularEnemyProfile;

        if (newProfile != activeProfile)
        {
            SetActiveProfile(newProfile);
            lastProfileSwitchTime = Time.time; 
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

        float targetSize = inLevel3Zone ? level3Zone.zoneZoomSize : activeProfile.zoomSize;
        float smoothTime = inLevel3Zone ? level3Zone.zoneZoomSmoothTime : zoomSmoothTime;

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetSize,
            ref zoomVelocity,
            smoothTime
        );
    }

    private void ApplyImmediateZoom()
    {
        float targetSize = inLevel3Zone ? level3Zone.zoneZoomSize :
                         (activeProfile != null ? activeProfile.zoomSize : defaultProfile.zoomSize);
        cam.orthographicSize = targetSize;
    }

    private void ApplySmoothYOffset()
    {
        if (player == null) return;

        float targetYOffset = inLevel3Zone ? level3Zone.zoneYOffset :
                              (activeProfile != null ? activeProfile.yOffset : 0f);
        float smoothTime = inLevel3Zone ? level3Zone.zoneYOffsetSmoothTime : yOffsetSmoothTime;

        Vector3 pos = transform.position;
        float newY = Mathf.SmoothDamp(
            pos.y,
            player.position.y + targetYOffset,
            ref yOffsetVelocity,
            smoothTime
        );

        Vector3 basePosition = new Vector3(
            player.position.x,
            newY,
            originalZ
        );

        Vector3 shake = cameraShake != null ? cameraShake.ShakeOffset : Vector3.zero;

        transform.position = basePosition + shake;
    }

    private void ApplyImmediateYOffset()
    {
        if (player == null) return;

        Vector3 pos = transform.position;
        float targetY = inLevel3Zone ?
            player.position.y + level3Zone.zoneYOffset :
            player.position.y + (activeProfile?.yOffset ?? 0f);

        transform.position = new Vector3(
            pos.x,
            targetY,
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

        // Draw detection range and Y offset for profiles
        DrawProfileGizmo(defaultProfile);
        DrawProfileGizmo(regularEnemyProfile);
        DrawProfileGizmo(bossEnemyProfile);

        // Draw Level 3 Zone Y Offset visual
        if (player != null)
        {
            float level3Y = player.position.y + level3Zone.zoneYOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(transform.position.x - 10, level3Y, transform.position.z),
                            new Vector3(transform.position.x + 10, level3Y, transform.position.z));
            Gizmos.DrawSphere(new Vector3(transform.position.x, level3Y, transform.position.z), 0.3f);
            Gizmos.color = Color.white;
        }

        // Draw MinX line (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(level3MinX, transform.position.y - 1000, transform.position.z),
                        new Vector3(level3MinX, transform.position.y + 1000, transform.position.z));
        Gizmos.DrawSphere(new Vector3(level3MinX, transform.position.y, transform.position.z), 0.5f);

        // Draw MaxX line (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(level3MaxX, transform.position.y - 1000, transform.position.z),
                        new Vector3(level3MaxX, transform.position.y + 1000, transform.position.z));
        Gizmos.DrawSphere(new Vector3(level3MaxX, transform.position.y, transform.position.z), 0.5f);

        // Draw MinY line (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1000, level3MinY, transform.position.z),
                        new Vector3(transform.position.x + 1000, level3MinY, transform.position.z));
        Gizmos.DrawSphere(new Vector3(transform.position.x, level3MinY, transform.position.z), 0.5f);

        // Draw MaxY line (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1000, level3MaxY, transform.position.z),
                        new Vector3(transform.position.x + 1000, level3MaxY, transform.position.z));
        Gizmos.DrawSphere(new Vector3(transform.position.x, level3MaxY, transform.position.z), 0.5f);

        Gizmos.color = Color.white;
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
