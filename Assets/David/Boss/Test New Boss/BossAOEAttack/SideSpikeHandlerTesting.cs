using System.Collections;
using UnityEngine;

public class SideSpikeHandlerTesting : MonoBehaviour
{
    [SerializeField] private GameObject spikeLeft;
    [SerializeField] private GameObject spikeRight;
    [SerializeField] private float sideSpikesInterval = 2f;
    [SerializeField] private bool enableSideSpikes = true;

    private Coroutine spikeRoutine;
    public bool IsEnabled => enableSideSpikes;

    public void Initialize()
    {
        if (spikeLeft != null) spikeLeft.SetActive(false);
        if (spikeRight != null) spikeRight.SetActive(false);
    }

    public void StartSideSpikes()
    {
        if (enableSideSpikes)
        {
            spikeRoutine = StartCoroutine(ToggleSideSpikesLoop());
        }
    }

    public void StopSideSpikes()
    {
        if (spikeRoutine != null)
        {
            StopCoroutine(spikeRoutine);
            ToggleSideSpikes(false);
        }
    }

    private IEnumerator ToggleSideSpikesLoop()
    {
        while (true)
        {
            ToggleSideSpikes(true);
            yield return new WaitForSeconds(sideSpikesInterval);
            ToggleSideSpikes(false);
            yield return new WaitForSeconds(sideSpikesInterval);
        }
    }

    private void ToggleSideSpikes(bool state)
    {
        ToggleSingleSpike(spikeLeft, state, "Spike_Left");
        ToggleSingleSpike(spikeRight, state, "Spike_Right");
    }

    private void ToggleSingleSpike(GameObject spike, bool state, string name)
    {
        if (spike == null) return;

        spike.SetActive(state);

        var spikeScript = spike.GetComponent<Spike>();
        if (spikeScript != null)
        {
            spikeScript.SetColliderEnabled(state);
        }
        else
        {
            Debug.LogError($"Spike script not found on {name}.");
        }
    }
}
