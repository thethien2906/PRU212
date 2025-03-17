using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mana : MonoBehaviour
{
    [SerializeField] private int maxMana = 100;
    private int currentMana;
    [SerializeField] private Slider manaSlider;

    public delegate void OnManaChanged();
    public event OnManaChanged ManaUpdated;

    private Dictionary<string, Action> itemActions;

    private void Start()
    {
        currentMana = maxMana;
        UpdateManaUI();
        // Initialize item actions like mana potion etc.
        itemActions = new Dictionary<string, Action>
        {
            { "addMana", () => RestoreMana(50) },
            { "consumeMana", () => ConsumeMana(100) }
            // Add more if needed
        };
    }

    private void UpdateManaUI()
    {
        if (manaSlider)
            manaSlider.value = (float)currentMana / maxMana;
        ManaUpdated?.Invoke();
    }

    public void ConsumeMana(int amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0);
        UpdateManaUI();
    }

    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        UpdateManaUI();
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }

    public void UseItem(string itemName)
    {
        if (itemActions.ContainsKey(itemName))
        {
            itemActions[itemName].Invoke();
        }
    }
}
