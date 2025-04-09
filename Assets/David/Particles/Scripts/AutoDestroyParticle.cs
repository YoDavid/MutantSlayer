using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemAutoDestroy : MonoBehaviour
{
    private ParticleSystem ps;

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Ground"))
        {
            Debug.Log("Particle hit ground!");
            // Spawn blood splatter decal, play sound, etc.
        }
    }

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        Destroy(gameObject);
    }
}