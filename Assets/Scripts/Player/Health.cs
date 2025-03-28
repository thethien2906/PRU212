using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10000;
    private int currentHealth;
    [SerializeField] private Slider healthSlider;

    public delegate void OnHealthChanged();
    public event OnHealthChanged HealthUpdated;

    private Dictionary<string, Action> itemActions;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        // Initialize the dictionary with item actions
        itemActions = new Dictionary<string, Action>
        {
            { "addBlood", () => Heal(20) },
            { "minusBlood", () => TakeDamage(20) }
            // Add more items and their actions here
        };
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
        if (itemActions.TryGetValue(collision.tag, out Action action))
        {
            action.Invoke();
            collision.gameObject.SetActive(false);
            Debug.Log($"{collision.tag} action executed");
        }
    }
}

