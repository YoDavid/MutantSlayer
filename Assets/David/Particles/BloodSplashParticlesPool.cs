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

    private void Awake()
    {
        for (int i = 0; i < deathPoolSize; i++)
        {
            var obj = Instantiate(deathSplashPrefab, transform);
            obj.SetActive(false);
            deathPool.Enqueue(obj);
        }

        for (int i = 0; i < hitPoolSize; i++)
        {
            var obj = Instantiate(hitSplashPrefab, transform);
            obj.SetActive(false);
            hitPool.Enqueue(obj);
        }
    }

    public void PlayDeathSplash(Vector3 position)
    {
        PlaySplash(deathPool, deathSplashPrefab, position);
    }

    public void PlayHitSplash(Vector3 position)
    {
        PlaySplash(hitPool, hitSplashPrefab, position);
    }

    private void PlaySplash(Queue<GameObject> pool, GameObject prefab, Vector3 position)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        obj.transform.position = position;
        obj.SetActive(true);

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        StartCoroutine(ReturnToPoolAfterDelay(obj, 0.5f, pool));
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay, Queue<GameObject> pool)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
