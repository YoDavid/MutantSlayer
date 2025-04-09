using System.Collections.Generic;
using UnityEngine;

public class AOESpikeControllerTesting : MonoBehaviour
{
    [SerializeField] private List<GameObject> spikeWaves; // 0 & 1, 2 & 3, 4 & 5 = 3 waves (6 spikes)

    public void Initialize()
    {
        foreach (var spike in spikeWaves)
        {
            if (spike != null) spike.SetActive(false);
        }
    }

    public void ToggleSpikeWave(int waveIndex, bool state)
    {
        int start = waveIndex * 2;
        for (int i = start; i < start + 2 && i < spikeWaves.Count; i++)
        {
            if (spikeWaves[i] != null)
            {
                spikeWaves[i].SetActive(state);

                var spikeScript = spikeWaves[i].GetComponent<Spike>();
                if (spikeScript != null)
                {
                    spikeScript.SetColliderEnabled(state);
                }
                else
                {
                    Debug.LogError($"Missing Spike script on spike {i + 1}");
                }
            }
        }
    }
}
