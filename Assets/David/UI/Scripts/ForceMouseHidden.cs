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
        if (Cursor.visible || Cursor.lockState != CursorLockMode.Locked)
        {
            // LockAndHideMouse();
            // UnlockAndShowMouse();
        }
    }

    public void LockAndHideMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void UnlockAndShowMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
