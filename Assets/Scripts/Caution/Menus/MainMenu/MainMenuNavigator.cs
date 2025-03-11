using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigator : MonoBehaviour
{
    private void Update()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(1);
        }
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
