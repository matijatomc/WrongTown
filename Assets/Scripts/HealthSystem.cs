using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 100f;

    private float currentHP;
    private bool isDead = false;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        Debug.Log(
            gameObject.name +
            " je primio štetu. Trenutni HP: " +
            currentHP
        );

        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " died.");

        OnDeath?.Invoke();

        // Normal enemy
        if (CompareTag("Character"))
        {
            Destroy(gameObject);
        }

        // Player se NE uništava.
        // PlayerDeath skripta æe prikazati death screen.
        else if (CompareTag("Player"))
        {
            Debug.Log("Player died - waiting for PlayerDeath.");
        }
    }
}