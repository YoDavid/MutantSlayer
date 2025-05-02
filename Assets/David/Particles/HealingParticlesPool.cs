using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingParticlesPool : MonoBehaviour
{
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int poolSize = 5;

    private Queue<GameObject> particlePool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(particlePrefab, transform);
            obj.SetActive(false);
            particlePool.Enqueue(obj);
        }
    }


    public void PlayParticles(Vector3 position)
    {
        GameObject particle = particlePool.Count > 0 ? particlePool.Dequeue() : Instantiate(particlePrefab, transform);
        particle.transform.position = position;
        particle.SetActive(true);

        var ps = particle.GetComponent<ParticleSystem>();
        ps?.Play();

        StartCoroutine(DeactivateAfterDelay(particle, 0.5f));
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        particlePool.Enqueue(obj);
    }
}
