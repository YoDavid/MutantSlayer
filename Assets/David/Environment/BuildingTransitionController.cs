using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTransitionZoneController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("List of all building overlays to enable/disable together.")]
    public List<GameObject> buildingOverlays = new List<GameObject>();

    public float activationDelay = 0.1f;

    private Coroutine transitionRoutine;
    private bool shouldShowOverlay = true;

    public void SetOverlayState(bool show)
    {
        shouldShowOverlay = show;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(UpdateOverlayWithDelay());
    }

    private IEnumerator UpdateOverlayWithDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        foreach (GameObject overlay in buildingOverlays)
        {
            if (overlay != null)
                overlay.SetActive(shouldShowOverlay);
        }
    }
}
