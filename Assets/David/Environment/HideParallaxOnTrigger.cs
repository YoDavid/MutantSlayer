using System.Collections;
using UnityEngine;

public class HideParallaxOnTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject parallax;  // The parallax object to hide
    public GameObject player;    // Assign player manually (optional)

    [Header("Settings")]
    public float delay = 1.5f;   // Time before hiding (trigger-based)
    public float hideThresholdX; // If player.x > this, hide immediately

    private void Update()
    {
        if (player != null && player.transform.position.x > hideThresholdX)
        {
            parallax.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = player;
        if (isPlayer)
        {
            StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        parallax.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(hideThresholdX, -100, 0),
            new Vector3(hideThresholdX, 100, 0)
        );
    }
}