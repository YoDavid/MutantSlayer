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
            currentHealing--;
            healingIcons[currentHealing].enabled = false; 
        }
    }

    public void GainHealing()
    {
        if (CanGainHeal)
        {
            healingIcons[currentHealing].enabled = true; 
            currentHealing++;
        }
    }


    public int GetCurrentHealingCount() => currentHealing;
}
