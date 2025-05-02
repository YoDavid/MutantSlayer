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

    [SerializeField] private GameObject rockParticlesPrefab;

    [Header("Falling Spikes")]
    [SerializeField] private FallingSpikeSpawner fallingSpikeSpawner;

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
        fallingSpikeSpawner = FindAnyObjectByType<FallingSpikeSpawner>();
    }

    public void ActivateAOEAttack()
    {
        StartCoroutine(HandleAOEAttack());
        if (enableSideSpikes && sideSpikesCoroutine == null)
        {
            sideSpikesCoroutine = StartCoroutine(HandleSideSpikes());  // Start side spikes coroutine
        }
    }


    private IEnumerator HandleAOEAttack()
    {
        yield return new WaitForSeconds(initialDelay);

        // Wave 1: Spawn 1 spike
        ToggleSpikes(0, 1, true);
        fallingSpikeSpawner?.SpawnSpikes1();  // Triggers Wave 1 with 1 spike
        cameraShake?.ShakeCameraAOEAttack();
        AudioManager.Instance.PlayAOEAttackBoss();
        yield return new WaitForSeconds(spikeActiveDuration);
        ToggleSpikes(0, 1, false);

        // Wave 2: Spawn 2 spikes
        ToggleSpikes(2, 3, true);
        fallingSpikeSpawner?.SpawnSpikes2();  // Triggers Wave 2 with 2 spikes
        cameraShake?.ShakeCameraAOEAttack();
        AudioManager.Instance.PlayAOEAttackBoss();
        yield return new WaitForSeconds(spikeActiveDuration);
        ToggleSpikes(2, 3, false);

        // Wave 3: Spawn 3 spikes
        ToggleSpikes(4, 5, true);
        fallingSpikeSpawner?.SpawnSpikes3();  // Triggers Wave 3 with 3 spikes
        cameraShake?.ShakeCameraAOEAttack();
        AudioManager.Instance.PlayAOEAttackBoss();
        yield return new WaitForSeconds(spikeActiveDuration);
        ToggleSpikes(4, 5, false);

        // Cleanup
        if (enableSideSpikes && sideSpikesCoroutine != null)
        {
            StopCoroutine(sideSpikesCoroutine);
            ToggleSideSpikes(false);
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

            if (state && rockParticlesPrefab != null)
            {
                Collider2D spikeCollider = spikes[index1].GetComponent<Collider2D>();
                if (spikeCollider != null)
                {
                    Vector2 spawnPos = new Vector2(spikeCollider.bounds.center.x, spikeCollider.bounds.min.y + 2f);
                    Instantiate(rockParticlesPrefab, spawnPos, Quaternion.identity);
                }
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

            if (state && rockParticlesPrefab != null)
            {
                Collider2D spikeCollider = spikes[index2].GetComponent<Collider2D>();
                if (spikeCollider != null)
                {
                    Vector2 spawnPos = new Vector2(spikeCollider.bounds.center.x, spikeCollider.bounds.min.y + 2f);
                    Instantiate(rockParticlesPrefab, spawnPos, Quaternion.identity);
                }
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