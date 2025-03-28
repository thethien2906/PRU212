using UnityEngine;

public class HeadDamageHandler : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageToPlayer = 10;
    [SerializeField] private int damageFromPlayerAttack = 10;
    [SerializeField] private int damageFromSpecialAttack = 30;

    // References
    private Head1Controller head1Controller;
    private Head2Controller head2Controller;
    private int headType = 0; // 1 for Head1, 2 for Head2

    private void Awake()
    {
        // Try to get the head controller references
        head1Controller = GetComponent<Head1Controller>();
        if (head1Controller != null)
        {
            headType = 1;
        }
        else
        {
            head2Controller = GetComponent<Head2Controller>();
            if (head2Controller != null)
            {
                headType = 2;
            }
        }

        // If no head controller found, log a warning
        if (headType == 0)
        {
            Debug.LogWarning("HeadDamageHandler: No head controller found on this GameObject!");
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Check if colliding with player
        if (other.CompareTag("Player"))
        {
            // Deal damage to player
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }
        }
        // Check if hit by player's regular attack
        else if (other.CompareTag("PlayerAttack"))
        {
            AudioManager.instance.PlaySFX(42);
            Mana playerMana = other.transform.root.GetComponent<Mana>();
            // Apply damage to the appropriate head
            if (headType == 1 && head1Controller != null)
            {
                head1Controller.TakeDamage(damageFromPlayerAttack);
                playerMana.GainManaOnHit(30);

            }
            else if (headType == 2 && head2Controller != null)
            {
                head2Controller.TakeDamage(damageFromPlayerAttack);
                playerMana.GainManaOnHit(10);
            }
        }
        // Check if hit by player's special attack
        else if (other.CompareTag("SpecialAttack"))
        {
            // Apply special attack damage to the appropriate head
            if (headType == 1 && head1Controller != null)
            {
                head1Controller.TakeDamage(damageFromSpecialAttack);
            }
            else if (headType == 2 && head2Controller != null)
            {
                head2Controller.TakeDamage(damageFromSpecialAttack);
            }
        }
    }
}