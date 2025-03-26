using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask destroyLayers; // Assign "Ground" and "Wall" layers in Inspector

    private Rigidbody2D rb;
    private int direction = 1; // Default to right

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Destroy if it doesn't hit anything
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        transform.localScale = new Vector3(dir, 1, 1); // Flip if needed
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(speed * direction, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & destroyLayers) != 0)
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
            } else if (collision.CompareTag("SpecialAttack"))
            {
                Destroy(gameObject);
            }
        }
    }
}
