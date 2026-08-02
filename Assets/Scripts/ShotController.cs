using UnityEngine;

public class ShotController : MonoBehaviour
{
    public void Shot()
    {
        if (gameObject.CompareTag("Character"))
        {
            TakeDamage();
        }
        else
        {
            Debug.LogWarning("Object does not have a recognized tag for shot behavior.");
        }
    }

    void TakeDamage()
    {
        HealthSystem healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(25);
        }
    }
}
