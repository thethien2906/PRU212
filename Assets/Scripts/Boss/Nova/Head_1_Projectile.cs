using UnityEngine;

public class Head_1_Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 direction;
    private float speed;

    private void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector2 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;
    }

    private void Update()
    {
        // Move in the set direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if collided with player
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.PlaySFX(59);
            // Deal damage to player
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Destroy projectile
            Destroy(gameObject);
        }
    }
}