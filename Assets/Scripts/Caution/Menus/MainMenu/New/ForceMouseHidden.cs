using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMouseHidden : MonoBehaviour
{
    private void Start()
    {
        LockAndHideMouse();
    }

    private void Update()
    {
        // Continuously enforce in case other systems try to show it
        if (Cursor.visible || Cursor.lockState != CursorLockMode.Locked)
        {
            LockAndHideMouse();
        }
    }

    private void LockAndHideMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
