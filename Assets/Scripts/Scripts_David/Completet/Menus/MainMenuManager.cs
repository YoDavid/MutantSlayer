using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Scene_PauseMenu_Test_David");
    }

    public void LoadGame()
    {
        // Code to open the savings menu.
    }

    public void OpenOptions()
    {
        // Code to open the options menu.
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
