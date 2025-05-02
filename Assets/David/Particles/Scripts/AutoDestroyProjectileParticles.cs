using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyProjectileParticles : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        //AudioManager.Instance.PlayBloodParticlesDeathSound();
    }

    private void OnParticleSystemStopped()
    {
        Destroy(gameObject);
    }
}
