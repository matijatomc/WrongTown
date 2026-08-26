using UnityEngine;

public class Motorcycle : MonoBehaviour
{
    public GameObject winScreen;

    public void Ride()
    {
        Debug.Log("Player escaped Wrong Town!");

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
    }
}