using UnityEngine;
using UnityEngine.UI;

public class TutorialAnimator : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.3f)
            .setEase(LeanTweenType.easeOutBack);
    }
}