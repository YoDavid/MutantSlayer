using UnityEngine;

public class EnemyBehindWall : MonoBehaviour
{
    [SerializeField] private GameObject wallGameObject; // (Optional: Manually assign if needed)
    [SerializeField] public GameObject healthBar;

    private void Start()
    {
        // 1. Auto-find "BiuldOut" if not manually assigned
        if (wallGameObject == null)
        {
            wallGameObject = GameObject.Find("BiuldOut"); // Case-sensitive name match!
            if (wallGameObject == null)
                Debug.LogError("Could not find 'BiuldOut' GameObject!", this);
        }

        // 2. Auto-find health bar if not assigned (e.g., child object)
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.gameObject;
    }

    private void Update()
    {
        if (wallGameObject == null || healthBar == null)
            return;

        // Toggle health bar based on wall's active state
        healthBar.SetActive(!wallGameObject.activeInHierarchy);
    }
}