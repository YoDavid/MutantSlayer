using UnityEngine;

public class EnemyHealthBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform uiManager;
    [SerializeField] private Camera targetCamera; // Assign in inspector

    private void Start()
    {
        SpawnHealthBarForEnemy();
    }
    private void SpawnHealthBarForEnemy()
    {
        GameObject healthBar = Instantiate(healthBarPrefab, uiManager);
        UIEnemyHealthBar healthBarScript = healthBar.GetComponent<UIEnemyHealthBar>();
        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

        if (healthBarScript != null && enemyHealth != null)
        { 
            healthBarScript.enemyHealth = enemyHealth;
            healthBarScript.Initialize(enemyHealth, targetCamera);

        }
    }
}
