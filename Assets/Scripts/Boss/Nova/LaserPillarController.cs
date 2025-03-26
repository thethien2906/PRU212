using UnityEngine;

public class LaserPillar : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D damageCollider;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 25;
    [SerializeField] private string playerTag = "Player";

    private bool isDamaging = false;

    private void Awake()
    {
        // Get components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        if (damageCollider == null)
            damageCollider = GetComponent<Collider2D>();

        // Ensure collider starts disabled
        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    // Set to warning state (pre-attack visual)
    public void SetWarningState()
    {
        if (animator != null)
            animator.SetBool("isDamaging", false);

        if (damageCollider != null)
            damageCollider.enabled = false;

        isDamaging = false;
    }

    // Set to damaging state
    public void SetDamagingState()
    {
        if (animator != null)
            animator.SetBool("isDamaging", true);

        if (damageCollider != null)
            damageCollider.enabled = true;

        isDamaging = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only deal damage if in damaging state
        if (!isDamaging) return;

        // Check if it's the player
        if (other.CompareTag(playerTag))
        {
            // Get player health component and apply damage
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

            }
        }
    }
}

