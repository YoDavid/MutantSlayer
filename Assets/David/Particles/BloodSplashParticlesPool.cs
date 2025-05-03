using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplashParticlesPool : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject deathSplashPrefab;
    [SerializeField] private GameObject hitSplashPrefab;

    [Header("Pool Sizes")]
    [SerializeField] private int deathPoolSize = 10;
    [SerializeField] private int hitPoolSize = 10;

    private Queue<GameObject> deathPool = new Queue<GameObject>();
    private Queue<GameObject> hitPool = new Queue<GameObject>();

    private bool isPersistent = false;

    private void Awake()
    {
        // Check if this object is persistent
        isPersistent = gameObject.scene.name == "DontDestroyOnLoad";

        // Ensure this object is active
        gameObject.SetActive(true);

        InitializePool(deathPool, deathSplashPrefab, deathPoolSize);
        InitializePool(hitPool, hitSplashPrefab, hitPoolSize);
    }

    private void InitializePool(Queue<GameObject> pool, GameObject prefab, int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject obj = isPersistent ? Instantiate(prefab) : Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void PlayDeathSplash(Vector3 position)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        Debug.Log("Death splash");
        PlaySplash(deathPool, deathSplashPrefab, position);
    }

    public void PlayHitSplash(Vector3 position)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        Debug.Log("Hit splash");
        PlaySplash(hitPool, hitSplashPrefab, position);
    }

    private void PlaySplash(Queue<GameObject> pool, GameObject prefab, Vector3 position)
    {
        if (pool == null || prefab == null) return;

        GameObject obj = null;

        // Try to get from pool first
        while (pool.Count > 0 && obj == null)
        {
            obj = pool.Dequeue();
            if (obj == null) continue; // Skip destroyed objects
        }

        // If no available object in pool, instantiate a new one
        if (obj == null)
        {
            obj = isPersistent ? Instantiate(prefab) : Instantiate(prefab, transform);
        }

        if (obj == null) return;

        obj.transform.position = position;
        obj.SetActive(true);

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPoolAfterDelay(obj, ps.main.duration, pool));
        }
        else
        {
            StartCoroutine(ReturnToPoolAfterDelay(obj, 0.5f, pool));
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay, Queue<GameObject> pool)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
        {
            obj.SetActive(false);
            if (pool != null) pool.Enqueue(obj);
        }
    }
}