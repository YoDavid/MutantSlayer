using UnityEngine;

public class ForceMouseHidden : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = UIManager.Instance;
        }
    }

    private void Update()
    {
        if (uiManager == null)
            return;

        bool isPauseMenuVisible = uiManager.pauseMenu != null && uiManager.pauseMenu.IsVisible;
        bool isGameOverMenuVisible = uiManager.gameOverMenu != null && uiManager.gameOverMenu.IsVisible;

        if (isPauseMenuVisible || isGameOverMenuVisible)
        {
            UnlockAndShowMouse();
        }
        else
        {
            LockAndHideMouse();
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
