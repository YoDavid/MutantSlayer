using UnityEngine;

public class BuildingTransitionTrigger : MonoBehaviour
{
    public bool disablesOverlay;

    private BuildingTransitionZoneController zoneController;

    private void Awake()
    {
        zoneController = GetComponentInParent<BuildingTransitionZoneController>();
        if (zoneController == null)
        {
            Debug.LogError("BuildingTransitionZoneController not found in parent!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        bool showOverlay = !disablesOverlay;
        zoneController.SetOverlayState(showOverlay);
    }
}
