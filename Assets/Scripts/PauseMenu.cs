using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                if (Time.timeScale > 0f)
                {
                    PauseGame();
                }
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}