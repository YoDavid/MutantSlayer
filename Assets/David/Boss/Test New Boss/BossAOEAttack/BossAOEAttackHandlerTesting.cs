using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAOEAttackHandlerTesting : MonoBehaviour
{
    [Header("Spike Logic")]
    [SerializeField] private AOESpikeControllerTesting spikeController;
    [SerializeField] private SideSpikeHandlerTesting sideSpikeHandler;

    [Header("Timing")]
    [SerializeField] private float spikeActiveDuration = 0.8f;
    [SerializeField] private float initialDelay = 1f;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    private void Start()
    {
        cameraShake = FindAnyObjectByType<CameraShake>();
        spikeController?.Initialize();
        sideSpikeHandler?.Initialize();
    }

    public void ActivateAOEAttack()
    {
        StartCoroutine(HandleAOEAttack());
    }

    private IEnumerator HandleAOEAttack()
    {
        yield return new WaitForSeconds(initialDelay);

        if (sideSpikeHandler != null && sideSpikeHandler.IsEnabled)
        {
            sideSpikeHandler.StartSideSpikes();
        }

        for (int wave = 0; wave < 3; wave++)
        {
            spikeController.ToggleSpikeWave(wave, true);
            cameraShake?.ShakeCameraAOEAttack();
            yield return new WaitForSeconds(spikeActiveDuration);
            spikeController.ToggleSpikeWave(wave, false);
        }

        if (sideSpikeHandler != null)
        {
            sideSpikeHandler.StopSideSpikes();
        }
    }
}
