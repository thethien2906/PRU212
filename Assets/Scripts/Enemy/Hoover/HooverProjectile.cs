using UnityEngine;

public class HooverProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; 
    [SerializeField] private int damage = 1; 
    [SerializeField] private LayerMask whatIsPlayer; 

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Auto-destroy after a set time
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsPlayer) != 0)
        {
            if (collision.CompareTag("Player"))
            {
                Health playerHealth = collision.GetComponent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(20);
                    Debug.Log("Projectile hit player! Dealing 20 damage.");
                }
                Destroy(gameObject);
            }

            Destroy(gameObject); // Destroy projectile on impact
        }
        // Optional: destroy if it hits the ground layer
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
