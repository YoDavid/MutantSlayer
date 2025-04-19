using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIHealingController : MonoBehaviour
{
    public Image[] healingIcons; 
    [SerializeField] private int currentHealing = 3;

    public bool CanHeal => currentHealing > 0;
    public bool CanGainHeal => currentHealing < healingIcons.Length;

    public void UseHealing()
    {
        if (CanHeal)
        {
            StartCoroutine(HandleHealingSequence(0.1f));
        }
    }

    private IEnumerator HandleHealingSequence(float delay)
    {
        AudioManager.Instance.PlayHealingGrunt();
        yield return new WaitForSeconds(delay);

        currentHealing--;
        AudioManager.Instance.PlayHealingSound();
        healingIcons[currentHealing].enabled = false;
    }

    public void GainHealing()
    {
        if (CanGainHeal)
        {
            AudioManager.Instance.PlayCreatingHeal();
            healingIcons[currentHealing].enabled = true; 
            currentHealing++;
        }
    }


    public int GetCurrentHealingCount() => currentHealing;
}
