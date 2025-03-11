using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject optionsMenuUI; // Add reference to options menu
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        Time.timeScale = 1f; // Resume the game
        isPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Pause the game
        isPaused = true;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }

    public void LoadGame()
    {
        // Placeholder for loading game logic
        // For example: Load saved data from PlayerPrefs or a save file
        Debug.Log("Loading game...");
        // You can call a save/load manager here
        // Example: SaveManager.Load();
        Resume(); // After loading, resume the game
    }

    public void SaveGame()
    {
        // Placeholder for saving game logic
        // For example: Save data to PlayerPrefs or a save file
        Debug.Log("Saving game...");
        // Example: SaveManager.Save();
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false); // Hide pause menu
        optionsMenuUI.SetActive(true); // Show options menu
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false); // Hide options menu
        pauseMenuUI.SetActive(true); // Show pause menu again
    }
}
