using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public HealthSystem playerHealth;
    public Slider healthSlider;

    private void Start()
    {
        if (playerHealth == null || healthSlider == null)
        {
            Debug.LogWarning("HealthBarUI references are missing!");
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = playerHealth.MaxHP;
        healthSlider.value = playerHealth.CurrentHP;

        playerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float currentHP, float maxHP)
    {
        healthSlider.maxValue = maxHP;
        healthSlider.value = currentHP;
    }
}