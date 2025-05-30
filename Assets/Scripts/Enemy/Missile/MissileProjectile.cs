using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20);
            }
            Destroy(gameObject);
        }
    }
}
