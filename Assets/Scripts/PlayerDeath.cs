using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    public HealthSystem healthSystem;
    public GameObject deathScreen;

    private PlayerController playerController;
    private Shooting shooting;
    private Rigidbody rb;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        shooting = GetComponent<Shooting>();
        rb = GetComponent<Rigidbody>();

        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        if (healthSystem != null)
        {
            healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (shooting != null)
        {
            shooting.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}