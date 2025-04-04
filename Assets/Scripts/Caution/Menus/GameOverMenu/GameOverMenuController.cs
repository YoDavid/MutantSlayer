using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuController : BaseMenuController
{
    [SerializeField] private PlayerHealth playerHealth;
    public bool IsVisible { get; private set; }

    protected override void Start()
    {
        base.Start(); // Call base initialization
        playerHealth.OnDeath += ShowGameOverMenu;
        SetVisible(false); // Start hidden
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
        gameObject.SetActive(visible);
        if (visible)
        {
            SelectButton(0);
            AudioManager.Instance.PlaySFX("game_over");
        }
    }

    private void ShowGameOverMenu()
    {
        Time.timeScale = 0f;
        SetVisible(true);
    }

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        AudioManager.Instance.PlayButtonClick();
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("MainMenu");
        Debug.Log("GameOver -> MainMenu transition started");
    }
}