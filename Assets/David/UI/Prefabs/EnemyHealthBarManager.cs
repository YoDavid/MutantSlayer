// EnemyHealthBarManager.cs
using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform healthBarParent;
    [SerializeField] private Camera mainCamera; // Assign in inspector

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        SpawnHealthBarsForAllEnemies();
    }

    private void SpawnHealthBarsForAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            SpawnHealthBarForEnemy(enemy);
        }
    }

    public void SpawnHealthBarForEnemy(GameObject enemy)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, healthBarParent);
        UIEnemyHealthBar healthBarScript = healthBar.GetComponent<UIEnemyHealthBar>();

        if (healthBarScript != null)
        {
            healthBarScript.Initialize(enemy.GetComponent<EnemyHealth>(), mainCamera);
        }
    }
}