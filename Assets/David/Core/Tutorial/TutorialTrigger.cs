using UnityEngine;
using System.Collections.Generic;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialContent[] tutorialPages;
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private bool showOnStart = false;

    private bool hasShown = false;

    private void Start()
    {
        if (showOnStart && tutorialPages.Length > 0)
        {
            TriggerTutorial();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasShown && tutorialPages.Length > 0)
        {
            Debug.Log("collision with player");
            TriggerTutorial();
        }
    }

    public void TriggerTutorial()
    {
        if (UIManager.Instance != null && tutorialPages.Length > 0)
        {
            UIManager.Instance.ShowTutorial(new List<TutorialContent>(tutorialPages));
            hasShown = showOnlyOnce;
        }
    }
}