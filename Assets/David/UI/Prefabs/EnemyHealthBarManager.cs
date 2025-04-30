using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform healthBarParent; // Assign a dedicated UI panel
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        SpawnHealthBarsForAllEnemies();
    }

    private void SpawnHealthBarsForAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            SpawnHealthBarForEnemy(enemy);
        }
    }

    public void SpawnHealthBarForEnemy(GameObject enemy)
    {
        var healthBar = Instantiate(healthBarPrefab, healthBarParent);
        var healthBarScript = healthBar.GetComponent<UIEnemyHealthBar>();

        if (healthBarScript != null && enemy.TryGetComponent(out EnemyHealth health))
        {
            healthBarScript.Initialize(health, mainCamera);
        }
    }
}