using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlayBloodParticlesSound : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {  
        AudioManager.Instance.PlayBloodParticlesDeathSound();
    }

}