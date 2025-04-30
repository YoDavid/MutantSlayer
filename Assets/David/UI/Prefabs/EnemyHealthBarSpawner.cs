using UnityEngine;

public class EnemyHealthBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;  // Prefab for the health bar
    [SerializeField] private Transform uiManager;         // UIManager reference to spawn health bar under it

    private void Start()
    {
        SpawnHealthBarForEnemy();
    }

    private void SpawnHealthBarForEnemy()
    {
        // Instantiate the health bar under the UIManager
        GameObject healthBar = Instantiate(healthBarPrefab, uiManager);

        // Get the UIEnemyHealthBar component from the health bar prefab
        UIEnemyHealthBar healthBarScript = healthBar.GetComponent<UIEnemyHealthBar>();
        if (healthBarScript != null)
        {
            // Set the health bar's enemy reference dynamically
            healthBarScript.enemyHealth = GetComponent<EnemyHealth>();  // Set the health component of the enemy
        }
    }
}
