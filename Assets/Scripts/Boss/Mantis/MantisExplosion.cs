using UnityEngine;

public class MantisExplosion : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;
    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, 1f); // Auto-destroy after explosion animation
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
        }
    }
    private void hideSprite()
    {
        SpriteRenderer.enabled = false;
    }
}
