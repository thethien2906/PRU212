using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    [SerializeField] private Slider healthSlider;

    public delegate void OnHealthChanged();
    public event OnHealthChanged HealthUpdated;

    private Dictionary<string, Action> itemActions;
    private DifficultyType gameDifficulty;

    private void Start()
    {
        UpdateGameDifficulty();
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
    private void UpdateGameDifficulty()
    {
        DifficultyManager difficultyManager = DifficultyManager.instance;

        if (difficultyManager != null)
            gameDifficulty = difficultyManager.difficulty;
    }

    public void TakeDamage(int damage)
    {
        float modifiedDamage = damage;

        switch (gameDifficulty)
        {
            case DifficultyType.Easy:
                modifiedDamage = damage * 0.1f; // Reduce damage by half
                break;
            case DifficultyType.Normal:
                modifiedDamage = damage * 0.5f;
                break;
            case DifficultyType.Hard:
                break;
        }

        // Convert back to integer (round down)
        int finalDamage = Mathf.FloorToInt(modifiedDamage);

        // Ensure minimum damage of 1 (optional)
        finalDamage = Mathf.Max(1, finalDamage);

        // Apply the modified damage
        currentHealth -= finalDamage;
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

