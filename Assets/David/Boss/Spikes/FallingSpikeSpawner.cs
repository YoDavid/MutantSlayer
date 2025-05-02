using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class FallingSpikeSpawner : MonoBehaviour
{
    [Header("Spike Prefabs")]
    [SerializeField] private GameObject spikePrefabSmall;
    [SerializeField] private GameObject spikePrefabMedium;
    [SerializeField] private GameObject spikePrefabLarge;
    [SerializeField] private GameObject warningPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRangeX = 5f;
    [SerializeField] private float warningDuration = 0.3f;
    [SerializeField] private float warningYPosition = 0.3f;

    [Header("Smash Spike Count")]
    [SerializeField] private int smashMinCount = 3;
    [SerializeField] private int smashMaxCount = 7;

    private ObjectPool<GameObject> spikePoolSmall;
    private ObjectPool<GameObject> spikePoolMedium;
    private ObjectPool<GameObject> spikePoolLarge;

    private void Awake()
    {
        spikePoolSmall = CreatePool(spikePrefabSmall);
        spikePoolMedium = CreatePool(spikePrefabMedium);
        spikePoolLarge = CreatePool(spikePrefabLarge);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnGet: (spike) => spike.SetActive(true),
            actionOnRelease: (spike) => spike.SetActive(false),
            actionOnDestroy: (spike) => Destroy(spike),
            defaultCapacity: 10
        );
    }

    // Smash Attack
    public void SpawnSpikesSmash()
    {
        int count = Random.Range(smashMinCount, smashMaxCount + 1);
        for (int i = 0; i < count; i++)
        {
            float offsetX = Random.Range(-spawnRangeX, spawnRangeX);
            float spawnX = transform.position.x + offsetX;
            int randomType = Random.Range(0, 3);
            ObjectPool<GameObject> pool = randomType switch
            {
                0 => spikePoolSmall,
                1 => spikePoolMedium,
                _ => spikePoolLarge
            };

            StartCoroutine(SpawnSpikeWithDelay(pool, spawnX, transform.position.y));
        }
    }


    public void SpawnSpikes1()
    {
        float spikeY = transform.position.y;
        float spawnX = transform.position.x + Random.Range(-spawnRangeX, spawnRangeX);
        int randomType = Random.Range(0, 3);
        ObjectPool<GameObject> pool = randomType switch
        {
            0 => spikePoolSmall,
            1 => spikePoolMedium,
            _ => spikePoolLarge
        };

        StartCoroutine(SpawnSpikeWithDelay(pool, spawnX, spikeY)); 
    }

    public void SpawnSpikes2()
    {
        float spikeY = transform.position.y;
        for (int i = 0; i < 2; i++) 
        {
            float spawnX = transform.position.x + Random.Range(-spawnRangeX, spawnRangeX);
            int randomType = Random.Range(0, 3);
            ObjectPool<GameObject> pool = randomType switch
            {
                0 => spikePoolSmall,
                1 => spikePoolMedium,
                _ => spikePoolLarge
            };

            StartCoroutine(SpawnSpikeWithDelay(pool, spawnX, spikeY)); 
        }
    }

    public void SpawnSpikes3()
    {
        float spikeY = transform.position.y;
        for (int i = 0; i < 3; i++)
        {
            float spawnX = transform.position.x + Random.Range(-spawnRangeX, spawnRangeX);
            int randomType = Random.Range(0, 3);
            ObjectPool<GameObject> pool = randomType switch
            {
                0 => spikePoolSmall,
                1 => spikePoolMedium,
                _ => spikePoolLarge
            };

            StartCoroutine(SpawnSpikeWithDelay(pool, spawnX, spikeY));  
        }
    }

  

    private void SpawnSpike(ObjectPool<GameObject> pool, float xPos, float spikeY)
    {
        GameObject warning = Instantiate(warningPrefab, new Vector3(xPos, warningYPosition, 0), Quaternion.identity);

        GameObject spike = pool.Get();
        spike.transform.position = new Vector3(xPos, spikeY, 0);
        spike.SetActive(true);
        spike.GetComponent<FallingSpike>().Init(pool);

        Destroy(warning, warningDuration);
    }

    private IEnumerator SpawnSpikeWithDelay(ObjectPool<GameObject> pool, float xPos, float spikeY)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));

        // Spawn the spike
        SpawnSpike(pool, xPos, spikeY);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Visualize the warning position (in the X and Y plane)
        Vector3 warningPosition = new Vector3(transform.position.x, warningYPosition, 0);

        // Draw a small sphere at the warning position
        Gizmos.DrawSphere(warningPosition, 0.2f);

        // Optional: label the gizmo for clarity
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, warningPosition);  // Line from the spawner to the warning position
    }
}
