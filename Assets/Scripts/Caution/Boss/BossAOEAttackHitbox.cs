using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAOEAttack : MonoBehaviour
{
    [Header("Spikes Settings")]
    [SerializeField] private List<GameObject> spikes;
    [SerializeField] private GameObject spikeLeft; 
    [SerializeField] private GameObject spikeRight; 
    [SerializeField] private float spikeActiveDuration = 0.8f; 
    [SerializeField] private float initialDelay = 1f; 

    [Header("Side Spikes Settings")]
    [SerializeField] private bool enableSideSpikes = true; 
    [SerializeField] private float sideSpikesInterval = 2f; 
    [SerializeField] private bool sideSpikesActive = false; 
    private Coroutine sideSpikesCoroutine;

    [Header("Camera Shake")]
    private CameraShake cameraShake;

    private void Start()
    {
        foreach (var spike in spikes)
        {
            spike.SetActive(false);
        }

        if (spikeLeft != null) spikeLeft.SetActive(false);
        if (spikeRight != null) spikeRight.SetActive(false);

        cameraShake = FindAnyObjectByType<CameraShake>();
        if (cameraShake == null)
        {
            Debug.LogError("CameraShake component not found in the scene.");
        }
    }

    public void ActivateAOEAttack()
    {
        StartCoroutine(HandleAOEAttack());
    }

    private IEnumerator HandleAOEAttack()
    {
        yield return new WaitForSeconds(initialDelay);

        if (enableSideSpikes)
        {
            sideSpikesCoroutine = StartCoroutine(HandleSideSpikes());
        }

        // Wave 1: Activate Spike 1 and 2
        ToggleSpikes(0, 1, true); // Enable Spike 1 and 2
        cameraShake?.ShakeCameraAOEAttack(); // Trigger camera shake
        yield return new WaitForSeconds(spikeActiveDuration); // Wait for 0.8 seconds
        ToggleSpikes(0, 1, false); // Disable Spike 1 and 2

        // Wave 2: Activate Spike 3 and 4
        ToggleSpikes(2, 3, true); // Enable Spike 3 and 4
        cameraShake?.ShakeCameraAOEAttack(); // Trigger camera shake
        yield return new WaitForSeconds(spikeActiveDuration); // Wait for 0.8 seconds
        ToggleSpikes(2, 3, false); // Disable Spike 3 and 4

        // Wave 3: Activate Spike 5 and 6
        ToggleSpikes(4, 5, true); // Enable Spike 5 and 6
        cameraShake?.ShakeCameraAOEAttack(); // Trigger camera shake
        yield return new WaitForSeconds(spikeActiveDuration); // Wait for 0.8 seconds
        ToggleSpikes(4, 5, false); // Disable Spike 5 and 6

        // Stop the side spikes timer after the attack sequence ends
        if (enableSideSpikes && sideSpikesCoroutine != null)
        {
            StopCoroutine(sideSpikesCoroutine);
            ToggleSideSpikes(false); // Ensure side spikes are turned off
        }
    }

    private IEnumerator HandleSideSpikes()
    {
        while (true)
        {
            ToggleSideSpikes(true);
            sideSpikesActive = true;
            yield return new WaitForSeconds(sideSpikesInterval);

            ToggleSideSpikes(false);
            sideSpikesActive = false;
            yield return new WaitForSeconds(sideSpikesInterval);
        }
    }

    private void ToggleSpikes(int index1, int index2, bool state)
    {
        if (index1 < spikes.Count && spikes[index1] != null)
        {
            spikes[index1].SetActive(state);
            Spike spikeScript = spikes[index1].GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetColliderEnabled(state);
            }
            else
            {
                Debug.LogError($"Spike script not found on Spike {index1 + 1}");
            }
        }

        if (index2 < spikes.Count && spikes[index2] != null)
        {
            spikes[index2].SetActive(state);
            Spike spikeScript = spikes[index2].GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetColliderEnabled(state);
            }
            else
            {
                Debug.LogError($"Spike script not found on Spike {index2 + 1}");
            }
        }
    }

    private void ToggleSideSpikes(bool state)
    {
        if (spikeLeft != null)
        {
            spikeLeft.SetActive(state);
            Spike spikeScript = spikeLeft.GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetColliderEnabled(state);
            }
            else
            {
                Debug.LogError("Spike script not found on Spike_Left.");
            }
        }

        if (spikeRight != null)
        {
            spikeRight.SetActive(state);
            Spike spikeScript = spikeRight.GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.SetColliderEnabled(state);
            }
            else
            {
                Debug.LogError("Spike script not found on Spike_Right.");
            }
        }
    }
}