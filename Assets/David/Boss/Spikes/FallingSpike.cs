using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject breakEffectPrefab;

    private ObjectPool<GameObject> pool;

    public void Init(ObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerHealth>().TakeDamage(damage);
            }
            BreakSpike();
        }
    }

    private void BreakSpike()
    {
        Debug.Log("Returning spike to pool.");

        // Call centralized audio method
        AudioManager.Instance.PlaySpikeHit();

        // Play break effect
        if (breakEffectPrefab)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // Return to pool
        pool.Release(gameObject);
    }
}
