using System.Collections;
using UnityEngine;

public class ShowImageOnTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    public GameObject[] imagesToShow;
    public float hideDelay = 3f;

    private Coroutine disableImagesCoroutine;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Hide all images at the start
        if (imagesToShow != null)
        {
            foreach (GameObject image in imagesToShow)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("One of the Image To Show elements is null on " + gameObject.name);
                }
            }
        }
        else
        {
            Debug.LogError("Images To Show array is not assigned on " + gameObject.name);
        }

        // Find the player's health component
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && imagesToShow != null)
        {
            // Cancel any pending disable coroutine
            if (disableImagesCoroutine != null)
            {
                StopCoroutine(disableImagesCoroutine);
                disableImagesCoroutine = null;
            }

            // Show all images
            foreach (GameObject image in imagesToShow)
            {
                if (image != null)
                {
                    image.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && imagesToShow != null && (playerHealth == null || !playerHealth.isDead))
        {
            // Check if this GameObject is active before running coroutine
            if (!gameObject.activeInHierarchy)
            {
                DisableImagesImmediately(); // If inactive, hide immediately
                return;
            }

            // If the other object is tagged "Tutorial," hide immediately
            if (this.gameObject.CompareTag("Tutorial"))
            {
                DisableImagesImmediately();
            }
            else // Otherwise, hide with delay
            {
                DisableImagesWithDelay();
            }
        }
    }

    private void DisableImagesWithDelay()
    {
        if (!gameObject.activeInHierarchy) // Prevent coroutine if inactive
        {
            DisableImagesImmediately();
            return;
        }

        if (disableImagesCoroutine != null)
        {
            StopCoroutine(disableImagesCoroutine);
        }
        disableImagesCoroutine = StartCoroutine(DisableImagesAfterDelay(hideDelay));
    }

    private IEnumerator DisableImagesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisableImagesImmediately();
    }

    private void DisableImagesImmediately()
    {
        foreach (GameObject image in imagesToShow)
        {
            if (image != null)
            {
                image.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.isDead && imagesToShow != null && imagesToShow.Length > 0 && imagesToShow[0].activeSelf)
        {
            DisableImagesWithDelay();
        }
    }
}