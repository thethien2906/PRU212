using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MantisHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private Slider healthSlider;
    public delegate void OnHealthChanged();
    public event OnHealthChanged HealthUpdated;
    public UnityEvent onDamaged;
    public UnityEvent onHealthBelowHalf;
    public UnityEvent onDeath;
    private Flash flashEffect;

    private bool isHalfHealthTriggered = false;
    private void Awake()
    {
        flashEffect = GetComponent<Flash>();
    }
    void Start()
    {
        currentHealth = maxHealth;

        // Configure slider range
        if (healthSlider)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.gameObject.SetActive(false);
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider)
            healthSlider.value = currentHealth;  // Set the actual health value

        Debug.Log("Health: " + currentHealth + "/" + maxHealth + " (Slider: " + healthSlider.value + ")");
        HealthUpdated?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        if (flashEffect != null)
        {
            Debug.Log("Flashed");
            flashEffect.FlashSprite();
        }
        onDeath.Invoke();
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (!isHalfHealthTriggered && currentHealth <= maxHealth / 2)
        {
            isHalfHealthTriggered = true;
            onHealthBelowHalf.Invoke();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ShowHealthBar()
    {
        if (healthSlider)
            healthSlider.gameObject.SetActive(true); // Call this via animation event
    }
    private void Die()
    {
        if (flashEffect != null)
        {
            flashEffect.FlashSprite();
        }
        onDeath.Invoke();

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("isDead");
            StartCoroutine(DestroyAfterAnimation(2f)); // Adjust time based on death animation length
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}