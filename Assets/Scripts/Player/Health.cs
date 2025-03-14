using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    [SerializeField] private Slider healthSlider;

    public delegate void OnHealthChanged();
    public event OnHealthChanged HealthUpdated;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider)
            healthSlider.value = (float)currentHealth / maxHealth;
        HealthUpdated?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            GetComponent<Player>().Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("AddHealth"))
        {
            Heal(20);
        }
    }
}

